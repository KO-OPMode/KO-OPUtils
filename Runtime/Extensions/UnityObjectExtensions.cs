using UnityObject = UnityEngine.Object;

namespace KOOP.Extensions
{
    public static class UnityObjectExtensions
    {
        /// <summary>
        /// Checks whether an object is reference null, or,
        /// if it is a Unity object wrapper, whether it is destroyed, dead, or unassigned.
        /// This is convenient when dealing with interfaces that do not
        /// syntactically employ the Unity null-check operator or bool operator overrides.
        /// </summary>
        public static bool IsUnityNull(this object obj)
        {
            return ReferenceEquals(obj, null) || (obj is UnityObject uo && uo == null);
        }
        /// <summary>
        /// Checks whether a Unity object is destroyed, that is, whether it is not actually reference null,
        /// but only null with the Unity null-check operator.
        /// </summary>
        public static bool IsDestroyed(this UnityObject uo)
        {
            return !ReferenceEquals(uo, null) && (uo == null);
        }

        /// <summary>
        /// Returns a true null if the passed Unity object is destroyed, dead or unassigned,
        /// which then allows for all language coalescing operators to work as expected.
        /// </summary>
        /// <example>
        /// Examples:
        /// Instead of:
        /// if (myObject != null)
        /// {
        ///     myObject.Method();
        /// }
        /// We can call:
        /// myObject.UnityNullable()?.Method();
        /// myObject.n()?.Method();
        /// </example>
        /// <example>
        /// Instead of:
        /// var origin = originOverride != null ? originOverride : transform;
        /// We can call:
        /// var origin = originOverride.UnityNullable() ?? transform;
        /// var origin = originOverride.n() ?? transform;
        /// </example>
        public static T UnityNullable<T>(this T obj) where T : class
        {
            return obj.IsUnityNull() ? null : obj;
        }
        
        /// <inheritdoc cref="UnityNullable{T}"/>
        // ReSharper disable once InconsistentNaming
        public static T n<T>(this T obj) where T : class => obj.UnityNullable();
    }
}