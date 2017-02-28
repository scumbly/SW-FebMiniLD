using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent (typeof(Camera))]
[AddComponentMenu("")]
/// <summary>
/// Common base class for single or multi-pass post-processing effects. 
/// It is a sort of extension of the "ImageEffectBase" class from the Image Effect (Unity Pro) library.
/// 
/// A script instance can hold multiple shaders, each possibly consisting of multiple passes. 
/// Compatibility checks, shader loading and material creation are managed by the script.
/// Rendering executes all the stored shaders and all their passes one after the other, 
/// 	with automatic temporary buffer creation if needed.
/// </summary>
public class NprImageProcessBase : MonoBehaviour {

	public Shader [] shaders;

	protected Material [] materials;
	public bool useHDRTemporaries;
	protected bool depthBased;

	void Start () {
		IsSupported();
		InitMaterials();
	}

	protected bool IsSupported() {		
		if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
		{
			Debug.LogWarning("Image based effects are not supported by this platform");
			enabled = false;
			return false;
		}
		if (SystemInfo.graphicsShaderLevel < 30)
		{
			Debug.LogWarning("Shader model 3.0 is required but not supported by this platform");
			enabled = false;
			return false;
		}
		if (useHDRTemporaries && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.DefaultHDR))
		{
			Debug.LogWarning("HDR textures are not supported by this platform");
			enabled = false;
			return false;
		}
		if (depthBased && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth)) {
			Debug.LogWarning("Depth textures are not supported by this platform");
			enabled = false;
			return false;
		}
		if (depthBased) {
			GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
		}
		return true;
	}

	// precondition: shaders has been set and material array is created
	protected bool InitMaterial(int index = 0) {
		Shader shader = shaders[index];
        if (shader == null) {
            return true;
        }
		if ( !shader.isSupported ) {
			Debug.LogWarning("Shader " + shader.ToString() + " is not supported by this platform");
			return false;
		}
		if ( materials[index] != null && materials[index].shader == shader ) {
			return true;
		}
		materials[index] = new Material(shader);
		materials[index].hideFlags = HideFlags.DontSave;
		return true;
	}

	// precondition: shaders has been set - either manually or by createMaterials(string[])
	protected void InitMaterials() {
        if (shaders == null) return;
		if (materials == null || materials.Length != shaders.Length)
			materials = new Material[shaders.Length];
		bool supported = true;
		for ( int shader = 0; shader < shaders.Length; ++shader ) {
			supported = supported && InitMaterial(shader);
		}
		if ( !supported ) {
			enabled = false;
		}
	}

	protected void InitMaterials(string [] shaderNames) {
		shaders = new Shader [shaderNames.Length];
		bool update = false;
		for ( int i = 0; i < shaderNames.Length; ++i ) {
			if ( shaders[i] == Shader.Find (shaderNames[i]) ) continue;
			update = true;
			shaders[i] = Shader.Find (shaderNames[i]);
			if ( shaders[i] == null ) {
				Debug.LogWarning("Could not find shader: " + shaderNames[i]);
				enabled = false;
				return;
			}
		}
		if ( update )
			InitMaterials();
	}

	protected RenderTextureFormat getBufferFormat() {
		return useHDRTemporaries ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
	}

	protected void RenderEffects(RenderTexture source, RenderTexture destination) {
		// Note that temporary rendertextures could be avoided using GrabPass in the shaders. 
		// However, this way it is easier to write new shaders
		RenderTexture buffer1 = RenderTexture.GetTemporary(Screen.width,Screen.height,0,getBufferFormat());
		RenderTexture buffer2 = RenderTexture.GetTemporary(Screen.width,Screen.height,0,getBufferFormat());

		RenderTexture from = source;
		RenderTexture to;
		for ( int mat = 0; mat < materials.Length; ++mat ) {
            // material might be null when the shader is not yet set in the editor
            int passCount = materials[mat] != null ? materials[mat].passCount : 0;
			for ( int pass = 0; pass < passCount; ++pass ) {
				to = (mat == materials.Length-1 && pass == materials[mat].passCount-1) ? destination : buffer2;
				Graphics.Blit(from,to,materials[mat],pass);
				if ( from != null ) // can be null somehow when Unity is loading
					from.DiscardContents(); // http://forum.unity3d.com/threads/where-to-call-rendertexture-discardcontents.215555/
				Swap (ref buffer1, ref buffer2);
				from = buffer1;
			}
		}

		RenderTexture.ReleaseTemporary(buffer1);
		RenderTexture.ReleaseTemporary(buffer2);
	}

	protected void RenderEffect(RenderTexture source, RenderTexture destination, int mat) {
		// Note that temporary rendertextures could be avoided using GrabPass in the shaders. 
		// However, this way it is easier to write new shaders
		RenderTexture buffer1 = RenderTexture.GetTemporary(Screen.width,Screen.height,0,getBufferFormat());
		RenderTexture buffer2 = RenderTexture.GetTemporary(Screen.width,Screen.height,0,getBufferFormat());
		
		RenderTexture from = source;
		RenderTexture to;
        // material might be null when the shader is not yet set in the editor
        int passCount = materials[mat] != null ? materials[mat].passCount : 0;
		for ( int pass = 0; pass < passCount; ++pass ) {
			to = (pass == materials[mat].passCount-1) ? destination : buffer2;
			Graphics.Blit(from,to,materials[mat],pass);
			if ( from != null ) // can be null somehow when Unity is loading
				from.DiscardContents(); // http://forum.unity3d.com/threads/where-to-call-rendertexture-discardcontents.215555/
			Swap (ref buffer1, ref buffer2);
			from = buffer1;
		}
		
		RenderTexture.ReleaseTemporary(buffer1);
		RenderTexture.ReleaseTemporary(buffer2);
	}

	private static void Swap(ref RenderTexture a, ref RenderTexture b) {
		RenderTexture tmp;
		tmp = a;
		a = b;
		b = tmp;
	}

	protected virtual void Update() {
		IsSupported();
		InitMaterials();
	}
}
