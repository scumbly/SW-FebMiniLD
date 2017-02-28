using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("")]
[RequireComponent(typeof(Camera))]
/// <summary>
/// Implementation of the edge detectors: intensity and geometry (depth-normal) edges.
/// </summary>
public class NprEdgeRender : NprImageProcessBase {

    public NprFadeControl fadeControl = null;
    public NprEdgeSettings edgeSettings = null;

	public void Init (NprEdgeSettings edgeSettings, NprFadeControl fadeControl) {
        useHDRTemporaries = true;
		IsSupported();
		string [] shaderNames = {
			"Hidden/NPR/EdgeFlowField",
			"Hidden/NPR/GeometricEdge",
			"Hidden/NPR/DOGEdge",
			"Hidden/NPR/CombineEdges"
		};
		InitMaterials(shaderNames);
		this.fadeControl = fadeControl;
        this.edgeSettings = edgeSettings;
	}

	protected override void Update () {
		depthBased = edgeSettings.fade && (fadeControl.fadeType == NprFadeControl.FadeType.DEPTH_BASED)
			|| edgeSettings.edgeType == NprEdgeSettings.EDGETYPE.Combined 
			|| edgeSettings.edgeType == NprEdgeSettings.EDGETYPE.Geometry;
		base.Update();
        // avoid error message when pressing play in editor and also leaving garbage when the NPREffect is removed
#if UNITY_EDITOR
        if (gameObject.GetComponent<NprEffects>() == null) DestroyImmediate(this);
#endif
	}
	
	public void Render(RenderTexture source, RenderTexture destination, ref RenderTexture flowFieldBuffer, bool exportFlowField = false) {
		edgeSettings.ApplyToMaterial(materials[FLOWFIELD], materials[GEOMEDGES], materials[IMGEDGES]);
		if (edgeSettings.fade) { // this rewrites a shader parameter, so it is important to put this _after_ the other apply
			fadeControl.ApplyToMaterial(materials[GEOMEDGES]);
			fadeControl.ApplyToMaterial(materials[IMGEDGES]);
		}
		switch ( edgeSettings.edgeType ) {
		case NprEdgeSettings.EDGETYPE.Geometry: RenderGeometryEdges(source,destination); break;
		case NprEdgeSettings.EDGETYPE.ImageSpace: RenderImageSpaceEdges(source,destination,ref flowFieldBuffer,exportFlowField); break;
		case NprEdgeSettings.EDGETYPE.Combined: RenderCombinedEdges(source,destination,ref flowFieldBuffer,exportFlowField); break;
		default: Debug.LogWarning("Edge type not supported: "+edgeSettings.edgeType.ToString()); break;
		}
	}

	public void RenderFlowField(RenderTexture source, ref RenderTexture flowFieldBuffer) {
		if ( flowFieldBuffer != null ) return;
		edgeSettings.ApplyToMaterial(materials[FLOWFIELD], null, null);
		flowFieldBuffer = RenderTexture.GetTemporary(Screen.width,Screen.height,0,RenderTextureFormat.DefaultHDR);
		RenderEffect(source,flowFieldBuffer,FLOWFIELD);
	}

	protected void RenderImageSpaceEdges(RenderTexture source, RenderTexture destination, ref RenderTexture flowFieldBuffer, bool exportFlowField = false) {
		RenderFlowField(source,ref flowFieldBuffer);
		materials[IMGEDGES].SetTexture("_flowTex",flowFieldBuffer);
		RenderEffect(source,destination,IMGEDGES);
		if ( !exportFlowField ) {
			RenderTexture.ReleaseTemporary(flowFieldBuffer);
			flowFieldBuffer = null;
		}
	}
	
	protected void RenderGeometryEdges(RenderTexture source, RenderTexture destination) {
		RenderEffect(source,destination,GEOMEDGES);
	}

	protected void RenderCombinedEdges(RenderTexture source, RenderTexture destination, ref RenderTexture flowFieldBuffer, bool exportFlowField = false) {
		RenderTexture imageSpaceEdges = RenderTexture.GetTemporary(Screen.width,Screen.height); // NOTE: no need to be HDR
		RenderImageSpaceEdges(source,imageSpaceEdges,ref flowFieldBuffer,exportFlowField);
		RenderTexture geomEdges = RenderTexture.GetTemporary(Screen.width,Screen.height); // NOTE: no need to be HDR
		RenderGeometryEdges(source,geomEdges);
		minimumImage(imageSpaceEdges,destination,geomEdges);
		RenderTexture.ReleaseTemporary(imageSpaceEdges);
		RenderTexture.ReleaseTemporary(geomEdges);
	}

	protected void minimumImage(RenderTexture imageSpaceEdges, RenderTexture destination, RenderTexture geomEdges) {
		materials[MERGE].SetTexture("_OtherTex", geomEdges);
		RenderEffect(imageSpaceEdges,destination,MERGE);
	}

    const int FLOWFIELD = 0;
	const int GEOMEDGES = FLOWFIELD+1;
	const int IMGEDGES = GEOMEDGES+1;
	const int MERGE = IMGEDGES+1;

}
