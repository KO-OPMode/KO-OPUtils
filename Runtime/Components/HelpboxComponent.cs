using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace KOOP
{
    [AddComponentMenu("Helpbox")]
    public class HelpboxComponent : MonoBehaviour
    {

#if UNITY_EDITOR
        [SerializeField]
        public string Content;
#endif

    }
}