using UnityEngine;

namespace KOOP.Extensions
{
    public static class BoundsExtensions
    {
        /// <summary>
        /// Edges are inclusive (i.e. two equal sized bounds encompass one another)
        /// </summary>
        public static bool Encompasses(this Bounds parent, Bounds child)
        {
            return parent.Contains(child.min) && parent.Contains(child.max);
        }
    }
}