using UnityEngine;

namespace KOOP
{
    public static class MathUtility
    {
        /// <owner>Lazlo</owner>
        /// <summary>
        /// Lerp from <see cref="a" /> to <see cref="b" /> by interpolator <see cref="t" />,
        /// and it the resulting value is within <see cref="threshold" /> of <see cref="b"/>,
        /// returns <see cref="b"/>.
        /// This is useful when using lerp as an animation decay method where <see cref="t"/>
        /// is delta time to ensure that the result eventually reaches the target when it is close enough.
        /// </summary>
        public static float LerpThatSettles(float a, float b, float t, float threshold)
        {
            var l = Mathf.Lerp(a, b, t);
            
            if (Mathf.Abs(l - b) <= threshold)
            {
                return b;
            }
            else
            {
                return l;
            }
        }
    }
}