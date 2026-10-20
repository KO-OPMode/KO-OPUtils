using System;
using System.Linq;
using UnityEngine;

namespace KOOP.Extensions
{
    public static class MaterialExtensions
    {
        public static void SetKeywordEnabled(this Material material, string keyword, bool enabled)
        {
            if (enabled)
            {
                material.EnableKeyword(keyword);
            }
            else
            {
                material.DisableKeyword(keyword);
            }
        }
        
        /// <owner>Lazlo</owner>
        /// <summary>
        /// Gets a cache of lerpable properties for a given material, to be used later with <see cref="LerpSerialized"/>.
        /// </summary>
        public static LerpablePropertyCache GetLerpableProperties(this Material material)
        {
            if (material == null) throw new ArgumentNullException(nameof(material));

            return new LerpablePropertyCache
            (
                intPropertyIDs: material.GetPropertyNames(MaterialPropertyType.Int).Select(Shader.PropertyToID).ToArray(),
                floatPropertyIDs: material.GetPropertyNames(MaterialPropertyType.Float).Select(Shader.PropertyToID).ToArray(),
                vectorPropertyIDs: material.GetPropertyNames(MaterialPropertyType.Vector).Select(Shader.PropertyToID).ToArray()
            );
        }
        
        /// <owner>Lazlo</owner>
        /// <summary>
        /// Lerps ints, floats and vectors from a material using the Set methods, which serializes the values,
        /// unlike when using <see cref="Material.Lerp" />, which only lerps them in memory / GPU.
        /// This is needed in code where you subsequently need to read or copy lerped values, for example in
        /// UI material modification scenarios where a further modifier needs to instantiate from a lerped material.
        /// </summary>
        public static void LerpSerialized(this Material material, LerpablePropertyCache lerpableProperties, Material start, Material end, float t, bool validate = true)
        {
            if (material == null) throw new ArgumentNullException(nameof(material));
            if (start == null) throw new ArgumentNullException(nameof(start));
            if (end == null) throw new ArgumentNullException(nameof(end));
            
            if (lerpableProperties.IntPropertyIDs != null)
            {
                foreach (var intProperty in lerpableProperties.IntPropertyIDs)
                {
                    if (!validate || start.HasInt(intProperty) && end.HasInt(intProperty))
                    {
                        material.SetInt(intProperty, Mathf.RoundToInt(Mathf.Lerp(start.GetInt(intProperty), end.GetInt(intProperty), t)));
                    }
                }
            }
            
            if (lerpableProperties.FloatPropertyIDs != null)
            {
                foreach (var floatProperty in lerpableProperties.FloatPropertyIDs)
                {
                    if (!validate || start.HasFloat(floatProperty) && end.HasFloat(floatProperty))
                    {
                        material.SetFloat(floatProperty, Mathf.Lerp(start.GetFloat(floatProperty), end.GetFloat(floatProperty), t));
                    }
                }
            }
            
            if (lerpableProperties.VectorPropertyIDs != null)
            {
                foreach (var vectorProperty in lerpableProperties.VectorPropertyIDs)
                {
                    if (!validate || start.HasVector(vectorProperty) && end.HasVector(vectorProperty))
                    {
                        material.SetVector(vectorProperty, Vector4.Lerp(start.GetVector(vectorProperty), end.GetVector(vectorProperty), t));
                    }
                }
            }
        }

        public readonly struct LerpablePropertyCache
        {
            public readonly int[] IntPropertyIDs;
            public readonly int[] FloatPropertyIDs;
            public readonly int[] VectorPropertyIDs;

            public LerpablePropertyCache(int[] intPropertyIDs, int[] floatPropertyIDs, int[] vectorPropertyIDs)
            {
                IntPropertyIDs = intPropertyIDs;
                FloatPropertyIDs = floatPropertyIDs;
                VectorPropertyIDs = vectorPropertyIDs;
            }
        }
    }
}