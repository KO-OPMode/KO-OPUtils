using UnityEngine;
using UnityCollections;

namespace KOOP.Extensions
{
    public static class RigidbodyExtensions
    {
        /// <summary>
        /// Returns the world-space bounds of every collider under this rigidbody
        /// </summary>
        public static Bounds GetCompoundBounds(this Rigidbody rigidbody, bool includeTriggers = true)
        {
            using var colliders = new PooledList<Collider>();
            rigidbody.GetComponentsInChildren(colliders.BackingList);
            if (colliders.Count > 0)
            {
                var bounds = colliders[0].bounds;
                for ( var i = 1; i < colliders.Count; i++ )
                {
                    if (!colliders[i].enabled || colliders[i].GetComponentInParent<Rigidbody>() != rigidbody)
                    {
                        // Ignore disabled colliders and colliders that are part of a child rigidbody
                        continue;
                    }

                    if (includeTriggers || !colliders[i].isTrigger)
                    {
                        bounds.Encapsulate(colliders[i].bounds);
                    }
                }

                return bounds;
            }

            return new Bounds( rigidbody.transform.position, Vector3.zero );
        }
    }
}