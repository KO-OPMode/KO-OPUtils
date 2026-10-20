using System;
using System.Linq;

namespace KOOP.SerializeReferenceExtensions
{
    /// <summary>
    /// When added to a <see cref="SubclassSelectorAttribute"/>, allows allowlisting and/or blocklisting
    /// the available types based on their tags defined via <see cref="TypeTagsAttribute" />. 
    /// </summary>
    /// <owner>Lazlo</owner>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TypeTagsFilterAttribute : Attribute
    {
        public string[] Allowlist { get; set; }
        public string[] Blocklist { get; set; }

        public TypeTagsFilterAttribute(params string[] allowlist)
        {
            Allowlist = allowlist;
        }
    }
}