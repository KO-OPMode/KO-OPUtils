using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace KOOP.SerializeReferenceExtensions.Editor
{
    public static class TypeImplementationSearch
    {
        private static readonly Dictionary<Type, List<Type>> _typeToImplementingTypes = new();

        public static IEnumerable<Type> GetImplementingTypes(Type type)
        {
            if (type.IsGenericType)
            {
                return GetImplementingTypesWithGeneric(type);
            }
            else
            {
                return GetImplementingTypesUsingTypeCache(type);
            }
        }

        private static IEnumerable<Type> GetImplementingTypesUsingTypeCache(Type baseType)
        {
            return TypeCache.GetTypesDerivedFrom(baseType).Append(baseType).Where(IsValidImplementationType);
        }

        private static IEnumerable<Type> GetImplementingTypesWithGeneric(Type baseType)
        {
            if (_typeToImplementingTypes.TryGetValue(baseType, out var result))
            {
                return result;
            }

            result = new List<Type>();
            Type genericTypeDefinition;
            Type[] targetTypeArguments;
            Type[] genericTypeParameters;

            if (baseType.IsGenericType)
            {
                genericTypeDefinition = baseType.GetGenericTypeDefinition();
                targetTypeArguments = baseType.GetGenericArguments();
                genericTypeParameters = genericTypeDefinition.GetGenericArguments();
            }
            else
            {
                genericTypeDefinition = baseType;
                targetTypeArguments = Type.EmptyTypes;
                genericTypeParameters = Type.EmptyTypes;
            }

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(IsValidImplementationType);

            foreach (var type in types)
            {
                var interfaceTypes = type.GetInterfaces();

                foreach (var interfaceType in interfaceTypes)
                {
                    if (!interfaceType.IsGenericType ||
                        interfaceType.GetGenericTypeDefinition() != genericTypeDefinition)
                    {
                        continue;
                    }

                    var sourceTypeArguments = interfaceType.GetGenericArguments();

                    var allParametersMatch = true;

                    for (var i = 0; i < genericTypeParameters.Length; i++)
                    {
                        var variance = genericTypeParameters[i].GenericParameterAttributes &
                                       GenericParameterAttributes.VarianceMask;

                        var sourceTypeArgument = sourceTypeArguments[i];
                        var targetTypeArgument = targetTypeArguments[i];

                        if (variance == GenericParameterAttributes.Contravariant)
                        {
                            if (!sourceTypeArgument.IsAssignableFrom(targetTypeArgument))
                            {
                                allParametersMatch = false;
                                break;
                            }
                        }
                        else if (variance == GenericParameterAttributes.Covariant)
                        {
                            if (!targetTypeArgument.IsAssignableFrom(sourceTypeArgument))
                            {
                                allParametersMatch = false;
                                break;
                            }
                        }
                        else
                        {
                            if (sourceTypeArgument != targetTypeArgument)
                            {
                                allParametersMatch = false;
                                break;
                            }
                        }
                    }

                    if (allParametersMatch)
                    {
                        result.Add(type);
                        break;
                    }
                }
            }

            _typeToImplementingTypes.Add(baseType, result);
            return result;
        }

        private static bool IsValidImplementationType(Type type)
        {
            return
                (type.IsPublic || type.IsNestedPublic || type.IsNestedPrivate) &&
                !type.IsAbstract &&
                !type.IsGenericType &&
                !typeof(Object).IsAssignableFrom(type) &&
                Attribute.IsDefined(type, typeof(SerializableAttribute));
        }
    }
}