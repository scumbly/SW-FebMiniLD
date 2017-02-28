using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/NPR/Utility/MirrorY")]
[RequireComponent(typeof(Camera))]
/// <summary>
/// A utility post-processing script to flip the rendered image in vertical direction.
/// </summary>
public class NprImageMirrorY : NprGeneralImageProcessing {

	void Start () {
		IsSupported();
		string [] shaderNames = {
			"Hidden/NPR/MirrorY"
		};
		InitMaterials(shaderNames);	
	}
}
