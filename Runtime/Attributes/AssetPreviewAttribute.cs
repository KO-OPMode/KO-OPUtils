using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KOOP.Attributes
{
	public class AssetPreviewAttribute : PropertyAttribute
	{
		public Vector2 Size = new Vector2(69, 69);

		public AssetPreviewAttribute() { }

		public AssetPreviewAttribute(float width, float height)
		{
			Size.x = width;
			Size.y = height;
		}
	}

	#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(AssetPreviewAttribute))]
	public class AssetPreviewAttributeDrawer : PropertyDrawer
	{
		private Vector2 _previewSize;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			using (EditorGUI.PropertyScope scope = new(position, label, property))
			{
				position.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(position, property, scope.content);

				if (property.objectReferenceValue != null)
				{
					position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
					position.x += EditorGUIUtility.labelWidth;
					position.width = _previewSize.x;
					position.height = _previewSize.y;
					GUI.DrawTexture(position, AssetPreview.GetAssetPreview(property.objectReferenceValue), ScaleMode.ScaleToFit);
				}
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (property.objectReferenceValue == null)
				return EditorGUIUtility.singleLineHeight;

			_previewSize = ((attribute as AssetPreviewAttribute)?.Size ?? Vector2.zero) * EditorGUIUtility.pixelsPerPoint;
			return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + _previewSize.y;
		}
	}

	#endif
}