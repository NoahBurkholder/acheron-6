// Colorful FX - Unity Asset
// Copyright (c) 2015 - Thomas Hourdel
// http://www.thomashourdel.com

namespace Colorful.Editors
{
	using UnityEngine;
	using UnityEditor;

	[CustomEditor(typeof(Noise))]
	public class NoiseEditor : BaseEffectEditor
	{
		SerializedProperty p_Mode;
		SerializedProperty p_Animate;
		SerializedProperty p_Seed;
		SerializedProperty p_Strength;

		void OnEnable()
		{
			p_Mode = serializedObject.FindProperty("Mode");
			p_Animate = serializedObject.FindProperty("Animate");
			p_Seed = serializedObject.FindProperty("Seed");
			p_Strength = serializedObject.FindProperty("Strength");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(p_Mode);
			EditorGUILayout.PropertyField(p_Animate);

			EditorGUI.BeginDisabledGroup(p_Animate.boolValue);
			{
				EditorGUILayout.PropertyField(p_Seed);
			}
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.PropertyField(p_Strength);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
