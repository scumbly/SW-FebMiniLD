using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("")]
[RequireComponent(typeof(Camera))]
/// <summary>
/// Implementation of the compositor, which merges the different effects, 
/// 	i.e. it combines shadows, the color image (possibly simplified) and the edges.
/// </summary>
public class NprCompositor : NprImageProcessBase {

	public void Init () {
		IsSupported();
		string [] shaderNames = {
			"Hidden/NPR/CompositeEdge",
			"Hidden/NPR/CompositeShadow",
			"Hidden/NPR/CompositeEdgeShadow",
			"Hidden/NPR/CompositeAll",
		};
		InitMaterials(shaderNames);
	}
	
	protected override void Update () {
		//depthBased = (fadeControl.fadeType == FadeControl.FadeType.DEPTH_BASED);
		base.Update();

		// avoid error message when pressing play in editor and also leaving garbage when the NPREffect is removed
#if UNITY_EDITOR
		if (gameObject.GetComponent<NprEffects>() == null) DestroyImmediate(this);
#endif	
	}
	
	public void RenderEdges(RenderTexture color, RenderTexture edgeMap, Texture background, RenderTexture destination, Color edgeColor, bool useBackgroundColor) {
		if (background != null) {
			materials[EDGES].SetTexture("_BackGroundTex",background);
		}
		materials[EDGES].SetTexture("_EdgeMapTex",edgeMap);
		materials[EDGES].SetInt("_useBackgroundEdgeColor",useBackgroundColor ? 1:0);
		materials[EDGES].SetColor("_edgeColor",edgeColor);

		RenderEffect(useBackgroundColor ? color:edgeMap,destination,EDGES);
	}

	public void RenderShadows(RenderTexture color, Texture shadowMap, RenderTexture destination, NprShadowSettings shadowSettings) {
		if (shadowMap != null) {
			materials[SHADOWS].SetTexture("_ShadowImageTex",shadowMap);
		}
		shadowSettings.ApplyToMaterial(materials[SHADOWS],false);
		
		RenderEffect(color,destination,SHADOWS);
	}

	public void RenderEdgesShadows(RenderTexture color, Texture shadowMap, Texture edgeMap, Texture background, RenderTexture destination, 
	                                      NprShadowSettings shadowSettings, Color edgeColor, bool useBackgroundColor=true) {
		if (shadowMap != null) {
			materials[EDGES_SHADOWS].SetTexture("_ShadowImageTex",shadowMap);
		}
		shadowSettings.ApplyToMaterial(materials[EDGES_SHADOWS],false);
		if (background != null)
			materials[EDGES_SHADOWS].SetTexture("_BackGroundTex",background);
		materials[EDGES_SHADOWS].SetTexture("_EdgeMapTex",edgeMap);
		materials[EDGES_SHADOWS].SetColor("_edgeColor",edgeColor);
		materials[EDGES_SHADOWS].SetInt("_useBackgroundEdgeColor",useBackgroundColor ? 1:0);
		
		RenderEffect(color,destination,EDGES_SHADOWS);
	}

	public void RenderAll(RenderTexture color, Texture shadowMap, Texture edgeMap, RenderTexture destination, NprShadowSettings shadowSettings, Color edgeColor) {
		if (shadowMap != null) {
			materials[ALL].SetTexture("_ShadowImageTex",shadowMap);
		}
		shadowSettings.ApplyToMaterial(materials[ALL],false);
		materials[ALL].SetTexture("_EdgeMapTex",edgeMap);
		materials[ALL].SetColor("_edgeColor",edgeColor);
		
		RenderEffect(color,destination,ALL);
	}

	const int EDGES = 0;
	const int SHADOWS = EDGES+1;
	const int EDGES_SHADOWS = SHADOWS+1;
	const int ALL = EDGES_SHADOWS+1;
}
