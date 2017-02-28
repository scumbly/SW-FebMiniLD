using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(NprEffects))]
public class NprEffectsEditor : Editor {
	public override void OnInspectorGUI()
	{
		EditorStyles.label.wordWrap = true;		

		NprEffects effect = target as NprEffects;
		SerializedObject obj = new SerializedObject(effect);

		bool edgesSupported = effect.edgesSupported;
		bool simplificationSupported = effect.simplificationSupported;
		bool shadowsSupported = effect.shadowsSupported;
		bool desaturationSupported = effect.desaturationSupported;
		bool colorTransferSupported = effect.colorTransferSupported;

		var edges = obj.FindProperty("edges");
		var simplify = obj.FindProperty("simplify");
		var simplificationSettings = obj.FindProperty("simplificationSettings");
		var shadowEffects = obj.FindProperty("shadowEffects");
		var shadowSettings = obj.FindProperty("shadowSettings");
		var desaturate = obj.FindProperty("desaturate");
		var edgeSettings = obj.FindProperty("edgeSettings");
        var fadeControlEdges = obj.FindProperty("fadeControlEdges");
        var fadeControlSimplification = obj.FindProperty("fadeControlSimplification");
		var fadeControlDesaturation = obj.FindProperty("fadeControlDesaturation");
		var colorTransfer = obj.FindProperty("colorTransfer");
		var colorTransferSettings = obj.FindProperty("colorTransferSettings");
        bool hasFadeController = (effect.gameObject.GetComponent<NprJointFadeController>() != null);
		bool hasPresetsController = (effect.gameObject.GetComponent<NprPresets>() != null);
		
		obj.Update();

        bool fadedEffect;
		if ( edgesSupported ) {
			EditorGUILayout.PropertyField(edges);
			if ( edges.boolValue ) {
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(edgeSettings,new GUIContent("Edge settings"),true);
    	        fadedEffect = edgeSettings.FindPropertyRelative("fade").boolValue;
	            if (!hasFadeController && fadedEffect)
                	EditorGUILayout.PropertyField(fadeControlEdges, new GUIContent("Fade control"), true);
				--EditorGUI.indentLevel;
			}
		} else {
			EditorGUILayout.LabelField(new GUIContent("Edge effects are not supported by your platform with the current parameter settings."));
		}

		if ( simplificationSupported ) {
			EditorGUILayout.PropertyField(simplify);
			if ( simplify.boolValue ) {
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(simplificationSettings, new GUIContent("Simplification Settings"), true);
            	fadedEffect = simplificationSettings.FindPropertyRelative("fade").boolValue;
            	if (!hasFadeController && fadedEffect)
                	EditorGUILayout.PropertyField(fadeControlSimplification, new GUIContent("Fade control"), true);
				--EditorGUI.indentLevel;
			}
		} else {
			EditorGUILayout.LabelField(new GUIContent("Simplification effects are not supported by your platform with the current parameter settings."));
		}

		if ( shadowsSupported ) {
			EditorGUILayout.PropertyField(shadowEffects, new GUIContent("Shadow Coloring"), true);
			if ( shadowEffects.boolValue ) {
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(shadowSettings, new GUIContent("Shadow Settings"), true);
				--EditorGUI.indentLevel;
			}
		} else {
			EditorGUILayout.LabelField(new GUIContent("Shadow effects are not supported by your platform with the current parameter settings."));
		}

		if ( desaturationSupported ) {
			EditorGUILayout.PropertyField(desaturate);
			if ( desaturate.boolValue && !hasFadeController ) {
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(fadeControlDesaturation,new GUIContent("Fade control"),true);
				--EditorGUI.indentLevel;
			}
		} else {
			EditorGUILayout.LabelField(new GUIContent("Desaturation effects are not supported by your platform with the current parameter settings."));
		}

		if ( colorTransferSupported ) {
			EditorGUILayout.PropertyField(colorTransfer);
			if ( colorTransfer.boolValue ) {
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(colorTransferSettings,new GUIContent("Color transfer settings"),true);
			}
		} else {
			EditorGUILayout.LabelField(new GUIContent("Color transfer effects are not supported by your platform with the current parameter settings."));
		}

		if (!edgesSupported && !simplificationSupported && 
		    !shadowsSupported && !desaturationSupported &&
		    !colorTransferSupported) {
			EditorGUILayout.LabelField(new GUIContent("Sorry! None of the effects are supported by your platform."));
		}

		if (!hasPresetsController) {
			if (GUILayout.Button("Add preset selector script")) {
				effect.gameObject.AddComponent<NprPresets>();
			}
		}

        if (!hasFadeController) {
            if (GUILayout.Button("Add joint fade control script")) {
                effect.gameObject.AddComponent<NprJointFadeController>();
            }
        }

		obj.ApplyModifiedProperties();
	}
}
