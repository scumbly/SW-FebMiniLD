using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/NPR/NPREffects")]
[RequireComponent(typeof(Camera))]
/// <summary>
/// Main effect class, which controls all the rendering effects. 
/// It keeps track of the rendering settings and the effect scipts.
/// In addition, it switches shadow casting on/off which is used for shadow re-coloring effects.
/// </summary>
public class NprEffects : MonoBehaviour {

	public bool desaturate = false;
	public bool edges = false;
	public bool simplify = false;
	public bool shadowEffects = false;
	public bool colorTransfer = false;

	public NprEdgeSettings edgeSettings;
	public NprSimplificationSettings simplificationSettings;
	protected NprSimplificationSettings prefilterSettings;
	public NprShadowSettings shadowSettings;
    public NprFadeControl fadeControlEdges;
	public NprFadeControl fadeControlSimplification;
	public NprFadeControl fadeControlDesaturation;
	public NprColorTransferSettings colorTransferSettings;

	[SerializeField]
    protected NprEdgeRender edgeRenderer;
	[SerializeField]
	protected NprDesaturate desaturator;
	[SerializeField]
	protected NprImageSimplification simplifier;
	[SerializeField]
	protected NprShadowCreate shadowCreator;
	[SerializeField]
	protected NprCompositor compositor;
	[SerializeField]
	protected NprColorTransfer colorTransferRenderer;

	protected Light[] lights;
	protected Dictionary<Light, LightShadows> shadowCastState = new Dictionary<Light, LightShadows>();

	void Awake () {
		// a quick and dirty way to ensure that only 1 instance is assigned per gameObject
		lock (gameObject) {
			if (gameObject.GetComponents<NprEffects> ().Length > 1) {
				Debug.LogWarning ("NPR warning: only one instance of " + typeof(NprEffects) + " is allowed per each camera.");
				enabled = false;
				DestroyImmediate (this);
				return;
			}
		}

		edgeMap = shadowMap = null;
		CreateIfNotNull<NprEdgeSettings>(ref edgeSettings);
		CreateIfNotNull<NprSimplificationSettings>(ref simplificationSettings);
		CreateIfNotNull<NprShadowSettings>(ref shadowSettings);
		CreateIfNotNull<NprFadeControl>(ref fadeControlDesaturation);
		CreateIfNotNull<NprFadeControl>(ref fadeControlEdges);
		CreateIfNotNull<NprFadeControl>(ref fadeControlSimplification);
		CreateIfNotNull<NprColorTransferSettings>(ref colorTransferSettings);
		CreateAndAttachIfNottNull<NprDesaturate>(ref desaturator);
		CreateAndAttachIfNottNull<NprEdgeRender>(ref edgeRenderer);
		CreateAndAttachIfNottNull<NprImageSimplification>(ref simplifier);
		CreateAndAttachIfNottNull<NprShadowCreate>(ref shadowCreator);
		CreateAndAttachIfNottNull<NprCompositor>(ref compositor);
		CreateAndAttachIfNottNull<NprColorTransfer>(ref colorTransferRenderer);

		// there is no direct user control over this
		prefilterSettings = new NprSimplificationSettings();
		prefilterSettings.quantizationAmount = 0;

		lights = GameObject.FindObjectsOfType(typeof(Light)) as Light[];
	}

	void CreateIfNotNull<T>(ref T obj) where T : new() {
		if (obj == null) {
			obj = new T();
		}
	}

	void CreateAndAttachIfNottNull<T>(ref T obj) where T : MonoBehaviour {
		if (obj == null) {
			obj = gameObject.AddComponent<T>();
			obj.hideFlags = HideFlags.HideInInspector;
		}
	}

	void OnEnable() {
		edgeRenderer.Init(edgeSettings, fadeControlEdges);
		simplifier.Init(simplificationSettings,fadeControlSimplification);
		shadowCreator.Init(shadowSettings);
		desaturator.Init(fadeControlDesaturation);
		compositor.Init();
		colorTransferRenderer.Init(colorTransferSettings);
		CreateWhiteBackground();
	}

	void OnDisable() {
		ReleaseBuffers();
	}

	void OnDestroy() {
		ReleaseBuffers();
		if ( whiteBackground != null ) 
#if UNITY_EDITOR
			DestroyImmediate(whiteBackground);
#else
			Destroy(whiteBackground);
#endif
	}
	
	void Update () {
		CreateBuffers();
		CreateWhiteBackground();
		shadowSettings.edgeDrawingSet = edgesOn();
	}

	void CreateBuffers() {
		if ( edgeMap == null || edgeMap.width != Screen.width || edgeMap.height != Screen.height ) {
			ReleaseBuffer(edgeMap);
			edgeMap = new RenderTexture(Screen.width,Screen.height,0);
			edgeMap.hideFlags = HideFlags.DontSave;
		}
		if ( shadowMap == null || shadowMap.width != Screen.width || shadowMap.height != Screen.height ) {
			ReleaseBuffer(shadowMap);
			shadowMap = new RenderTexture(Screen.width,Screen.height,0);
			shadowMap.hideFlags = HideFlags.DontSave;
		}
		if ( source == null || source.width != Screen.width || source.height != Screen.height ) {
			ReleaseBuffer(source);
			source = new RenderTexture(Screen.width,Screen.height,0);
			source.hideFlags = HideFlags.DontSave;
		}
	}

	void CreateWhiteBackground() {
		if (edgeSettings.backGroundTexture == null) {
			if ( whiteBackground == null ) {
				whiteBackground = new Texture2D(1,1);
				whiteBackground.SetPixel(0,0,Color.white);
				whiteBackground.Apply();
			}
			edgeSettings.backGroundTexture = whiteBackground;
		}
	}

	void ReleaseBuffers() {
		ReleaseBuffer(edgeMap);
		ReleaseBuffer(shadowMap);
		ReleaseBuffer(source);
	}

	void ReleaseBuffer(RenderTexture buffer) {
		if (buffer != null) {
			buffer.Release();
#if UNITY_EDITOR
			DestroyImmediate(buffer);
#else
			Destroy(buffer);
#endif
		}
	}
	
	void OnPreCull() {
		if (!shadowsOn() ) return;
		foreach (Light l in lights)	{
			shadowCastState[l] = l.shadows;
			l.shadows = LightShadows.None;
		}
	}
	
	void OnPostRender() {
		if (!shadowsOn() ) return;
		foreach (Light l in lights)	{
			l.shadows = shadowCastState[l];
		}
	}

	void OnRenderImage(RenderTexture src, RenderTexture destination)
	{
		if ( noEffects() ) {
			Graphics.Blit(src,destination);
			return;
		}
		if (noNonColorTransferEffects() && earlyColorTransferOn()) {
			colorTransferRenderer.Render(src,destination);
			return;
		}
		if (earlyColorTransferOn()) {
			colorTransferRenderer.Render(src,source);
		} else {
			Graphics.Blit(src,source);
		}

		RenderTexture flowField = null;
        if (edgesOn()) {
			RenderEdges(source,edgeMap,ref flowField);
		}
	
		if (simplifyOn()) {
			// edgeRenderer renders the flowfield only with specific parameter settings 
			// i.e. when edges are enabled, we create the flowfield again
			edgeRenderer.RenderFlowField(source, ref flowField);
			simplifier.Render (source,source,flowField);
		}
		if ( flowField != null )
			RenderTexture.ReleaseTemporary(flowField);

		if (shadowsOn()) {
			shadowCreator.Render(src,shadowMap);
		}

		RenderTexture composite;
		if (desaturationOn () || lateColorTransferOn ()) {
			composite = RenderTexture.GetTemporary (Screen.width, Screen.height);
			CompositeEffects(source,composite);
			Graphics.Blit(composite,source);
			RenderTexture.ReleaseTemporary(composite);
		} else {
			CompositeEffects(source,destination);
			return;
		}


		if ( desaturationOn() ) {
			desaturator.Render(source,lateColorTransferOn() ? source : destination);
		}

		if ( lateColorTransferOn() ) {
			colorTransferRenderer.Render(source,destination);
		}
	}

	void RenderEdges( RenderTexture source, RenderTexture destination, ref RenderTexture flowField ) {
		RenderTexture prefilteredSource = null;
		if (edgeSettings.preSmooth) {
			edgeRenderer.RenderFlowField(source, ref flowField);
			prefilteredSource = RenderTexture.GetTemporary(Screen.width,Screen.height);
			switch (edgeSettings.detailedness) {
			case 1: prefilterSettings.smoothingAmount = 5; break;
			case 2: prefilterSettings.smoothingAmount = 1; break;
			default:break;
			}
			simplifier.simplificationSettings = prefilterSettings;
			simplifier.SmoothImage(source,prefilteredSource,flowField,1,true);
			simplifier.simplificationSettings = simplificationSettings;
			edgeRenderer.Render(prefilteredSource, edgeMap, ref flowField, simplifyOn());
			RenderTexture.ReleaseTemporary(prefilteredSource);
		}
		else {
			edgeRenderer.Render(source, edgeMap, ref flowField, simplifyOn());
		}
	}

	void CompositeEffects(RenderTexture source, RenderTexture destination) {
		if ( edgesOn() && !edgeSettings.onlyEdges && shadowsOn() ) {
			compositor.RenderAll(source,shadowMap,edgeMap,destination,shadowSettings,edgeSettings.edgeColor);
		}
		else if ( edgesOn () && !shadowsOn() ) {
			Texture bg = edgeSettings.onlyEdges ? (Texture)edgeSettings.backGroundTexture : (Texture)source;
			compositor.RenderEdges(source,edgeMap,bg,destination,edgeSettings.edgeColor,edgeSettings.onlyEdges&&edgeSettings.useBackgroundColor);
		}
		else if ( edgesOn () && shadowsOn() ) {
			compositor.RenderEdgesShadows(source,shadowMap,edgeMap,edgeSettings.backGroundTexture,destination,
			                                     shadowSettings,edgeSettings.edgeColor,edgeSettings.useBackgroundColor);
		}
		else if ( shadowsOn() ) {
			compositor.RenderShadows(source,shadowMap,destination,shadowSettings);
		}
		else {
			Graphics.Blit(source,destination);
		}
	}

	public bool edgesSupported { get { return edgeRenderer == null ? true : edgeRenderer.enabled; } }
	public bool simplificationSupported { get { return simplifier == null ? true : simplifier.enabled; } }
	public bool shadowsSupported { get { return shadowCreator == null ? true : shadowCreator.enabled; } }
	public bool desaturationSupported { get { return desaturator == null ? true : desaturator.enabled; } }
	public bool colorTransferSupported { get { return colorTransferRenderer == null ? true : colorTransferRenderer.enabled; } }

    bool edgesOn() {
		return edges && edgesSupported;
	}

	bool simplifyOn() {
		return simplify && simplificationSupported;
	}

	bool shadowsOn() {
		return shadowEffects && shadowsSupported;
	}

	bool desaturationOn() {
		return desaturate && desaturationSupported;
	}

	bool colorTransferOn() {
		return colorTransfer;
	}

	bool lateColorTransferOn() {
		return colorTransferOn() && !colorTransferSettings.applyBeforeOtherEffects;
	}

	bool earlyColorTransferOn() {
		return colorTransferOn() && colorTransferSettings.applyBeforeOtherEffects;
	}

	bool noNonColorTransferEffects() {
		return !desaturationOn() && !edgesOn() && !simplifyOn() && !shadowsOn();
	}

	bool noEffects() {
		return noNonColorTransferEffects() && !colorTransferOn();
	}

	private RenderTexture edgeMap;
	private RenderTexture shadowMap;	// this has nothing to do with the standard shadow map, it contains the pixels which are in shadow
	private RenderTexture source;
	private Texture2D whiteBackground;
}
