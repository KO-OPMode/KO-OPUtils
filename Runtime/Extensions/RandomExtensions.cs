using System.Runtime.CompilerServices;
using UnityEngine;
using Random = System.Random;

namespace KOOP
{
    /// <summary>
    /// Extension methods for System.Random, mostly designed to give it similar functionality to Unity's Random class
    /// A lot of the logic is based on Unity's own Random.h to provide consistent behavior. However, all ranges
    /// have an exclusive upper bound rather than an inclusive one. (So e.g. when requesting a point inside a sphere,
    /// you will never get a point exactly on the surface of the sphere).
    /// </summary>
    public static class RandomExtensions
    {
        private const float TAU = 2.0f * Mathf.PI;
        
        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NextFloat(this Random random)
        {
            return (float)random.NextDouble();
        }

        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to min, and less than maxExclusive.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NextFloat(this Random random, float min, float maxExclusive)
        {
            float t = random.NextFloat();
            return Mathf.Lerp(min, maxExclusive, t);
        }
        
        /// <summary>
        /// Returns a random point on a circle with radius 1.0
        /// </summary>
        public static Vector2 NextPointOnUnitCircle(this Random random)
        {
            float angle = random.NextFloat(0.0f, TAU);

            float x = Mathf.Cos(angle);
            float y = Mathf.Sin(angle);

            return new Vector2(x, y);
        }

        /// <summary>
        /// Returns a random point inside a circle with radius 1.0
        /// </summary>
        public static Vector2 NextPointInsideUnitCircle(this Random random)
        {
            Vector2 v = random.NextPointOnUnitCircle();
            
            // To ensure a uniform distribution, as the area of the circle increases by (x^2)
            // we have to increase our radius by x^(1/2) (i.e. sqrt)
            v *= Mathf.Sqrt(random.NextFloat());
            return v;
        }
        
        /// <summary>
        /// Returns a random point inside a sphere with radius 1.0
        /// </summary>
        public static Vector3 NextPointInsideUnitSphere(this Random random)
        {
            Vector3 v = random.NextPointOnUnitSphere();
            
            // To ensure a uniform distribution, as the volume of the sphere increases by (x^3)
            // we have to increase our radius by x^(1/3)
            v *= Mathf.Pow(random.NextFloat(), 1.0F / 3.0f);
            return v;
        }
        
        /// <summary>
        /// Returns a random point on a sphere with radius 1.0
        /// </summary>
        public static Vector3 NextPointOnUnitSphere(this Random random)
        {
            float z = random.NextFloat(-1.0f, 1.0f);
            float angle = random.NextFloat(0.0f, TAU);

            float radius = Mathf.Sqrt(1.0f - z * z);

            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);

            return new Vector3(x, y, z);
        }
        
        /// <summary>
        /// Returns a random point inside an axis-aligned bounding box
        /// </summary>
        public static Vector3 NextPointInsideBounds(this Random random, Bounds bounds)
        {
            return bounds.center + NextPointInsideBounds(random, bounds.extents);
        }
        
        /// <summary>
        /// Returns a random point inside an axis-aligned bounding box with the given extents (in each direction)
        /// </summary>
        public static Vector3 NextPointInsideBounds(this Random random, Vector3 extents)
        {
            return new Vector3(
                random.NextFloat(-extents.x, extents.x),
                random.NextFloat(-extents.y, extents.y),
                random.NextFloat(-extents.z, extents.z));
        }

        /// <summary>
        /// Returns a random rotation
        /// </summary>
        public static Quaternion NextRotation(this Random random)
        {
            float x = random.NextFloat(-1.0f, 1.0f);
            float y = random.NextFloat(-1.0f, 1.0f);
            float z = random.NextFloat(-1.0f, 1.0f);
            float w = random.NextFloat(-1.0f, 1.0f);
            Quaternion rotation = new(x, y, z, w);
            rotation.Normalize();

            if (Quaternion.Dot(rotation, Quaternion.identity) < 0.0f)
                return new Quaternion(-rotation.x, -rotation.y, -rotation.z, -rotation.w);
            else
                return rotation;
        }
        
        /// <summary>
        /// Returns a random rotation with uniform distribution
        /// </summary>
        public static Quaternion NextRotationUniform(this Random random)
        {
            // Employs Hopf fibration to uniformly distribute quaternions
            float u1 = random.NextFloat();
            float theta = random.NextFloat(0.0f, TAU);
            float rho = random.NextFloat(0.0f, TAU);

            float i = Mathf.Sqrt(1.0f - u1);
            float j = Mathf.Sqrt(u1);

            // We do not need to normalize the generated quaternion, because the probability density corresponds to the Haar measure.
            // This means that a random rotation is obtained by picking a point at random on S^3, and forming the unit quaternion.
            float x = i * Mathf.Sin(theta);
            float y = i * Mathf.Cos(theta);
            float z = j * Mathf.Sin(rho);
            float w = j * Mathf.Cos(rho);
            
            Quaternion rotation = new(x, y, z, w);

            if (Quaternion.Dot(rotation, Quaternion.identity) < 0.0f)
                return new Quaternion(-rotation.x, -rotation.y, -rotation.z, -rotation.w);
            else
                return rotation;
        }

        /// <summary>
        /// Returns a random color with an alpha of 1.
        /// </summary>
        public static Color NextColorHSV(this Random random)
        {
            return random.NextColorHSV(0.0f, 1f, 0.0f, 1f, 0.0f, 1f, 1f, 1f);
        }

        /// <summary>
        /// Returns a random color between the provided hue values.
        /// The alpha is always 1.
        /// </summary>
        public static Color NextColorHSV(this Random random, float hueMin, float hueMax)
        {
            return random.NextColorHSV(hueMin, hueMax, 0.0f, 1f, 0.0f, 1f, 1f, 1f);
        }

        /// <summary>
        /// Returns a random color between the provided hue and saturation values.
        /// The alpha is always 1.
        /// </summary>
        public static Color NextColorHSV(this Random random, float hueMin, float hueMax, float saturationMin, float saturationMax)
        {
            return random.NextColorHSV(hueMin, hueMax, saturationMin, saturationMax, 0.0f, 1f, 1f, 1f);
        }

        /// <summary>
        /// Returns a random color between the provided hue, saturation, and value values.
        /// The alpha is always 1.
        /// </summary>
        public static Color NextColorHSV(
            this Random random, 
            float hueMin, 
            float hueMax, 
            float saturationMin, 
            float saturationMax,
            float valueMin, 
            float valueMax)
        {
            return random.NextColorHSV(hueMin, hueMax, saturationMin, saturationMax, valueMin, valueMax, 1f, 1f);
        }

        /// <summary>
        /// Returns a random color between the provided values.
        /// </summary>
        public static Color NextColorHSV(
            this Random random,
            float hueMin, 
            float hueMax, 
            float saturationMin, 
            float saturationMax,
            float valueMin, 
            float valueMax, 
            float alphaMin, 
            float alphaMax)
        {
            float hue = random.NextFloat(hueMin, hueMax);
            float saturation = random.NextFloat(saturationMin, saturationMax);
            float value = random.NextFloat(valueMin, valueMax);
            float alpha = random.NextFloat(alphaMin, alphaMax);

            Color color = Color.HSVToRGB(hue, saturation, value);
            color.a = alpha;

            return color;
        }
    }

}
