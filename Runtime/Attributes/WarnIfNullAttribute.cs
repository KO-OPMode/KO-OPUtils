using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace KOOP.Attributes
{
    public class WarnIfNullAttribute : PropertyAttribute
    {
        public WarnIfNullAttribute() 
        { 
            this.WarningMessage = "This property can not be null!";
        }

        public WarnIfNullAttribute(string warningMessage)
        {
            this.WarningMessage = warningMessage;
        }

        public string WarningMessage;
    }
}