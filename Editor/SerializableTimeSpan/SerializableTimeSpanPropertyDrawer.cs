
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using KOOP;

namespace KOOPEditor
{
    [CustomPropertyDrawer(typeof(SerializableTimeSpan))]
    public class SerializableTimeSpanPropertyDrawer : PropertyDrawer
    {
        private const string _propName_ticks = "_ticks";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			using (new EditorGUI.PropertyScope(position, label, property))
			{
				var ticks = property.FindPropertyRelative(_propName_ticks);

				// Show prefix label
				position = EditorGUI.PrefixLabel(position, label);

				// Get defaults
				int days = 0;
				int hours = 0;
				int minutes = 0;
				int seconds = 0;
				int milliseconds = 0;

				int numberOfValidFields = 5;
				
				// Draw all our fields
				position.width -= (numberOfValidFields - 1) * EditorGUIUtility.standardVerticalSpacing;
				position.width /= numberOfValidFields;
				days = TimeSpanUnitField(position, "d", ticks.longValue, TimeSpan.TicksPerDay, null);

				position.x += position.width + EditorGUIUtility.standardVerticalSpacing;
				hours = TimeSpanUnitField(position, "h", ticks.longValue, TimeSpan.TicksPerHour, TimeSpan.TicksPerDay);
				
				position.x += position.width + EditorGUIUtility.standardVerticalSpacing;
				minutes = TimeSpanUnitField(position, "m", ticks.longValue, TimeSpan.TicksPerMinute, TimeSpan.TicksPerHour);
				
				position.x += position.width + EditorGUIUtility.standardVerticalSpacing;
				seconds = TimeSpanUnitField(position, "s", ticks.longValue, TimeSpan.TicksPerSecond, TimeSpan.TicksPerMinute);
				
				position.x += position.width + EditorGUIUtility.standardVerticalSpacing;
				milliseconds = TimeSpanUnitField(position, "ms", ticks.longValue, TimeSpan.TicksPerMillisecond, TimeSpan.TicksPerSecond);
				
				// Calculate our end value
				ticks.longValue = (days * TimeSpan.TicksPerDay)
					+ (hours * TimeSpan.TicksPerHour)
					+ (minutes * TimeSpan.TicksPerMinute)
					+ (seconds * TimeSpan.TicksPerSecond)
					+ (milliseconds * TimeSpan.TicksPerMillisecond); 
			}
		}

		/// <summary>
		/// Shows an IntField for a specific unit (days, hours, minutes, etc).
		/// </summary>
		private static int TimeSpanUnitField(Rect position,
			string label,
			long totalTicks, 
			long ticksPerUnit, 
			long? parentTicksPerUnit)
		{
			if (parentTicksPerUnit.HasValue)
			{
				// If we have a "parent" unit, we need to only
				// use the relevant ticks (any remaining from the parent).
				totalTicks = totalTicks % parentTicksPerUnit.Value; 
			}

			// Calculate our value in the actual unit
			int valueInUnit = Mathf.FloorToInt(totalTicks / ticksPerUnit); 

			// Show the field
			int outputValue = EditorGUI.IntField(position, valueInUnit);

			// Show the label
			position.x += position.width - 25f;
			position.width = 20f;
			EditorGUI.LabelField(position, label, LabelStyle);

			return outputValue;
		}

		// Label style that is a semi-transparent right-aligned label to exist on top of the
		// text boxes.
		private static GUIStyle _labelStyle;
		private static GUIStyle LabelStyle
		{
			get
			{
				if (_labelStyle == null)
				{
					_labelStyle = new(EditorStyles.label);
					_labelStyle.normal.textColor = new(_labelStyle.normal.textColor.r,
						_labelStyle.normal.textColor.g,
						_labelStyle.normal.textColor.b,
						0.5f);
					_labelStyle.alignment = TextAnchor.MiddleRight;
				}

				return _labelStyle;
			}
		}
    }
}

#endif