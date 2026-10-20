#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

using UnityEditorInternal;

public static class InspectorUtils
{
    [MenuItem("CONTEXT/Component/Collapse All Components", false, 100)]
    public static void CollapseAllComponents()
    {   
        Component[] components = null;
        if (Selection.activeTransform != null)
        {
            components = Selection.activeTransform.GetComponents<Component>();
        }
        else if (Selection.activeGameObject != null)
        {
            components = Selection.activeGameObject.GetComponents<Component>();
        }
        else
        {
            Debug.LogWarning("Attempted to collapse components but not selecting anything that can be collapsed! :-(");
            return;
        }
        
        foreach (Component component in components)
        {
            if (component is Transform)
            {
                continue;
            }

            if (component is Renderer)
            {
                var mats = ((Renderer)component).sharedMaterials;
                for (int i = 0; i < mats.Length; ++i)
                {
                    InternalEditorUtility.SetIsInspectorExpanded(mats[i], false);
                }
            }
            InternalEditorUtility.SetIsInspectorExpanded(component, false);
        }
        ActiveEditorTracker.sharedTracker.ForceRebuild();
    }

    [MenuItem("CONTEXT/Component/Collapse All Others", false, 100)]
    public static void CollapseAllOthers(MenuCommand command)
    {   
        Component[] components = null;
        if (Selection.activeTransform != null)
        {
            components = Selection.activeTransform.GetComponents<Component>();
        }
        else if (Selection.activeGameObject != null)
        {
            components = Selection.activeGameObject.GetComponents<Component>();
        }
        else
        {
            Debug.LogWarning("Attempted to collapse components but not selecting anything that can be collapsed! :-(");
            return;
        }
        
        foreach (Component component in components)
        {
            if (component == command.context)
            {
                InternalEditorUtility.SetIsInspectorExpanded(component, true);
                continue;
            }

            if (component is Transform)
            {
                continue;
            }

            if (component is Renderer)
            {
                var mats = ((Renderer)component).sharedMaterials;
                for (int i = 0; i < mats.Length; ++i)
                {
                    InternalEditorUtility.SetIsInspectorExpanded(mats[i], false);
                }
            }
            InternalEditorUtility.SetIsInspectorExpanded(component, false);
        }
        ActiveEditorTracker.sharedTracker.ForceRebuild();
    }

    [MenuItem("CONTEXT/Component/Alphabetize Components", false, 102)]
    public static void AlphabetizeComponents()
    {   
        List<Component> components = new List<Component>();
        if (Selection.activeTransform != null)
        {
            components = new List<Component>(Selection.activeTransform.GetComponents<Component>());
        }
        else if (Selection.activeGameObject != null)
        {
            components = new List<Component>(Selection.activeGameObject.GetComponents<Component>());
        }
        else
        {
            Debug.LogWarning("Attempted to collapse components but not selecting anything that can be collapsed! :-(");
            return;
        }

        components.Sort((x,y) => 
        {
            Type xType = x.GetType();
            string xName = xType.Name;
            Type yType = y.GetType();
            string yName = yType.Name;

            AddComponentMenu[] xAttr = (AddComponentMenu[])xType.GetCustomAttributes(typeof(AddComponentMenu), false);
            if (xAttr.Length > 0)
            {
                string[] xWords = xAttr[0].componentMenu.Split('/');
                if (xWords.Length > 1)
                {
                    xName = xWords[xWords.Length - 1];
                }
            }

            AddComponentMenu[] yAttr = (AddComponentMenu[])yType.GetCustomAttributes(typeof(AddComponentMenu), false);
            if (yAttr.Length > 0)
            {
                string[] yWords = yAttr[0].componentMenu.Split('/');
                if (yWords.Length > 1)
                {
                    yName = yWords[yWords.Length - 1];
                }
            }
            
            return xName.CompareTo(yName);
        });

        foreach (Component component in components)
        {
            if (component is Transform)
            {
                continue;
            }  
            
            while (UnityEditorInternal.ComponentUtility.MoveComponentDown(component))
            {
                continue;
            }
        }
        ActiveEditorTracker.sharedTracker.ForceRebuild();
    }

    [MenuItem("CONTEXT/Component/Alphabetize Components (Ignore AddComponentMenu)", false, 103)]
    public static void AlphabetizeComponentsNoAddComponentMenu()
    {   
        List<Component> components = new List<Component>();
        if (Selection.activeTransform != null)
        {
            components = new List<Component>(Selection.activeTransform.GetComponents<Component>());
        }
        else if (Selection.activeGameObject != null)
        {
            components = new List<Component>(Selection.activeGameObject.GetComponents<Component>());
        }
        else
        {
            Debug.LogWarning("Attempted to collapse components but not selecting anything that can be collapsed! :-(");
            return;
        }

        components.Sort((x,y) => x.GetType().Name.CompareTo(y.GetType().Name));
        foreach (Component component in components)
        {
            if (component is Transform)
            {
                continue;
            }  
            
            while (UnityEditorInternal.ComponentUtility.MoveComponentDown(component))
            {
                continue;
            }
        }
        ActiveEditorTracker.sharedTracker.ForceRebuild();
    }

    [MenuItem("CONTEXT/Component/Expand All Components", false, 101)]
    public static void UncollapseAllComponents()
    {   
        Component[] components = null;
        if (Selection.activeTransform != null)
        {
            components = Selection.activeTransform.GetComponents<Component>();
        }
        else if (Selection.activeGameObject != null)
        {
            components = Selection.activeGameObject.GetComponents<Component>();
        }
        else
        {
            Debug.LogWarning("Attempted to collapse components but not selecting anything that can be collapsed! :-(");
            return;
        }
        
        foreach (Component component in components)
        {
            if (component is Transform)
            {
                continue;
            }

            if (component is Renderer)
            {
                var mats = ((Renderer)component).sharedMaterials;
                for (int i = 0; i < mats.Length; ++i)
                {
                    InternalEditorUtility.SetIsInspectorExpanded(mats[i], true);
                }
            }
            InternalEditorUtility.SetIsInspectorExpanded(component, true);
        }
        ActiveEditorTracker.sharedTracker.ForceRebuild();
    }
    
    private static System.Reflection.MethodInfo _methodInfoRepaintInspectors = null;
    public static void RepaintAllInspectors()
    {
        if (_methodInfoRepaintInspectors == null)
        {
            var inspWin = typeof(EditorApplication).Assembly.GetType("UnityEditor.InspectorWindow");
            _methodInfoRepaintInspectors = inspWin.GetMethod("RepaintAllInspectors", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic );
        }
        _methodInfoRepaintInspectors.Invoke(null, null);
    }
}

#endif
