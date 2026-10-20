using UnityEngine;
using System.Reflection;

namespace KOOP.Extensions
{
    public static class ColliderExtensions
    {
        public static Bounds ToBounds(this BoxCollider boxCollider)
        {
            return new Bounds(boxCollider.center, boxCollider.size);
        }
    }
}