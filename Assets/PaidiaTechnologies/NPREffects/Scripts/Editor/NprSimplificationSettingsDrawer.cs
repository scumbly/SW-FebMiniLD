using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomPropertyDrawer (typeof(NprSimplificationSettings))]
public class NprSimplificationSettingsDrawer : PropertyDrawer {

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return 16f + 18f + 18f;
	}
	
	public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label) {
		var smoothingAmount = prop.FindPropertyRelative ("smoothingAmount");
		var quantizationAmount = prop.FindPropertyRelative ("quantizationAmount");
        var fade = prop.FindPropertyRelative("fade");

		int ystart = 0;
		EditorGUI.PropertyField(new Rect(position.x,position.y+ystart,position.width,16),smoothingAmount);
		ystart += 18;	EditorGUI.PropertyField(new Rect(position.x,position.y+ystart,position.width,16),quantizationAmount);
        ystart += 18;   EditorGUI.PropertyField(new Rect(position.x, position.y + ystart, position.width, 16), fade);
	}
}
