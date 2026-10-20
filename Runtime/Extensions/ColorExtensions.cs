using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KOOP.Extensions
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Returns this color's hex color code with only the RGB values.
        /// </summary>
        /// <param name="color">The color to get the hex code for.</param>
        /// <remarks>
        /// Note that HDR colors will be compressed into the 0-1 value range by dividing each
        /// component by the value of the brightest component (ignoring alpha). So an HDR color
        /// with values of <c>(2.0, 1.0, 0.5, 1.0)</c> will be interpreted as <c>(1.0, 0.5, 0.25, 1.0)</c>.
        /// </remarks>
        /// <seealso cref="HexCodeRGBA"/>
        public static string HexCodeRGB(this Color color)
        {
            float maxComponent = Mathf.Max(color.r, color.g, color.b);
            if (maxComponent > 1)
                color /= maxComponent;
            return $"#{RoundColorComponent(color.r):x2}{RoundColorComponent(color.g):x2}{RoundColorComponent(color.b):x2}";
        }

        /// <summary>
        /// Returns this color's hex color code with all RGBA values.
        /// </summary>
        /// <param name="color">The color to get the hex code for.</param>
        /// <remarks><inheritdoc cref="HexCodeRGB"/></remarks>
        /// <seealso cref="HexCodeRGB"/>
        public static string HexCodeRGBA(this Color color)
        {
            // this should handle HDR colors the same as HexCodeRGB but should clamp the alpha value to 0-1 before getting its hex code
            return $"{color.HexCodeRGB()}{RoundColorComponent(color.a, clamp: true):x2}";
        }
        
        /// <summary>
        /// The hex color code corresponding to this single channel value (0-1)
        /// </summary>
        public static string HexCodeComponent(float colorComponent)
        {
            return $"#{RoundColorComponent(colorComponent, clamp: true):x2}";
        }

        /// <summary>
        /// Converts a color component's typical 0-1 range to a 0-255 integer for generating hex color codes.
        /// </summary>
        private static int RoundColorComponent(float colorComponent, bool clamp = false)
        {
            if (clamp)
            {
                return Mathf.RoundToInt(Mathf.Clamp01(colorComponent) * 255);
            }
            
            return Mathf.RoundToInt(colorComponent * 255);
        }
        
        /// <summary>
        /// Returns a copy of <paramref name="color"/> with a specific alpha value.
        /// </summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        /// <summary>
        /// Returns a copy of <paramref name="color"/> with a multiplied alpha value.
        /// </summary>
        public static Color MultipliedAlpha(this Color color, float alphaMultiplier)
        {
            color.a *= alphaMultiplier;
            return color;
        }

        /// <summary>
        /// Returns a copy of <paramref name="color"/> with its values inverted.
        /// </summary>
        /// <param name="color">The color to invert.</param>
        /// <param name="invertAlpha">Should the alpha be inverted as well?</param>
        /// <returns></returns>
        public static Color Invert(this Color color, bool invertAlpha = false)
        {
            return new Color(1 - color.r, 1 - color.g, 1 - color.b, invertAlpha ? 1 - color.a : color.a);
        }

        /// <summary>
        /// Calculates the relative luminance value of a color.
        /// </summary>
        /// <seealso href="https://www.w3.org/WAI/WCAG21/Understanding/contrast-minimum.html#dfn-relative-luminance"/>
        public static float GetRelativeLuminance(this Color color)
        {
            float CorrectChannelValue(float value) => value <= 0.03928f ? value / 12.92f : Mathf.Pow((value + 0.055f) / 1.055f, 2.4f);
            color.r = CorrectChannelValue(color.r);
            color.g = CorrectChannelValue(color.g);
            color.b = CorrectChannelValue(color.b);

            return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
        }

        /// <summary>
        /// Gets the contrast ratio between two colours.
        /// </summary>
        /// <seealso href="https://www.w3.org/WAI/WCAG21/Understanding/contrast-minimum.html#dfn-contrast-ratio"/>
        public static float ContrastRatio(this Color colorA, Color colorB)
        {
            return colorA.ContrastRatio(colorB.GetRelativeLuminance());
        }

        /// <summary>
        /// Gets the contrast ratio between a color and a contrasting luminance value.
        /// </summary>
        /// <seealso href="https://www.w3.org/WAI/WCAG21/Understanding/contrast-minimum.html#dfn-contrast-ratio"/>
        public static float ContrastRatio(this Color color, float contrastingLuminance)
        {
            float colorLuminance = color.GetRelativeLuminance();
            return (Mathf.Max(colorLuminance, contrastingLuminance) + 0.05f) / (Mathf.Min(colorLuminance, contrastingLuminance) + 0.05f);
        }

        /// <summary>
        /// Takes a <paramref name="color"/>, converts it to the LCH color space
        /// and increases then decreases its luminance until it has at least a
        /// minimum contrast ratio when shown on top of <paramref name="contrastingColor"/>.
        /// </summary>
        public static Color IncreaseContrastAgainst(this Color color, Color contrastingColor, float minimumContrastRatio = 3.5f) => color.IncreaseContrastAgainst(contrastingColor.GetRelativeLuminance(), minimumContrastRatio);

        /// <summary>
        /// Takes a <paramref name="color"/>, converts it to the LCH color space
        /// and increases then decreases its luminance until it has at least a
        /// minimum contrast ratio against a luminance value of <paramref name="contrastingLuminance"/>.
        /// </summary>
        public static Color IncreaseContrastAgainst(this Color color, float contrastingLuminance, float minimumContrastRatio = 3.5f)
        {
            // if the colour is already above the minimum contrast ratio, return it unchanged
            if (color.ContrastRatio(contrastingLuminance) >= minimumContrastRatio)
                return color;

            (float luminance, float chroma, float hue) = GetLCHValues(color);

            float luminanceStepSize = 2.5f;

            float newLuminance = luminance;
            Color newColor;
            do
            {
                // shift the luminance
                newLuminance += luminanceStepSize;

                if (newLuminance < 0 || newLuminance > 100)
                {
                    // if out of range, start again in the other direction
                    luminanceStepSize *= -1;
                    newLuminance = luminance + luminanceStepSize;
                }

                newColor = ColorFromLCH(newLuminance, chroma, hue);
            } while (newColor.ContrastRatio(contrastingLuminance) < minimumContrastRatio);

            return newColor;
        }

        /// <summary>
        /// Get LCH color space values for a Unity RGB color.
        /// </summary>
        /// <remarks>
        /// Unity uses the sRGB color space, and LCH (Lightness, Chroma, Hue) is a color space
        /// based in the CIELAB color space and which is designed to be perceptually uniform.
        /// </remarks>
        private static (float lightness, float chroma, float hue) GetLCHValues(Color color)
        {
            // slowly convert from unity's sRGB to LCH colour space via sRGB -> XYZ -> LAB -> LCH

            // sRGB -> XYZ
            float rgbR = color.r;
            float rbgG = color.g;
            float rgbB = color.b;
            rgbR = rgbR > 0.04045f ? Mathf.Pow((rgbR + 0.055f) / 1.055f, 2.4f) : rgbR / 12.92f;
            rbgG = rbgG > 0.04045f ? Mathf.Pow((rbgG + 0.055f) / 1.055f, 2.4f) : rbgG / 12.92f;
            rgbB = rgbB > 0.04045f ? Mathf.Pow((rgbB + 0.055f) / 1.055f, 2.4f) : rgbB / 12.92f;

            float xyzX = rgbR * 0.4124f + rbgG * 0.3576f + rgbB * 0.1805f;
            float xyzY = rgbR * 0.2126f + rbgG * 0.7152f + rgbB * 0.0722f;
            float xyzZ = rgbR * 0.0193f + rbgG * 0.1192f + rgbB * 0.9505f;

            // XYZ -> LAB
            xyzX /= 0.95047f;
            xyzZ /= 1.08883f;

            xyzX = xyzX > 0.008856f ? Mathf.Pow(xyzX, 1f / 3f) : 7.787f * xyzX + 16f / 116f;
            xyzY = xyzY > 0.008856f ? Mathf.Pow(xyzY, 1f / 3f) : 7.787f * xyzY + 16f / 116f;
            xyzZ = xyzZ > 0.008856f ? Mathf.Pow(xyzZ, 1f / 3f) : 7.787f * xyzZ + 16f / 116f;

            float labL = (116 * xyzY) - 16;
            float labA = 500 * (xyzX - xyzY);
            float labB = 200 * (xyzY - xyzZ);

            // LAB -> LCH
            float hr = Mathf.Atan2(labB, labA);
            float hue = hr * 360f / 2f / Mathf.PI;

            if (hue < 0)
                hue += 360;
            float chroma = Mathf.Sqrt(labA * labA + labB * labB);
            float lightness = labL;

            return (lightness, chroma, hue);
        }

        /// <summary>
        /// Creates a Unity RGB color (sRGB color space) from LCH color space values.
        /// </summary>
        private static Color ColorFromLCH(float lightness, float chroma, float hue)
        {
            // slowly convert from LCH to sRGB colour space via LCH -> LAB -> XYZ -> sRGB

            // LCH -> LAB
            float hr = hue / 360f * 2f * Mathf.PI;
            float labA = chroma * Mathf.Cos(hr);
            float labB = chroma * Mathf.Sin(hr);
            float labL = lightness;

            // LAB -> XYZ
            float xyzY = (labL + 16) / 116f;
            float xyzX = labA / 500f + xyzY;
            float xyzZ = xyzY - labB / 200f;

            float xyzY2 = Mathf.Pow(xyzY, 3);
            float xyzX2 = Mathf.Pow(xyzX, 3);
            float xyzZ2 = Mathf.Pow(xyzZ, 3);

            xyzY = xyzY2 > 0.008856f ? xyzY2 : (xyzY - 16f / 116f) / 7.787f;
            xyzX = xyzX2 > 0.008856f ? xyzX2 : (xyzX - 16f / 116f) / 7.787f;
            xyzZ = xyzZ2 > 0.008856f ? xyzZ2 : (xyzZ - 16f / 116f) / 7.787f;

            xyzX *= 0.95047f;
            xyzY *= 1.00f;
            xyzZ *= 1.08883f;

            // XYZ -> sRGB
            float rgbR = xyzX * 3.2406f + xyzY * -1.5372f + xyzZ * -0.4986f;
            float rgbG = xyzX * -0.9689f + xyzY * 1.8758f + xyzZ * 0.0415f;
            float rgbB = xyzX * 0.0557f + xyzY * -0.2040f + xyzZ * 1.0570f;

            rgbR = rgbR > 0.0031308f ? 1.055f * Mathf.Pow(rgbR, 1.0f / 2.4f) - 0.055f : rgbR * 12.92f;
            rgbG = rgbG > 0.0031308f ? 1.055f * Mathf.Pow(rgbG, 1.0f / 2.4f) - 0.055f : rgbG * 12.92f;
            rgbB = rgbB > 0.0031308f ? 1.055f * Mathf.Pow(rgbB, 1.0f / 2.4f) - 0.055f : rgbB * 12.92f;

            rgbR = Mathf.Clamp01(rgbR);
            rgbG = Mathf.Clamp01(rgbG);
            rgbB = Mathf.Clamp01(rgbB);

            return new(rgbR, rgbG, rgbB);
        }

#if UNITY_EDITOR
        /// <summary>
        /// This is a janky way to access the editor's playmode tint color but currently there is no other way :)
        /// </summary>
        public static Color GetEditorPlayModeTint()
        {
            var tintString = UnityEditor.EditorPrefs.GetString("Playmode tint", "");
            if (string.IsNullOrEmpty(tintString))
            {
                return Color.white;
            }

            var sep1 = tintString.IndexOf(';', 0);
            var sep2 = tintString.IndexOf(';', sep1 + 1);
            var sep3 = tintString.IndexOf(';', sep2 + 1);
            var sep4 = tintString.IndexOf(';', sep3 + 1);

            return new(
                (float)double.Parse(tintString.Substring(sep1 + 1, sep2 - sep1 - 1), System.Globalization.CultureInfo.InvariantCulture),
                (float)double.Parse(tintString.Substring(sep2 + 1, sep3 - sep2 - 1), System.Globalization.CultureInfo.InvariantCulture),
                (float)double.Parse(tintString.Substring(sep3 + 1, sep4 - sep3 - 1), System.Globalization.CultureInfo.InvariantCulture),
                (float)double.Parse(tintString.Substring(sep4 + 1), System.Globalization.CultureInfo.InvariantCulture)
            );
        }

        public static Color IncreaseContrastAgainstEditorBackground(Color color, float contrastRatio = 4.5f)
        {
            // console background colours
            Color playModeTint = ColorExtensions.GetEditorPlayModeTint();
            Color backgroundLight =
                EditorGUIUtility.isProSkin ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.79f, 0.79f, 0.79f);
            Color backgroundDark =
                EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.78f, 0.78f, 0.78f);
            Color backgroundLightTinted = backgroundLight * playModeTint;
            Color backgroundDarkTinted = backgroundDark * playModeTint;

            float highestContrastingBackgroundLuminance = Mathf.Max(backgroundLight.GetRelativeLuminance(),
                backgroundDark.GetRelativeLuminance(), backgroundLightTinted.GetRelativeLuminance(),
                backgroundDarkTinted.GetRelativeLuminance());

            return color.IncreaseContrastAgainst(highestContrastingBackgroundLuminance, contrastRatio);
        }
#endif
    }
}