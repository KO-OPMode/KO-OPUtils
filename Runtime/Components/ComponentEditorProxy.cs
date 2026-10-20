using UnityEngine;

/// <summary>
/// An extremely tiny editor-only class meant for providing a shortcut to an editor for another component.<para/>
/// e.g. You may have a text object three thousand sub-objects deep that you want to be able to edit from the root object. Now you can!
/// </summary>
public class ComponentEditorProxy : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField, HideInInspector] public Component ComponentToProxy;
#endif
}