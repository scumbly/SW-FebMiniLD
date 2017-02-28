using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("")]
[RequireComponent(typeof(Camera))]
/// <summary>
/// Implementation of the image simplification effect, which removes texture details and decreases color complexity.
/// Algoritm based on "Real-Time Video Abstraction", Winnemoller et al. 2006.
/// </summary>
public class NprImageSimplification : NprImageProcessBase {

	public NprSimplificationSettings simplificationSettings = null;
	public NprFadeControl fadeControl = null;

	public void Init (NprSimplificationSettings simplificationSettings, NprFadeControl fadeControl) {
		useHDRTemporaries = true;
		IsSupported();
		string [] shaderNames = {
			"Hidden/NPR/RGBToCIELab",
			"Hidden/NPR/CIELabToRGB",
			"Hidden/NPR/SimplificationSmooth",
			"Hidden/NPR/SimplificationGradient",
			"Hidden/NPR/SimplificationQuantize"
		};
		InitMaterials(shaderNames);
		this.simplificationSettings = simplificationSettings;
		this.fadeControl = fadeControl;
	}

	protected override void Update () {
		depthBased = simplificationSettings.fade && (fadeControl.fadeType == NprFadeControl.FadeType.DEPTH_BASED);
		base.Update();
        // avoid error message when pressing play in editor and also leaving garbage when the NPREffect is removed
#if UNITY_EDITOR
        if (gameObject.GetComponent<NprEffects>() == null) DestroyImmediate(this);
#endif	
	}

	public void Render(RenderTexture source, RenderTexture destination, Texture flowField) {
		simplificationSettings.ApplyToMaterial(materials[SMOOTH],materials[QUANTIZATION_GRADIENT],materials[QUANTIZE]);
        if (simplificationSettings.fade) // this rewrites a shader parameter, so it is important to put this _after_ the other apply
		    fadeControl.ApplyToMaterial(materials[SMOOTH]);
		RenderTexture buffer = RenderTexture.GetTemporary(Screen.height,Screen.width,0,RenderTextureFormat.DefaultHDR);
		RenderTexture buffer2 = RenderTexture.GetTemporary(Screen.height,Screen.width,0,RenderTextureFormat.DefaultHDR);
		RenderEffect(source,buffer,RGB2CIELAB);
		SmoothImage(buffer,buffer2,flowField,simplificationSettings.blurIterations);

		if ( simplificationSettings.quantize ) {
			QuantizeImage(buffer2,buffer);
			Graphics.Blit(buffer,buffer2);
		}
		RenderTexture.ReleaseTemporary(buffer);

		RenderEffect(buffer2,destination,CIELAB2RGB);
		RenderTexture.ReleaseTemporary(buffer2);
	}

	public void SmoothImage(RenderTexture source, RenderTexture destination, Texture flowField, int iterations, bool updateMaterial=false) {
		if ( iterations < 1 ) {
			if (source != destination)
				Graphics.Blit(source,destination);
			return;
		}
		if ( updateMaterial )
			simplificationSettings.ApplyToMaterial(materials[SMOOTH],materials[QUANTIZATION_GRADIENT],materials[QUANTIZE]);
		materials[SMOOTH].SetTexture("_StructureTensorTex",flowField);
		RenderTexture buffer = RenderTexture.GetTemporary(Screen.height,Screen.width,0,RenderTextureFormat.DefaultHDR);
		RenderTexture from = source;
		for (int i = 0; i < iterations; i++) {
			materials[SMOOTH].SetInt("_tangentialBlur", 0);
			RenderEffect(from,buffer,SMOOTH);
			materials[SMOOTH].SetInt("_tangentialBlur", 1);
			RenderEffect(buffer,destination,SMOOTH);
			from = destination;
		}
		RenderTexture.ReleaseTemporary(buffer);
	}
	
	protected void QuantizeImage(RenderTexture source, RenderTexture destination) {
		RenderTexture gradient = RenderTexture.GetTemporary(Screen.height,Screen.width,0,RenderTextureFormat.DefaultHDR);
		RenderEffect(source,gradient,QUANTIZATION_GRADIENT);
		materials[QUANTIZE].SetTexture("_sharpnessMap", gradient);
		RenderEffect(source,destination,QUANTIZE);
		RenderTexture.ReleaseTemporary(gradient);
	}

	const int RGB2CIELAB = 0;
	const int CIELAB2RGB = RGB2CIELAB+1;
	const int SMOOTH = CIELAB2RGB+1;
	const int QUANTIZATION_GRADIENT = SMOOTH+1;
	const int QUANTIZE = QUANTIZATION_GRADIENT+1;
}
