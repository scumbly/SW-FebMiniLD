using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomPropertyDrawer (typeof(NprEdgeSettings))]
public class NprEdgeSettingsDrawer : PropertyDrawer {

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return 16f + 18f + 18f + 18f + 18f + 
			(drawImageSpaceEdge(property) ? 36f : 0f) +
			(property.FindPropertyRelative("onlyEdges").boolValue ? 36f : 0f) +
			(!property.FindPropertyRelative("useBackgroundColor").boolValue ? 18f : 0f);
	}

	public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label) {
		var onlyEdges = prop.FindPropertyRelative ("onlyEdges");
		var edgeType = prop.FindPropertyRelative ("edgeType");
		var thickness = prop.FindPropertyRelative ("thickness");
		var sharpness = prop.FindPropertyRelative ("sharpness");
		var detailedness = prop.FindPropertyRelative ("detailedness");
		var threshold = prop.FindPropertyRelative ("threshold");
		var useBackgroundColor = prop.FindPropertyRelative ("useBackgroundColor");
		var backGroundTexture = prop.FindPropertyRelative ("backGroundTexture");
		var edgeColor = prop.FindPropertyRelative ("edgeColor");
        var fade = prop.FindPropertyRelative("fade");

		int ystart = 0;
						EditorGUI.PropertyField(new Rect(position.x,position.y+ystart,position.width,16),onlyEdges);
		ystart += 18;	EditorGUI.PropertyField(new Rect(position.x,position.y+ystart,position.width,16),edgeType);
		if ( !useBackgroundColor.boolValue ) {
			ystart += 18;	EditorGUI.PropertyField(new Rect(position.x,position.y+ystart,position.width,16),edgeColor);
		}
		if ( onlyEdges.boolValue ) {
			ystart += 18;	EditorGUI.PropertyField(new Rect(position.x,position.y+ystart,position.width,16),useBackgroundColor,new GUIContent("Draw with rendered color"));
		}
		if ( onlyEdges.boolValue ) {
			ystart += 18;	EditorGUI.PropertyField(new Rect(position.x,position.y+ystart,position.width,16),backGroundTexture,new GUIContent("Background texture"));
		}
		ystart += 18;	EditorGUI.PropertyField(new Rect(position.x,position.y+ystart,position.width,16),thickness);
		ystart += 18;	EditorGUI.PropertyField(new Rect(position.x,position.y+ystart,position.width,16),sharpness);
		if ( drawImageSpaceEdge(prop) ) {
			ystart += 18;	EditorGUI.PropertyField(new Rect(position.x,position.y+ystart,position.width,16),detailedness);
			ystart += 18; EditorGUI.PropertyField(new Rect(position.x,position.y+ystart,position.width,16),threshold);
        }
        ystart += 18; EditorGUI.PropertyField(new Rect(position.x, position.y + ystart, position.width, 16), fade);
	}

	protected bool drawImageSpaceEdge ( SerializedProperty property ) {
		return property.FindPropertyRelative("edgeType").enumValueIndex != (int)NprEdgeSettings.EDGETYPE.Geometry;
	}
}
