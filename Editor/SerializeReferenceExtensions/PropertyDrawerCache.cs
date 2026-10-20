using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace KOOP.SerializeReferenceExtensions.Editor
{
    public static class PropertyDrawerCache
    {
        private static readonly Dictionary<Type, PropertyDrawer> typeToPropertyDrawer = new();

        public static bool TryGetPropertyDrawer(Type type, out PropertyDrawer drawer)
        {
            if (!typeToPropertyDrawer.TryGetValue(type, out drawer))
            {
                var drawerType = GetCustomPropertyDrawerType(type);
                drawer = (drawerType != null) ? (PropertyDrawer)Activator.CreateInstance(drawerType) : null;
                typeToPropertyDrawer.Add(type, drawer);
            }

            return (drawer != null);
        }

        private static Type GetCustomPropertyDrawerType(Type type)
        {
            var interfaceTypes = type.GetInterfaces();

            var types = TypeCache.GetTypesWithAttribute<CustomPropertyDrawer>();
            
            foreach (var drawerType in types)
            {
                var customPropertyDrawerAttributes = drawerType.GetCustomAttributes(typeof(CustomPropertyDrawer), true);
                
                foreach (CustomPropertyDrawer customPropertyDrawer in customPropertyDrawerAttributes)
                {
                    var field = customPropertyDrawer.GetType().GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    if (field != null)
                    {
                        var fieldType = (Type)field.GetValue(customPropertyDrawer);
                        
                        if (fieldType != null)
                        {
                            if (fieldType == type)
                            {
                                return drawerType;
                            }

                            // If the property drawer also allows for being applied to child classes, check if they match
                            var useForChildrenField = customPropertyDrawer.GetType().GetField("m_UseForChildren", BindingFlags.NonPublic | BindingFlags.Instance);
                            
                            if (useForChildrenField != null)
                            {
                                var useForChildrenValue = useForChildrenField.GetValue(customPropertyDrawer);
                                
                                if (useForChildrenValue is bool && (bool)useForChildrenValue)
                                {
                                    // Check interfaces
                                    if (Array.Exists(interfaceTypes, interfaceType => interfaceType == fieldType))
                                    {
                                        return drawerType;
                                    }

                                    // Check derived types
                                    var baseType = type.BaseType;
                                    
                                    while (baseType != null)
                                    {
                                        if (baseType == fieldType)
                                        {
                                            return drawerType;
                                        }

                                        baseType = baseType.BaseType;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}