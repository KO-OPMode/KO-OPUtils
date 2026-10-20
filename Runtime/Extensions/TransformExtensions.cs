using System;
using System.Collections.Generic;
using UnityEngine;

namespace KOOP.Extensions
{
    public static class TransformExtensions
    {
        /// <summary>
        /// Scales the transform around a specified pivot as opposed to the default pivot of the transform.
        /// </summary>
        public static void ScaleAroundRelative(this Transform target, Vector3 pivot, Vector3 scaleFplayerCharacter)
        {
            // pivot
            var pivotDelta = target.localPosition - pivot;
            pivotDelta.Scale(scaleFplayerCharacter);
            target.localPosition = pivot + pivotDelta;

            // scale
            var finalScale = target.localScale;
            finalScale.Scale(scaleFplayerCharacter);
            target.localScale = finalScale;
        }

        /// <summary>
        /// Alias for Transform.InverseTransformPoint(Vector3).
        /// </summary>
        public static void WorldSpacePointToLocalSpacePoint(this Transform target, Vector3 worldSpacePoint)
        {
            target.InverseTransformPoint(worldSpacePoint);
        }

        /// <summary>
        /// Alias for Transform.InverseTransformPoint(Vector3).
        /// </summary>
        public static void LocalSpacePointToWorldSpacePoint(this Transform target, Vector3 localSpacePoint)
        {
            target.TransformPoint(localSpacePoint);
        }

        /// <summary>
        /// Gathers all of the components of type <typeparamref name="T"/> in the list provided in <see cref="values"/>.
        /// </summary>
        public static void GetComponentsInDirectChildren<T>(this Transform target, List<T> values)
        {
            values.Clear();
            for (var i = 0; i < target.childCount; i++)
            {
                values.AddRange(target.GetChild(i).GetComponents<T>());
            }
        }

        /// <summary>
        /// Places a transform under a parent with default position, rotation, scale
        /// </summary>
        /// <param name="target"></param>
        /// <param name="newParent"></param>
        public static void SetParentedNeutral(this Transform target, Transform newParent)
        {
            target.transform.SetParent(newParent);
            target.transform.localScale = Vector3.one;
            target.transform.localRotation = Quaternion.identity;
            target.transform.localPosition = Vector3.zero;
        }

        /// <summary>
        /// Rotates a transform around a pivot point using a quaternion.
        /// </summary>
        public static void RotateAround(this Transform transform, Vector3 point, Quaternion rotation)
        {
            rotation.ToAngleAxis(out var angle, out var axis);
            transform.RotateAround(point, axis, angle);
        }

        /// <summary>
        /// Returns the level of nesting of the transform.
        /// </summary>
        public static int Depth(this Transform transform)
        {
            var depth = 0;

            while (transform.parent != null)
            {
                depth++;
                transform = transform.parent;
            }

            return depth;
        }

        public static void Rotate(this Transform transform, Quaternion rotation, Space space)
        {
            switch (space)
            {
                case Space.Self:
                    transform.rotation *= rotation;
                    return;
                
                case Space.World:
                    transform.rotation = rotation * transform.rotation;
                    return;
                
                default: throw new ArgumentException(nameof(space));
            }
        }

        public static Quaternion TransformRotation(this Transform transform, Quaternion rotation)
        {
            return transform.rotation * rotation;
        }

        public static Quaternion InverseTransformRotation(this Transform transform, Quaternion rotation)
        {
            return Quaternion.Inverse(transform.rotation) * rotation;
        }
    }
}