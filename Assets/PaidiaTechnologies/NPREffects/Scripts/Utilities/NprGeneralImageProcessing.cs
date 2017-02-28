using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/NPR/ImageProcessor")]
/// <summary>
/// A useful utility script that has the same functionalities as its base class
/// 	(i.e. a multi-pass post-processing effect).
/// By implementing the OnRenderImage, this script can be directly attached to camera objects.
/// 
/// To use the script, simply attach it to a camera object, and put your image processing shaders 
/// 	into its shader array. The shaders will be compiled, loaded and executed automatically.
/// </summary>
public class NprGeneralImageProcessing : NprImageProcessBase {
	
	protected override void Update () {
        base.Update();	
	}

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (materials == null) {
            Graphics.Blit(source, destination);
            return;
        }
        RenderEffects(source, destination);
    }
}
