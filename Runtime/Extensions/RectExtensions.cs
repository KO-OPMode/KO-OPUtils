using UnityEngine;

namespace KOOP.Extensions
{
    public static class RectExtensions
    {
        /// <summary>
        /// Edges are inclusive (i.e. two equal sized bounds encompass one another)
        /// </summary>
        public static bool Encompasses(this Rect parent, Rect child)
        {
            return parent.Contains(child.min) && parent.Contains(child.max);
        }
    }
}