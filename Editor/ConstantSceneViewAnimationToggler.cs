#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace KOOPEditor
{
	public class ConstantSceneViewAnimationToggler
	{
		[MenuItem("Tools/Toggle constant Scene View animation")]
		public static void OpenWindow()
		{
			var sceneView = EditorWindow.GetWindow<SceneView>(); 
			sceneView.sceneViewState.alwaysRefresh = !sceneView.sceneViewState.alwaysRefresh;
			Debug.Log("Constant scene view animation is now "+(sceneView.sceneViewState.alwaysRefresh?"en":"dis")+"abled.");
		}
	}
}

#endif