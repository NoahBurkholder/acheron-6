// Colorful FX - Unity Asset
// Copyright (c) 2015 - Thomas Hourdel
// http://www.thomashourdel.com

namespace Colorful.Editors
{
	using UnityEngine;
	using UnityEditor;
	using ColorMode = FastVignette.ColorMode;

	[CustomEditor(typeof(FastVignette))]
	public class FastVignetteEditor : BaseEffectEditor
	{
		SerializedProperty p_Color;
		SerializedProperty p_Darkness;

		void OnEnable()
		{
			p_Color = serializedObject.FindProperty("Color");
			p_Darkness = serializedObject.FindProperty("Darkness");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(p_Darkness);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
