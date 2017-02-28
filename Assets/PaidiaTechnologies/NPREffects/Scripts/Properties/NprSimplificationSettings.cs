using UnityEngine;
using System.Collections;

[System.Serializable]
/// <summary>
/// All the settings of image simplification. It can also upload parameter settings to the shaders.
/// </summary>
public class NprSimplificationSettings {

	[Range(1,5)]
	public int smoothingAmount = 3;
	[Range(0,5)]
	public int quantizationAmount = 3;

	public int blurIterations {
		get {
			switch (smoothingAmount) {
			case 1: return 1;
			case 2: return 2;
			case 3: return 3;
			case 4: return 4;
			case 5: return 5;
			default: return 1;
			}
		}
	}
	public bool quantize {
		get {
			return 0 < quantizationAmount;
		}
	}
    public bool fade = false;

	public void ApplyToMaterial(Material smoothMaterial,Material quantizationGradientMaterial,Material quantizationMaterial) {
		smoothMaterial.SetFloat("_imageSpaceSigma",imageSpaceSigma);
		smoothMaterial.SetFloat("_pixelSpaceSigma",pixelSpaceSigma);
        if (!fade) {
            smoothMaterial.SetInt("_depthBased", 0);
        }

		quantizationGradientMaterial.SetVector("_mapping", new Vector4(gradientClamp.x, gradientClamp.y, sharpnessRange.x, sharpnessRange.y));

		quantizationMaterial.SetFloat("_bins", quantizationBins);
		quantizationMaterial.SetFloat("_transitionSharpness",transitionSharpness);
	}

	private float imageSpaceSigma {
		get {
			switch (smoothingAmount) {
			case 1: return 0.2f;
			case 2: return 0.3f;
			case 3: return 0.3f;
			case 4: return 0.6f;
			case 5: return 0.8f;
			default: return 0.1f;
			}
		}
	}
	private int quantizationBins {
		get {
			switch (quantizationAmount) {
			case 0: return 20;
			case 1: return 13;
			case 2: return 10;
			case 3: return 8;
			case 4: return 5;
			case 5: return 3;
			default: return 20;
			}
		}
	}
	private const float pixelSpaceSigma = 3.0f;
	private static Vector2 gradientClamp = new Vector2(0.0f, 2.0f);
	private static Vector2 sharpnessRange = new Vector2(3.0f, 14.0f);
	private const float transitionSharpness = 0.1f;
}
