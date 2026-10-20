using UnityEngine;
using UnityEngine.UI;

namespace KOOP.Extensions
{
    public static class ScrollRectExtensions
    {
        public static float GetVerticalNormalizedPositionToEnsureRectTransformIsVisible(this ScrollRect scrollRect, RectTransform rectTransform, float padding = 0f)
        {
            float normalizedViewportHeight = scrollRect.viewport.rect.height / scrollRect.content.rect.height;
            float normalizedContentOutsideOfHeight = 1f - normalizedViewportHeight;
            float normalizedViewportTop = (1f - scrollRect.verticalNormalizedPosition) * normalizedContentOutsideOfHeight;
            float normalizedViewportBottom = normalizedViewportTop + normalizedViewportHeight;

			Vector2 anchoredPosition = ConvertRectTransformAnchoredPosition(rectTransform, scrollRect.content);
            float elementTop = -(anchoredPosition.y + (rectTransform.rect.height / 2f) + padding);
            float elementBottom = elementTop + rectTransform.rect.height + (padding * 2f);
            float normalizedElementTop = elementTop / scrollRect.content.rect.height;
            float normalizedElementBottom = elementBottom / scrollRect.content.rect.height;

            float previousVerticalNormalizedPosition = scrollRect.verticalNormalizedPosition;
            float output = scrollRect.verticalNormalizedPosition;

            if (normalizedElementTop < normalizedViewportTop)
            {
                output = scrollRect.verticalNormalizedPosition + (normalizedViewportTop - normalizedElementTop) * (1f / normalizedContentOutsideOfHeight);
            }
            else if (normalizedElementBottom > normalizedViewportBottom)
            {
                output = scrollRect.verticalNormalizedPosition + (normalizedViewportBottom - normalizedElementBottom) * (1f / normalizedContentOutsideOfHeight);
            }

            return Mathf.Clamp(output, 0f, 1f);
        }
        
        public static float GetHorizontalNormalizedPositionToEnsureRectTransformIsVisible(this ScrollRect scrollRect, RectTransform rectTransform, float padding = 0f)
        {
            float normalizedViewportWidth = scrollRect.viewport.rect.width / scrollRect.content.rect.width;
            float normalizedContentOutsideOfWidth = 1f - normalizedViewportWidth;
            float normalizedViewportTop = (1f - scrollRect.horizontalNormalizedPosition) * normalizedContentOutsideOfWidth;
            float normalizedViewportBottom = normalizedViewportTop + normalizedViewportWidth;

			Vector2 anchoredPosition = ConvertRectTransformAnchoredPosition(rectTransform, scrollRect.content);
            float elementTop = -(anchoredPosition.x + (rectTransform.rect.width / 2f) + padding);
            float elementBottom = elementTop + rectTransform.rect.width + (padding * 2f);
            float normalizedElementTop = 1f + (elementTop / scrollRect.content.rect.width);
            float normalizedElementBottom = 1f + (elementBottom / scrollRect.content.rect.width);

            float previousHorizontalNormalizedPosition = scrollRect.horizontalNormalizedPosition;
            float output = scrollRect.horizontalNormalizedPosition;

            if (normalizedElementTop < normalizedViewportTop)
            {
                output = scrollRect.horizontalNormalizedPosition + (normalizedViewportTop - normalizedElementTop) * (1f / normalizedContentOutsideOfWidth);
            }
            else if (normalizedElementBottom > normalizedViewportBottom)
            {
                output = scrollRect.horizontalNormalizedPosition + (normalizedViewportBottom - normalizedElementBottom) * (1f / normalizedContentOutsideOfWidth);
            }

            return Mathf.Clamp(output, 0f, 1f);
        }

        public static void EnsureRectTransformIsVisible(this ScrollRect scrollRect, RectTransform rectTransform, float padding = 0f)
        {
            scrollRect.verticalNormalizedPosition = GetVerticalNormalizedPositionToEnsureRectTransformIsVisible(scrollRect, rectTransform, padding);
        }

		public static Vector2 ConvertRectTransformAnchoredPosition(RectTransform from, RectTransform to)
		{
			Vector2 localPoint;
			Vector2 fromPivotDerivedOffset = new Vector2(from.rect.width * from.pivot.x + from.rect.xMin, from.rect.height * from.pivot.y + from.rect.yMin);
			Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, from.position);
			screenP += fromPivotDerivedOffset;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(to, screenP, null, out localPoint);
			return localPoint;
		}
    }
}