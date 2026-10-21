using UnityEditor;
using UnityEngine;

namespace KOOPEditor
{
    /// <summary>
    /// This class will generate texture assets in the project (replacing them if found).
    /// It's used to replace functions that used to be done in a fragment shader, for the sake of optimization.
    /// (In hindsight it might have been faster to make standalone shaders and read their output out of a rendertexture;
    /// this seemed like the simpler solution at the time.)
    /// Call RegenerateNoiseTextures from the associated MenuItem in order to update the files after changing their associated code.
    /// Currently, the two functions actually in use (OctaveNoise and SmoothNoise) are reproducing code that was formerly found
    /// inside StarrySky_Jupiter.cginc
    /// </summary>
    public static class NoiseTextureGenerator
    {
        
        private static void PseudoShaderMakePng(
            string filename,
            System.Func<Vector2, Vector2, Color> pseudoShader,
            int w, int h,
            Vector2 crossfadeFrac,
            bool crossSmoothstep=false,
            TextureFormat textureFormat=TextureFormat.RGB24)
        {
            Texture2D tex = new Texture2D(w, h, textureFormat, false);

            Color GenerateRawPixel(int x, int y) {
                return pseudoShader(new Vector2(x, y), new Vector2(x, y) / new Vector2(w, h));
            }
            float GetBlendFrac(int a, int e, float frac) {
                float linearCross = 1f - Mathf.Clamp01((float)a / e) / frac;
                if (crossSmoothstep) {
                    return Mathf.SmoothStep(0f,1f,linearCross);
                } else {
                    return linearCross;
                }
            }
            float GetBlendAmtX(int x) {
                return GetBlendFrac(x,w,crossfadeFrac.x);
            }
            float GetBlendAmtY(int y) {
                return GetBlendFrac(y,h,crossfadeFrac.y);
            }
            Color GenerateBlendX(int x, int y) {
                Color mainPixel = GenerateRawPixel(x, y);
                Color crossPixel = GenerateRawPixel(x+w, y);
                return Color.Lerp(mainPixel, crossPixel, GetBlendAmtX(x));
            }
            Color GenerateBlendY(int x, int y) {
                Color mainPixel = GenerateRawPixel(x, y);
                Color crossPixel = GenerateRawPixel(x, y+h);
                return Color.Lerp(mainPixel, crossPixel, GetBlendAmtY(y));
            }

            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    Color curPixel;
                    if (crossfadeFrac.x > 0f && crossfadeFrac.y > 0f) {
                        Color mainPixel = GenerateBlendX(x, y);
                        Color crossPixel = GenerateBlendX(x, y+h);
                        curPixel = Color.Lerp(mainPixel, crossPixel, GetBlendAmtY(y));
                    } else if (crossfadeFrac.x > 0f) {
                        curPixel = GenerateBlendX(x,y);
                    } else if (crossfadeFrac.y > 0f) {
                        curPixel = GenerateBlendY(x,y);
                    } else {
                        curPixel = GenerateRawPixel(x,y);
                    }
                    tex.SetPixel(x,y,curPixel);
                }
            }
            tex.Apply();
            byte[] bytes = ImageConversion.EncodeToPNG(tex);
            DestroyImmediate(tex);
            System.IO.File.WriteAllBytes(Application.dataPath+"/_Data/GeneratedTextures/"+filename+".png", bytes);
        }

        [MenuItem("Assets/KO_OP/Noise Textures/Regenerate Noise Textures")]
        private static void RegenerateNoiseTextures()
        {
            //these two files only exist to produce an output to be inspected by a human
            //to make sure that the generation process is working as expected; commenting them out
            //so as to not waste time generating them when there haven't been recent changes to the setup
            //(but leaving them here for convenience when there *have* been).
            //PseudoShaderMakePng("testGrad", TestPseudoShader, 256, 256, new Vector2(0.1f,0.2f), true);
            //PseudoShaderMakePng("testCheck", TestPseudoShaderCheckered, 256, 256, Vector2.one, false);
            Random.InitState(0);
            PseudoShaderMakePng("octave", OctaveNoise, 256, 256, Vector2.zero);
            PseudoShaderMakePng("smoothNoise", SmoothNoise, 8192, 1, new Vector2(0.1f,0), true);
            PseudoShaderMakePng("tinystardataA", TinyStarNoise, 256, 256, Vector2.zero, false, TextureFormat.ARGB32);
            PseudoShaderMakePng("tinystardataB", TinyStarNoise, 256, 256, Vector2.zero, false, TextureFormat.ARGB32);
            AssetDatabase.Refresh();
        }

        private static Color TestPseudoShader(Vector2 xy, Vector2 uv)
        {
            return new Color(uv.x, uv.y, ((Mathf.FloorToInt(xy.x)%2) == (Mathf.FloorToInt(xy.y)%2))?1f:0f, 1);
        }
        
        private static Color TestPseudoShaderCheckered(Vector2 xy, Vector2 uv)
        {
            return new Color(0, 0, ((Mathf.FloorToInt(xy.x*0.11f)%2) == (Mathf.FloorToInt(xy.y*0.12f)%2))?1f:0f, 1);
        }


        // in hindsight it might have been faster to set up a rendertexture and stuff but whatever
        private static Vector2 RotateVectorByAmt(Vector2 v, float delta)
        {
            return new Vector2(
                v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
            );
        }

        private static float Frac(float n)
        {
            return n - Mathf.Floor(n);
        }
        
        private static Vector2 Frac(Vector2 n)
        {
            return new Vector2(Frac(n.x), Frac(n.y));
        }
        
        private static Vector3 Frac(Vector3 n)
        {
            return new Vector3(Frac(n.x), Frac(n.y), Frac(n.z));
        }
        
        private static float Hash12(Vector2 p)
        {
            Vector3 p3  = Frac(new Vector3(p.x, p.y, p.x) * .1031f);
            p3 += Vector3.one*Vector3.Dot(p3, new Vector3(p3.y,p3.z,p3.x) + Vector3.one*33.33f);
            return Frac((p3.x + p3.y) * p3.z);
        }
        
        private static float BilinearHash12(Vector2 x)
        {
            Vector2 i = new Vector2(Mathf.Floor(x.x),Mathf.Floor(x.y));
            Vector2 f = Frac(x);

            // Four corners in 2D of a tile
            float a = Hash12(i);
            float b = Hash12(i + new Vector2(1f, 0f));
            float c = Hash12(i + new Vector2(0f, 1f));
            float d = Hash12(i + new Vector2(1f, 1f));

            return Mathf.Lerp(
                Mathf.Lerp(a, b, f.x),
                Mathf.Lerp(c, d, f.x),
                f.y
            );
        }

        private static Color OctaveNoise(Vector2 xy, Vector2 uv)
        {
            const float octaveNoiseRotAmt = 0.4f;
            float noiseAmt = 0f;
            xy /= 6f;
            for(int i=0; i<4; i++) {
                xy = RotateVectorByAmt(xy, octaveNoiseRotAmt);
                float oct = Mathf.Pow(2f,i);
                noiseAmt += (BilinearHash12(xy*oct)-0.5f)/4f;// /oct;
                // in typical perlin noise, you'd divide by the octave scale,
                // but I'm looking for more of a "clumpy speckles" than "speckly blobs"
                // and I think this hides the blockiness of the biggest octave better *too*
            }

            noiseAmt = noiseAmt * 0.5f + 0.5f;
            return new Color(noiseAmt, noiseAmt, noiseAmt, 1f);
        }

        private static Color SmoothNoise(Vector2 xy, Vector2 uv)
        {
            float x = xy.x/32f;
            float val = Mathf.Sin( x + Mathf.Sin(x*0.6f) + Mathf.Sin(x*0.2f)*Mathf.Sin(x*2.1f) );
            val = val * 0.5f + 0.5f;
            return new Color(val, val, val, 1);
        }

        private static Color TinyStarNoise(Vector2 xy, Vector2 uv) {
            return new Color(Random.value, Random.value, Random.value, Random.value);
        }

    }
}
