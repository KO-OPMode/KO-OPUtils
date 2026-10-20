using UnityEngine;
using System;
using System.Runtime.CompilerServices;

namespace KOOP.Extensions
{
    public static class VectorExtensions
    {
        /// <summary>
        /// Returns a copy of <paramref name="vector"/> with its magnitude clamped to <paramref name="minLength"/> as a minimum.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ClampMagnitudeInverse(this Vector3 vector, float minLength)
        {
            float sqrMagnitude = vector.sqrMagnitude;
            if (sqrMagnitude < minLength * minLength)
            {
                float magnitude = (float)Math.Sqrt(sqrMagnitude);
                float xNormalized = vector.x / magnitude;
                float yNormalized = vector.y / magnitude;
                float zNormalized = vector.z / magnitude;
                return new Vector3(xNormalized * minLength, yNormalized * minLength, zNormalized * minLength);
            }
            return vector;
        }

        /// <summary>
        /// Returns a copy of <paramref name="vector"/> with its magnitude clamped between <paramref name="minLength"/> and <paramref name="maxLength"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ClampMagnitude(this Vector3 vector, float minLength, float maxLength)
        {
            float sqrMagnitude = vector.sqrMagnitude;
            if (sqrMagnitude < minLength * minLength)
            {
                float magnitude = (float)Math.Sqrt(sqrMagnitude);
                float xNormalized = vector.x / magnitude;
                float yNormalized = vector.y / magnitude;
                float zNormalized = vector.z / magnitude;
                return new Vector3(xNormalized * minLength, yNormalized * minLength, zNormalized * minLength);
            }
            else if (sqrMagnitude > maxLength * maxLength)
            {
                float magnitude = (float)Math.Sqrt(sqrMagnitude);
                float xNormalized = vector.x / magnitude;
                float yNormalized = vector.y / magnitude;
                float zNormalized = vector.z / magnitude;
                return new Vector3(xNormalized * maxLength, yNormalized * maxLength, zNormalized * maxLength);
            }

            return vector;
        }

        /// <summary>
        /// Returns a copy of <paramref name="vector"/> with each of its components clamped between <paramref name="min"/> and <paramref name="max"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Clamp(this Vector2 vector, Vector2 min, Vector2 max)
        {
            return new Vector2(Mathf.Clamp(vector.x, min.x, max.x), Mathf.Clamp(vector.y, min.y, max.y));
        }

        /// <inheritdoc cref="Clamp(UnityEngine.Vector2,UnityEngine.Vector2,UnityEngine.Vector2)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
        {
            return new Vector3(Mathf.Clamp(vector.x, min.x, max.x), Mathf.Clamp(vector.y, min.y, max.y), Mathf.Clamp(vector.z, min.z, max.z));
        }

        /// <summary>
        /// Given a set of euler angles, returns a comparable set of angles
        /// where each axis is within the range of -180 to 180
        /// </summary>
        public static Vector3 SanitizeEulerAngles(Vector3 eulerAngles)
        {
            while (eulerAngles.x > 180)
                eulerAngles.x -= 360;
            while (eulerAngles.x < -180)
                eulerAngles.x += 360;

            while (eulerAngles.y > 180)
                eulerAngles.y -= 360;
            while (eulerAngles.y < -180)
                eulerAngles.y += 360;

            return eulerAngles;
        }

        /// <summary>
        /// Like a Vector3.Lerp except you can make it snap instantly to the target value (b) when in range
        /// Use to avoid values infinitely lerping to 0.999999x and instead getting to your nice round target values
        /// </summary>
        public static Vector3 LerpWithSnap(this Vector3 a, Vector3 b, float lerp, float snapRange)
        {
            if (Vector3.Magnitude(a - b) < snapRange)
                return b;
            else
                return Vector3.Lerp(a, b, lerp);
        }

        /// <summary>
        /// Like a Vector3.Lerp except you can make it snap instantly to the target value (b) when in range
        /// Use to avoid values infinitely lerping to 0.999999x and instead getting to your nice round target values
        /// </summary>
        public static float MapOut(this Vector2 range, float value, bool clamped = false)
        {
            return clamped ? Mathf.Lerp(range.x, range.y, value) : Mathf.LerpUnclamped(range.x, range.y, value);
        }

        /// <summary>
        /// Returns the square of the distance between two points.
        /// Cheaper than calculating and comparing the full distance
        /// </summary>
        public static float SqrDistance(this Vector3 from, Vector3 to)
        {
            return (from - to).sqrMagnitude;
        }

        /// <summary>
        /// Returns the square of the distance between two points.
        /// Cheaper than calculating and comparing the full distance
        /// </summary>
        public static float SqrDistance(this Vector2 from, Vector2 to)
        {
            return (from - to).sqrMagnitude;
        }

        /// <summary>
        /// Helper method for efficiently checking if you are within a given range using SqDistance()
        /// </summary>
        public static bool WithinRange(this Vector3 from, Vector3 to, float range)
        {
            return from.SqrDistance(to) <= range * range;
        }

        /// <summary>
        /// Helper method for efficiently checking is a vector's magnitude is within the given amount
        /// </summary>
        public static bool WithinRange(this Vector3 vec, float range)
        {
            return vec.sqrMagnitude <= range * range;
        }

        /// <summary>
        /// Helper method for efficiently checking if you are within a given range using SqDistance()
        /// </summary>
        public static bool WithinRange(this Vector2 from, Vector2 to, float range)
        {
            return from.SqrDistance(to) <= range * range;
        }

        /// <summary>
        /// Helper method for efficiently checking is a vector's magnitude is within the given amount
        /// </summary>
        public static bool WithinRange(this Vector2 vec, float range)
        {
            return vec.sqrMagnitude <= range * range;
        }

        /// <summary>
        /// Get the magnitude and direction of a vector with a single square root operation
        /// </summary>
        public static void GetMagnitudeAndDirection(this Vector3 vector, out float magnitude, out Vector3 direction)
        {
            magnitude = vector.magnitude;
            direction = magnitude > Mathf.Epsilon ? vector / magnitude : Vector3.zero;
        }

        /// <summary>
        /// Removes any component of this vector which lies along the given axis. 
        //  Flattens it on the plane that the axis is normal to.
        /// </summary>
        public static Vector3 WithoutAxis(this Vector3 vector, Vector3 axisToRemove)
        {
            return vector -= Vector3.Dot(vector, axisToRemove) * axisToRemove;
        }

        /// <summary>
        /// Returns a vector3 with the X flippes if binary equals 1
        /// </summary>
        public static Vector3 FlipXFromBin(this Vector3 vector, int binary)
        {
            if (binary == 1)
                return new Vector3(-vector.x, vector.y, vector.z);
            else
                return vector;
        }

        /// <summary>
        /// Given a Vector3, returns the angle in degrees of its X/Z vector
        /// </summary>
        public static float HorizontalAngleFromForward(this Vector3 vector)
        {
            var flatVector = new Vector3(vector.x, 0f, vector.z);
            return Vector3.Angle(flatVector, Vector3.forward);
        }

        /// <summary>
        /// Remaps a vector2 between 0-1 over two vector2s A and B
        /// </summary>
        public static Vector2 Remap(this Vector2 value, Vector2 A, Vector2 B)
        {
            return new Vector2(Mathf.LerpUnclamped(A.x, B.x, value.x), Mathf.LerpUnclamped(A.y, B.y, value.y));
        }

        /// <summary>
        /// Can lerp towards a targetvalue, but if the difference between the two is below snap, it'll automatically match target value
        /// </summary>
        /// <param name="value">current vector2 value</param>
        /// <param name="target">value to reach</param>
        /// <param name="lerp">lerp ratio</param>
        /// <param name="snap">snap threshold to automatically match target</param>
        /// <returns></returns>
        public static Vector2 SmoothSnapLerp(this Vector2 value, Vector2 target, float lerp, float snap)
        {
            if ((value - target).magnitude < snap)
                value = target;
            else
                value += (target - value) * Mathf.Clamp01(lerp);

            return value;
        }

        public static Vector2 FlipYIf(this Vector2 value, bool condition)
        {
            if (!condition)
                return value;
            else
                return new Vector2(value.x, -value.y);
        }

        /// <summary>
        /// Moves Vector2 value towards target value, using maxDelta. Set maxDelta to a negative value instantly lerp instead
        /// </summary>
        /// <param name="value">current value</param>
        /// <param name="target">value to reach</param>
        /// <param name="maxDelta">maximum lerping speed, set to negative to reach target value instantly</param>
        /// <returns></returns>
        public static Vector2 MoveTowards(this Vector2 value, Vector2 target, float maxDelta)
        {
            if (maxDelta < 0f)
                return target;
            else
                return new Vector2(Mathf.MoveTowards(value.x, target.x, maxDelta), Mathf.MoveTowards(value.y, target.y, maxDelta));
        }

        /// <summary>
        /// Creates a Vector3 by adding up two Vector3 based on a Vector2's values
        /// </summary>
        /// <param name="value">Vector2 value to use as multiplier</param>
        /// <param name="xVector">Vector3 multiplied by the Vector2's x property</param>
        /// <param name="yVector">Vector3 multiplied by the Vector2's y property</param>
        /// <returns></returns>
        public static Vector3 Composite(this Vector2 value, Vector3 xVector, Vector3 yVector)
        {
            return xVector * value.x + yVector * value.y;
        }

        /// <summary>
        /// Returns a normalized vector only if its magnitude exceeds a given threshold
        /// </summary>
        public static Vector3 NormalizeOverMagnitude(this Vector3 value, float threshold)
        {
            if (value.magnitude >= threshold)
                return value.normalized;
            else
                return value;
        }

        /// <summary>
        /// Returns the normalized, flatten Forward of a transform, and it's perpendicular vector
        /// </summary>
        public static bool TryToGetFlatForwardVectors(this Transform transform, out Vector3 forward, out Vector3 right)
        {
            if (transform != null)
            {
                forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
                right = Quaternion.Euler(Vector3.up * 90f) * forward;

                return true;
            }
            else
            {
                forward = right = Vector3.zero;
                return false;
            }
        }

        /// <summary>
        /// Returns a vector multiplied by another.
        /// </summary>
        public static Vector2 Scaled(this Vector2 v, Vector2 scale)
        {
            v.Scale(scale);
            return v;
        }

        /// <summary>
        /// Returns a vector multiplied by another.
        /// </summary>
        public static Vector3 Scaled(this Vector3 v, Vector3 scale)
        {
            v.Scale(scale);
            return v;
        }

        /// <summary>
        /// Returns the inverse of a vector (1/v).
        /// </summary>
        public static Vector2 Inverse(this Vector2 v)
        {
            return new Vector2(1f / v.x, 1f / v.y);
        }

        /// <summary>
        /// Returns the inverse of a vector (1/v).
        /// </summary>
        public static Vector3 Inverse(this Vector3 v)
        {
            return new Vector3(1f / v.x, 1f / v.y, 1f / v.z);
        }

        /// <summary>
        /// Round up components of a Vector3 at the nearest desired precision (1 by default)
        /// </summary>
        public static Vector3 Round(this Vector3 v, float rounding = 1f)
        {
            return new Vector3(Mathf.Round(v.x / rounding) * rounding,
            Mathf.Round(v.y / rounding) * rounding,
            Mathf.Round(v.z / rounding) * rounding);
        }

        public static float DistanceToLineSegment(this Vector2 v, Vector2 segmentA, Vector2 segmentB, out Vector2 closestPoint)
        {
            // Referencing https://csharphelper.com/howtos/howto_point_segment_distance.html
            float dx = segmentB.x - segmentA.x;
            float dy = segmentB.y - segmentA.y;
            if (Mathf.Approximately(dx, 0f) && Mathf.Approximately(dy, 0f))
            {
                // Our line segment is actually a point
                closestPoint = segmentA;
            }
            else
            {
                // Calculate the t that minimizes the distance.
                float t = ((v.x - segmentA.x) * dx + (v.y - segmentA.y) * dy) / (dx * dx + dy * dy);

                // See if this represents one of the segment's end points or a point in the middle.
                if (t < 0)
                {
                    closestPoint = segmentA;
                }
                else if (t > 1)
                {
                    closestPoint = segmentB;
                }
                else
                {
                    // Lerp along the segment based on t
                    closestPoint = new Vector2(segmentA.x + t * dx, segmentA.y + t * dy);
                }
            }
            
            return Vector2.Distance(v, closestPoint);
        }
    }
}