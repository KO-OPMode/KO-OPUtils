using System;
using UnityEngine;

namespace KOOP.Attributes
{
    /// <owner>Noé</owner>
    /// <summary>
    /// An attribute that invokes a callback method when its field or property is changed in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OnValueChangedAttribute : PropertyAttribute
    {
        public readonly string CallbackName;
        public uint ArrayContentHash;
        public int ArraySize;
        public bool ExecuteInEditMode;
        
        /// <param name="callbackName">The method name of the callback. Recommend using the <c>nameof()</c> function.</param>
        /// <param name="executeInEditMode">Should this callback be executed when the value is changed in edit mode as well as play mode?</param>
        public OnValueChangedAttribute(string callbackName, bool executeInEditMode = true)
        {
            CallbackName = callbackName;
            ExecuteInEditMode = executeInEditMode;
            
            // make sure that this attribute is drawn last, since it shouldn't interfere with any other decoration drawer
            order = int.MinValue;
        }
    }
}