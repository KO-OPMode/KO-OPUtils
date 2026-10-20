using System;
using System.Linq;

namespace KOOP.SerializeReferenceExtensions
{
    /// <summary>
    /// Adds string tags to a given type, which can then be filtered via <see cref="TypeTagsFilterAttribute" />.
    /// </summary>
    /// <owner>Lazlo</owner>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public sealed class TypeTagsAttribute : Attribute
    {
        public string[] Tags { get; }

        public TypeTagsAttribute(params string[] tags)
        {
            Tags = tags.ToArray();
        }
    }
}