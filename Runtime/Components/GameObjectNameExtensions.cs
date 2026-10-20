using UnityEngine;

namespace KOOP.Extensions
{
    /// <summary>
    /// Helper methods that make it easier to interact with <see cref="GameObjectName"/>
    /// </summary>
    public static class GameObjectNameExtensions
    {
        /// <summary>
        /// Adds a prefix to the given GameObject's name, adding a <see cref="GameObjectName"/> component to hold it if necessary.
        /// </summary>
        public static void SetNamePrefix(this GameObject gameObject, object context, string prefix, int order = 0)
        {
            var gameObjectName = gameObject.GetOrAddComponent<GameObjectName>();
            gameObjectName.SetPrefix(context, prefix, order);
        }

        /// <summary>
        /// Removes a prefix that was added via <see cref="SetNamePrefix"/>
        /// </summary>
        public static void RemoveNamePrefix(this GameObject gameObject, object context)
        {
            var gameObjectName = gameObject.GetOrAddComponent<GameObjectName>();
            gameObjectName.RemovePrefix(context);
        }

        /// <summary>
        /// Adds a prefix from the given GameObject's name, adding a <see cref="GameObjectName"/> component to hold it if necessary.
        /// </summary>
        public static void SetNameSuffix(this GameObject gameObject, object context, string suffix, int order = 0)
        {
            var gameObjectName = gameObject.GetOrAddComponent<GameObjectName>();
            gameObjectName.SetSuffix(context, suffix, order);
        }

        /// <summary>
        /// Removes a suffix that was added via <see cref="SetNameSuffix"/>
        /// </summary>
        public static void RemoveNameSuffix(this GameObject gameObject, object context)
        {
            var gameObjectName = gameObject.GetOrAddComponent<GameObjectName>();
            gameObjectName.RemoveSuffix(context);
        }
    }
}