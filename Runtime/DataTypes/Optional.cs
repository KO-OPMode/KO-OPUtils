using System;
using UnityEngine;

namespace KOOP
{
    /// <owner>Lazlo</owner>
    /// <summary>
    /// Represents a serializable and inspectable optional value, akin to Nullable{T}, but Unity-compatible.
    /// In the inspector, will be drawn as a checkbox to determine <see cref="HasValue"/>,
    /// then the <see cref="Value"/> field, if there is indeed a value. 
    /// </summary>
    [Serializable]
    public sealed class Optional<T>
    {
        [SerializeField] private bool _hasValue;

        /// <summary>
        /// Whether the optional has a value.
        /// </summary>
        public bool HasValue
        {
            get => _hasValue;
            set => _hasValue = value;
        }
        
        [SerializeField] private T _value;

        /// <summary>
        /// The value of the optional.
        /// </summary>
        public T Value
        {
            get => _value;
            set => _value = value;
        }
        
        public Optional() { }

        /// <summary>
        /// Initializes a new optional has having no value, but with a default value should it be enabled.
        /// </summary>
        /// <remarks>
        /// This is purposefully implemented as such because it's the most common pattern upon using optionals in code.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Optional turned off by default with a value of 100 if turned on 
        /// [SerializeField] private Optional&lt;float&gt; _healthOverride = 100f; 
        /// </code>
        /// </example>
        public Optional(T value)
        {
            _hasValue = false;
            _value = value;
        }

        public static implicit operator Optional<T>(T value)
        {
            return new Optional<T>(value);
        }

        /// <summary>
        /// The equivalent of the null-coalescing operator.
        /// </summary>
        /// <returns>
        /// Returns <see cref="Value"/> if optional <see cref="HasValue"/> is true, or <see cref="fallback"/> otherwise.
        /// </returns>
        public T Coalesce(T fallback)
        {
            return HasValue ? Value : fallback;
        }
    }
}