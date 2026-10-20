using KOOP.Extensions;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace KOOPEditor
{
    /// <summary>
    /// Provides a scene-view aligned custom pivot rotation for the scene view.
    /// </summary>
    /// <owner>Lazlo</owner>
    [CustomPivot(Name, tooltip = Tooltip, priority = CustomPivotAttribute.defaultPriority + 1)]
    [Icon("SceneViewCamera")]
    public sealed class SceneViewPivotRotation : CustomPivotRotation
    {
        private const string Name = "Scene View";
        private const string Tooltip = "The tool handle is rotated to align with Scene View camera's orientation.";

        public override Quaternion rotation => SceneView.lastActiveSceneView.UnityNullable()?.camera.transform.rotation ?? Quaternion.identity;
    }
}