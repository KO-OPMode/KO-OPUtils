using UnityEngine;

namespace KOOP
{
    /// <summary>
    /// A class full of handy utility functions for UI.
    /// </summary>
    public static class RectTransformExtensions
    {

        /// <summary>
        /// Returns a <see cref="Vector4"/> of a child <see cref="RectTransform"/>'s bounds within its parent's bounding box.
        /// </summary>
        /// <param name="child">The child <see cref="RectTransform"/>.</param>
        /// <param name="parent">The parent <see cref="RectTransform"/> of <paramref name="child"/>. If null, this will find the child's parent and cast it to a <see cref="RectTransform"/> if one exists. If not with will return <see cref="Vector4.zero"/>.</param>
        /// <returns>A <see cref="Vector4"/> whose x, y, z, w values are the left, right, top, bottom distances of the child's edges relative to its parent's corresponding edges.</returns>
        public static Vector4 GetInsetFromParentEdge(this RectTransform child, RectTransform parent = null)
        {
            // find the parent if not provided
            if (parent == null)
            {
                parent = child.parent as RectTransform;

                // if the child has no parent then its offset from anything is nothing
                if (parent == null)
                    return Vector4.zero;
            }

            Vector2 parentSize = parent.rect.size;

            // convert from normalized anchor positions to positions relative to the parent size
            Vector2 childRectMin = child.anchorMin * parentSize + child.offsetMin;
            Vector2 childRectMax = child.anchorMax * parentSize + child.offsetMax;

            return new Vector4(
                childRectMin.x,                // left
                parentSize.x - childRectMax.x, // right
                parentSize.y - childRectMax.y, // top
                childRectMin.y                 // bottom
            );
        }

        private static readonly Vector3[] _cornersScratch = new Vector3[4];
        
        public static Rect GetWorldRect(this RectTransform rectTransform)
        {
            lock (_cornersScratch)
            {
                rectTransform.GetWorldCorners(_cornersScratch);
                return new Rect(_cornersScratch[0], _cornersScratch[2] - _cornersScratch[0]);
            }
        }

        public static Rect GetScreenRect(this RectTransform rectTransform, Camera camera = null)
        {
            lock (_cornersScratch)
            {
                rectTransform.GetWorldCorners(_cornersScratch);

                Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
                Vector2 max = new Vector2(float.MinValue, float.MinValue);

                for (var i = 0; i < 4; i++)
                {
                    var screenCorner = RectTransformUtility.WorldToScreenPoint(camera, _cornersScratch[i]);

                    min = Vector2.Min(min, screenCorner);
                    max = Vector2.Max(max, screenCorner);
                }

                var size = max - min;

                return new Rect(min, size);
            }
        }
    }
}