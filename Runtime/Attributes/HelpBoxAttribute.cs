using UnityEngine;
using UnityEditor;

namespace KOOP.Attributes
{
    public enum HelpBoxMessageType { None, Info, Warning, Error }
    public class HelpBoxAttribute : PropertyAttribute 
    {
        public string text;
        public HelpBoxMessageType messageType;
     
        public HelpBoxAttribute(string text, HelpBoxMessageType messageType = HelpBoxMessageType.None) : base(applyToCollection: true)
        {
            this.text = text;
            this.messageType = messageType;
        }
    }
}