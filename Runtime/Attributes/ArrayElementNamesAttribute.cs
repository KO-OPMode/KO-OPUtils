using System;
using UnityEngine;

namespace KOOP.Attributes
{
    /// <summary>
    /// @owner Noé<br/>
    /// An attribute to be put on array or list fields to override the name displayed in the inspector per element.<br/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ArrayElementNamesAttribute : PropertyAttribute
    {
        public readonly string NamingMethodName;
    
        /// <param name="namingMethodName">The name of a method with a single <c>int</c> parameter for the element's index, that returns a <c>string</c> to display instead of "Element 0", "Element 1", etc.</param>
        public ArrayElementNamesAttribute(string namingMethodName)
        {
            NamingMethodName = namingMethodName;
        }
    }
}