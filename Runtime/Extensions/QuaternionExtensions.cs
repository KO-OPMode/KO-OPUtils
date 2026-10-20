using UnityEngine;

namespace KOOP.Extensions
{
    public static class QuaternionExtensions
    {
        /// <summary>
        /// Given a set of euler angles, returns a comparable set of angles
        /// where each axis is within the range of -180 to 180
        /// </summary>
        public static Vector3 GetSanitizedEulerAngles(this Quaternion quaternion)
        {
            return VectorExtensions.SanitizeEulerAngles(quaternion.eulerAngles);
        }

        /// <summary>
        /// Normalizes this quaternion if it does not already have a vector magnitude of 1.
        /// This is more efficient than just calling quaternion.normalized if our data is usually already
        /// normalized, as it lets us avoid an expensive square root operation.
        /// </summary>
        public static Quaternion SafeNormalize(this Quaternion quaternion)
        {
            var sqrMagnitude = quaternion.VectorSqrMagnitude();
            if (sqrMagnitude < Mathf.Epsilon)
            {
                // There is no valid quaternion to return
                return Quaternion.identity;
            }

            if (Mathf.Approximately(sqrMagnitude, 1f))
            {
                // This quaternion is already normalized
                return quaternion;
            }

            return quaternion.normalized;
        }

        /// <summary>
        /// Returns the vector magnitude of the given quaternion
        /// </summary>
        public static float VectorMagnitude(this Quaternion quaternion)
        {
            return (new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w)).magnitude;
        }
        
        /// <summary>
        /// Returns the vector square magnitude of the given quaternion
        /// </summary>
        public static float VectorSqrMagnitude(this Quaternion quaternion)
        {
            return (new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w)).sqrMagnitude;
        }

        /// <summary>
        /// Given a set of euler angles, returns a comparable set of angles
        /// where each axis is within the range of -180 to 180
        /// </summary>

        public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion velRot, float time)
        {
            if (Time.deltaTime < Mathf.Epsilon) 
            return rot;

            // Align target with Rot
            var Dot = Quaternion.Dot(rot, target);
            var Multi = Dot > 0f ? 1f : -1f;
            target.x *= Multi;
            target.y *= Multi;
            target.z *= Multi;
            target.w *= Multi;

            // Smooth damping
            var Result = new Vector4(
                Mathf.SmoothDamp(rot.x, target.x, ref velRot.x, time),
                Mathf.SmoothDamp(rot.y, target.y, ref velRot.y, time),
                Mathf.SmoothDamp(rot.z, target.z, ref velRot.z, time),
                Mathf.SmoothDamp(rot.w, target.w, ref velRot.w, time)
            ).normalized;

            // Keep VelRot tangent
            var derivError = Vector4.Project(new Vector4(velRot.x, velRot.y, velRot.z, velRot.w), Result);
            velRot.x -= derivError.x;
            velRot.y -= derivError.y;
            velRot.z -= derivError.z;
            velRot.w -= derivError.w;

            return new Quaternion(Result.x, Result.y, Result.z, Result.w);
        }

        // https://discussions.unity.com/t/look-rotation-but-using-the-up-axis/897368/5
        public static Quaternion LookRotationLockedUp(Vector3 forward, Vector3 up)
        {
            var q = Quaternion.LookRotation(up, forward); // Purposefully inverted
            var right = Vector3.Cross(forward, up);
            q = Quaternion.AngleAxis(90f, right) * q;
            return q;
        }

        /// <summary>
        /// Calculates the delta between two quaternions.
        /// </summary>
        public static Quaternion Delta(Quaternion from, Quaternion to)
        {
            return to * Quaternion.Inverse(from);
        }

        /// <inheritdoc cref="Delta" />
        public static Quaternion DeltaTo(this Quaternion from, Quaternion to)
        {
            return Delta(from, to);
        }

        /// <inheritdoc cref="Delta" />
        public static Quaternion DeltaFrom(this Quaternion to, Quaternion from)
        {
            return Delta(from, to);
        }

        public static bool IsNonZero(this Quaternion q)
        {
            return q.x != 0f || q.y != 0f || q.z != 0f || q.w != 0f;
        }
    }
}