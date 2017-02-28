using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("")]
[RequireComponent(typeof(Camera))]
/// <summary>
/// Implementation of the desaturation effect. 
/// 	It changes the color saturation of the image based on its FadeControl attribute (e.g. radial decrease).
/// </summary>
public class NprDesaturate : NprImageProcessBase {

	public NprFadeControl fadeControl = null;
	
	void Start () {
	}

	public void Init (NprFadeControl fadeControl) {
		IsSupported();
		string [] shaderNames = {
			"Hidden/NPR/RGBToHSV",
			"Hidden/NPR/Desaturate",
			"Hidden/NPR/HSVToRGB",
		};
		InitMaterials(shaderNames);
		this.fadeControl = fadeControl;
	}

	protected override void Update () {
		depthBased = (fadeControl.fadeType == NprFadeControl.FadeType.DEPTH_BASED);
		base.Update();
		// avoid error message when pressing play in editor and also leaving garbage when the NPREffect is removed
#if UNITY_EDITOR
        if (gameObject.GetComponent<NprEffects>() == null) DestroyImmediate(this);
#endif
	}

	public void Render(RenderTexture source, RenderTexture destination) {
		fadeControl.ApplyToMaterial(materials[1]);
		RenderEffects(source, destination);
	}
}