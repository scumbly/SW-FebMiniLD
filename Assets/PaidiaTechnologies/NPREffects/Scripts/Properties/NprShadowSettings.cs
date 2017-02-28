using UnityEngine;
using System.Collections;

[System.Serializable]
/// <summary>
/// All the settings of shadow re-coloring. It can also upload parameter settings to the shaders.
/// </summary>
public class NprShadowSettings {

	public bool smoothShadow = false;
	public Color shadowColor = Color.black;
	[Range(0,1)]
	public float hueOffset = 0.0f;
	[Range(0,2)]
	public float intensityScaler = 1.0f;
	public bool hideEdges = false;
	public bool negativeEdges = false;

	public void ApplyToMaterial(Material material, bool extract = true) {
		if ( extract ) {	// shadow extraction material
			material.SetInt("_smoothShadow",smoothShadow ? 1 : 0);
		} else {			// shadow renderer material
			material.SetColor("_shadowColor",shadowColor);
			material.SetFloat("_shadowHueOffset",hueOffset);
			material.SetFloat("_shadowIntensityScaler",intensityScaler);
			material.SetInt("_showShadowEdge",hideEdges ? 0:1);
			material.SetInt("_negativeEdges",negativeEdges ? 1:0);
		}
	}

	public bool edgeDrawingSet;
}
