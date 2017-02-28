using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("")]
[RequireComponent(typeof(Camera))]
/// <summary>
/// Implementation of the shadow re-coloring effects.
/// In order to determine the shadowed pixels, it renders the scene without shadows, 
/// 	which is then compared against the original rendered image with shadows.
/// </summary>
public class NprShadowCreate : NprImageProcessBase {

	public NprShadowSettings shadowSettings = null;
	
	void Awake() {
		GameObject go = GameObject.Find("NPRShadowCamera");
		while (go) {
		    DestroyImmediate(go);
			go = GameObject.Find("NPRShadowCamera"); 
		}

	}

	public void Init (NprShadowSettings shadowSettings) {
        IsSupported();
		string [] shaderNames = {
			"Hidden/NPR/ExtractShadows"
		};
		InitMaterials(shaderNames);
		this.shadowSettings = shadowSettings;
	}

    void OnPreRender() {
        Camera mainCam = gameObject.GetComponent<Camera>();
		createShadowCam ();
		shadowCamera.CopyFrom(mainCam);

        if (target == null || target.width!=Screen.width || target.height!=Screen.height) {
			DestroyTargetTex();
            target = new RenderTexture(Screen.width, Screen.height, 16);
			target.hideFlags = HideFlags.DontSave;
        }
        shadowCamera.targetTexture = target;
		// "Future work"
		//if (QualitySettings.antiAliasing != 0)
		//	target.antiAliasing = QualitySettings.antiAliasing;
    }

    protected override void Update () {
		base.Update();
        // avoid error message when pressing play in editor and also leaving garbage when the NPREffect is removed
#if UNITY_EDITOR
        if (gameObject.GetComponent<NprEffects>() == null) {
			GameObject go = GameObject.Find("NPRShadowCamera");
			while (go) {
				DestroyImmediate(go);
				go = GameObject.Find("NPRShadowCamera"); 
			}
            target.Release();
            DestroyImmediate(this);
        }
#endif
		// currently the class does not work with anti-aliasing
		if (QualitySettings.antiAliasing != 0) {
			Debug.LogWarning("Shadow effects are currently not supported with anti-aliasing. Disabling.");
			enabled = false;
		}
	}

	protected void createShadowCam() {
		if (shadowCamObj == null) {
			shadowCamObj = new GameObject("NPRShadowCamera");
			// Finally I decided better not to hide the other camera from the user
			//shadowCamObj.hideFlags = HideFlags.HideAndDontSave;
			shadowCamObj.hideFlags = HideFlags.DontSave;
		}
		if (shadowCamera == null) {
			shadowCamera = shadowCamObj.GetComponent<Camera>();
			if ( shadowCamera == null )
				shadowCamera = shadowCamObj.AddComponent<Camera>();
		}
	}
	
	protected void OnDestroy() {
		if (shadowCamObj != null)
#if UNITY_EDITOR
			DestroyImmediate(shadowCamObj);
#else
			Destroy(shadowCamObj);
#endif
		DestroyTargetTex();
	}

	protected void DestroyTargetTex() {
		if (target != null) {
			target.Release();
#if UNITY_EDITOR
			DestroyImmediate(target);
#else
			Destroy(target);
#endif
		}
	}

	public void Render(RenderTexture source, RenderTexture destination) {
		shadowSettings.ApplyToMaterial(materials[0]);
        materials[0].SetTexture("_ShadowTex", target);
        materials[0].SetFloat("_maxShadowDist",maxShadowDistance);
        RenderEffects(source, destination);
	}

	protected void LateUpdate() {
		if ( shadowCamera ) {
			shadowCamera.CopyFrom(gameObject.GetComponent<Camera>());
			shadowCamera.targetTexture = target;
		}
	}

    protected Camera shadowCamera;
	protected GameObject shadowCamObj;
    protected RenderTexture target = null;
    protected float maxShadowDistance = 0.01f;  // empirical value
}
