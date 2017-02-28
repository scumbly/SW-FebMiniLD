using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomPropertyDrawer (typeof(NprShadowSettings))]
public class NprShadowSettingsDrawer : PropertyDrawer {

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		bool edgeDrawingSet = property.FindPropertyRelative("edgeDrawingSet").boolValue;
		bool hideEdges = property.FindPropertyRelative("hideEdges").boolValue;
		return 16f + 18f + 18f + 18f + 
			(edgeDrawingSet ? 18f : 0f) +
			(edgeDrawingSet && !hideEdges ? 18f : 0f);
	}
	
	public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label) {
		var smoothShadow = prop.FindPropertyRelative ("smoothShadow");
		var shadowColor = prop.FindPropertyRelative ("shadowColor");
		var hueOffset = prop.FindPropertyRelative ("hueOffset");
		var intensityScaler = prop.FindPropertyRelative ("intensityScaler");
		var hideEdges = prop.FindPropertyRelative ("hideEdges");
		var negativeEdges = prop.FindPropertyRelative ("negativeEdges");
		var edgeDrawingSet = prop.FindPropertyRelative ("edgeDrawingSet");
		
		int ystart = 0;
		EditorGUI.PropertyField(new Rect(position.x,position.y+ystart,position.width,16),smoothShadow);
		ystart += 18;	EditorGUI.PropertyField(new Rect(position.x,position.y+ystart,position.width,16),shadowColor);
		ystart += 18;	EditorGUI.PropertyField(new Rect(position.x,position.y+ystart,position.width,16),hueOffset);
		ystart += 18;	EditorGUI.PropertyField(new Rect(position.x,position.y+ystart,position.width,16),intensityScaler);
		if ( edgeDrawingSet.boolValue ) {
			ystart += 18;	EditorGUI.PropertyField(new Rect(position.x,position.y+ystart,position.width,16),hideEdges);
		}
		if ( edgeDrawingSet.boolValue && !hideEdges.boolValue ) {
			ystart += 18;	EditorGUI.PropertyField(new Rect(position.x,position.y+ystart,position.width,16),negativeEdges);
		}
	}
}
