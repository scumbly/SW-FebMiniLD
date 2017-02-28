using UnityEngine;
using System.Collections;

[System.Serializable]
/// <summary>
/// All the settings of edge rendering. It can also upload parameter settings to the shaders.
/// </summary>
public class NprEdgeSettings {
	/// <summary>
	/// Edge detection algorithm. 
	/// ImageSpace (a.k.a. texure) edges work with color intensities, 
	/// 	Geometry implements a depth-normal edge detector
	/// </summary>
	public enum EDGETYPE {ImageSpace, Geometry, Combined};
	public EDGETYPE edgeType = EDGETYPE.Combined;

	public bool onlyEdges = false;
	[Range(1,5)]
	public int thickness = 2;
	[Range(1,3)]
	public int sharpness = 2;
	[Range(1,3)]
	public int detailedness = 3;
	public bool preSmooth { get { return detailedness < 3; } }
    [Range(-100,200)]
	public float threshold = 0.0f;  // epsilon in Winnemoeller et. al 2012: XDoG
	public bool useBackgroundColor = false;
	public Texture2D backGroundTexture;
	public Color edgeColor = new Color(0,0,0);
    public bool fade = false;
	
	protected const float xdogSmoothing = 1.0f;
	protected const float structureTensorSmoothing = 3.0f;
	protected const float depthEdgeThreshold = 0.3f;
	protected const float normalEdgeThreshold = 0.045f;

    public void ApplyToMaterial(Material tensorMaterial,Material geomEdgeMaterial,Material imgEdgeMaterial) {
		if (tensorMaterial != null)
        	tensorMaterial.SetFloat("_blurSigma", structureTensorSmoothing);

		if (geomEdgeMaterial != null) {
			geomEdgeMaterial.SetFloat ("_geometryEdgeOffset", geometryEdgeOffset);
			geomEdgeMaterial.SetFloat ("_blurSigma", geometryEdgeBlur);
			geomEdgeMaterial.SetFloat ("_depthThreshold", depthEdgeThreshold * 0.01f);
			geomEdgeMaterial.SetFloat ("_normalThreshold", normalEdgeThreshold);
		}

		if (imgEdgeMaterial != null) {
			imgEdgeMaterial.SetFloat ("_dogSigma", dogSigma);
			imgEdgeMaterial.SetFloat ("_dogSensitivity", dogSensitivity);
			imgEdgeMaterial.SetFloat ("_dogThreshold", threshold);
			imgEdgeMaterial.SetFloat ("_dogSharpness", dogSharpness);
			imgEdgeMaterial.SetFloat ("_xdogSmoothing", xdogSmoothing);
		}

        if (!fade) {
			if (geomEdgeMaterial != null) {
            	geomEdgeMaterial.SetInt("_depthBased",0);
			}
			if (imgEdgeMaterial != null) {
            	imgEdgeMaterial.SetInt("_depthBased", 0);
			}
        }
	}

	private float dogSigma { // variance of the DoG
		get {
			switch (thickness) {
				// some experimental values
			case 1: return 0.5f;
			case 2: return 1.0f;
			case 3: return 2.0f;
			case 4: return 3.0f;
			case 5: return 4.0f;
			default: return 0.0f;
			};
		}
	}
	private float dogSensitivity { // phi in Winnemoeller et. al 2012: XDoG
		get {
			switch (thickness) {
				// some experimental values
			case 1: return 4.02f;
			case 2: return 3.76f;
			case 3: return 4.98f;
			case 4: return 8.61f;
			case 5: return 14.07f;
			default: return 0.0f;
			};
		}
	}
	private float geometryEdgeOffset {
		get {
			switch (thickness) {
				// some experimental values
			case 1: return 0.5f;
			case 2: return 0.8f;
			case 3: return 1.5f;
			case 4: return 2.2f;
			case 5: return 3.0f;
			default: return 0.0f;
			};
		}
	}
	private float dogSharpness { // p in Winnemoeller et. al 2012: XDoG
		get {
			switch (sharpness) {
				// some experimental values
			case 1: return 0.005f;
			case 2: return 0.01f;
			case 3: return 0.1f;
			default: return 0.0f;
			};
		}
	}
	private float geometryEdgeBlur {
		get {
			switch (sharpness) {
				// some experimental values
			case 1: return 1.2f;
			case 2: return 0.6f;
			case 3: return 0.1f;
			default: return 0.0f;
			};
		}
	}
}
