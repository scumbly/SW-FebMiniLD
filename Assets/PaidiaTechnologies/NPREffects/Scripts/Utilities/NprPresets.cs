using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/NPR/Control/NPR Presets")]
[RequireComponent(typeof(NprEffects))]
/// <summary>
/// Utility script that contains a few example effects that can be achieved with the proper parameter settings. 
/// This should give you an idea about the capabilities and parameters of the NPR library.
/// </summary>
public class NprPresets : MonoBehaviour {

	public enum EffectType { 
		BW_COMIC, BW_COMIC_SHADOWS, BW_COMIC_SHADOWS_2, 
		COLOR_COMIC, COLOR_COMIC_RED_SHADOWS, COLOR_COMIC_COMPLEMENTARY_SHADOWS,
		PAINTING, PAINTING_BLACK_SHADOWS, 
		DESATURATE_RADIAL_SMOOTH, DESATURATE_RADIAL_SATURATE, DESATURATE_RADIAL_CIRCLES,
		DESATURATE_DEPTH_SMOOTH, DESATURATE_DEPTH_FOCUS,
		EDGETHICKNESS_FADE_WITH_DEPTH, BLUR_RADIAL_FADE,
		PASTEL, COLOR_PASTEL
	};

	public EffectType effectType = EffectType.COLOR_COMIC;

	void Start () {
		effect = gameObject.GetComponent<NprEffects>();
		oldEffectType = effectType;
	}

	void OnEnable() {
		applySettings();
	}

	void Update () {
		if (oldEffectType == effectType) return;
		applySettings();
		oldEffectType = effectType;
	}

	void applySettings() {
		if (effect == null) return;

		switch (effectType) {
		case EffectType.BW_COMIC:
			BWComicBase();
			break;
		case EffectType.BW_COMIC_SHADOWS:
			BWComicBase();
			BlackShadows();
			break;
		case EffectType.BW_COMIC_SHADOWS_2:
			BWComicBase();
			BlackShadows(true,true);
			break;
		case EffectType.COLOR_COMIC:
			ColorComicBase();
			break;
		case EffectType.COLOR_COMIC_RED_SHADOWS:
			ColorComicBase();
			BlackShadows(false,false,true);
			effect.shadowSettings.shadowColor = Color.red;
			break;
		case EffectType.COLOR_COMIC_COMPLEMENTARY_SHADOWS:
			ColorComicBase();
			ComplementaryShadows(true);
			break;
		case EffectType.PAINTING:
			PaintingBase();
			break;
		case EffectType.PAINTING_BLACK_SHADOWS:
			PaintingBase();
			BlackShadows(false,false,true);
			break;
		case EffectType.DESATURATE_RADIAL_SMOOTH:
			DesaturationBase();
			effect.fadeControlDesaturation.fadeType = NprFadeControl.FadeType.RADIAL;
			effect.fadeControlDesaturation.bothDirections = false;
			effect.fadeControlDesaturation.focus = 0.15f;
			effect.fadeControlDesaturation.changeSpeed = 3.0f;
			effect.fadeControlDesaturation.maxValue = 1;
			effect.fadeControlDesaturation.levels = 1;
			break;
		case EffectType.DESATURATE_RADIAL_SATURATE:
			DesaturationBase();
			effect.fadeControlDesaturation.fadeType = NprFadeControl.FadeType.RADIAL;
			effect.fadeControlDesaturation.bothDirections = false;
			effect.fadeControlDesaturation.focus = 0.3f;
			effect.fadeControlDesaturation.changeSpeed = 100.0f;
			effect.fadeControlDesaturation.maxValue = 2;
			effect.fadeControlDesaturation.levels = 1;
			break;
		case EffectType.DESATURATE_RADIAL_CIRCLES:
			DesaturationBase();
			effect.fadeControlDesaturation.fadeType = NprFadeControl.FadeType.RADIAL;
			effect.fadeControlDesaturation.bothDirections = false;
			effect.fadeControlDesaturation.focus = 0.15f;
			effect.fadeControlDesaturation.changeSpeed = 3.5f;
			effect.fadeControlDesaturation.maxValue = 1;
			effect.fadeControlDesaturation.levels = 5;
			break;
		case EffectType.DESATURATE_DEPTH_SMOOTH:
			DesaturationBase();
			effect.fadeControlDesaturation.fadeType = NprFadeControl.FadeType.DEPTH_BASED;
			effect.fadeControlDesaturation.bothDirections = false;
			effect.fadeControlDesaturation.focus = 0.2f;
			effect.fadeControlDesaturation.changeSpeed = 2.5f;
			effect.fadeControlDesaturation.maxValue = 1;
			effect.fadeControlDesaturation.levels = 1;
			break;
		case EffectType.DESATURATE_DEPTH_FOCUS:
			DesaturationBase();
			effect.fadeControlDesaturation.fadeType = NprFadeControl.FadeType.DEPTH_BASED;
			effect.fadeControlDesaturation.bothDirections = true;
			effect.fadeControlDesaturation.focus = 0.4f;
			effect.fadeControlDesaturation.changeSpeed = 25.0f;
			effect.fadeControlDesaturation.maxValue = 1.5f;
			effect.fadeControlDesaturation.levels = 1;
			break;
		case EffectType.EDGETHICKNESS_FADE_WITH_DEPTH:
			BWComicBase();
			effect.edgeSettings.thickness = 5;
			effect.edgeSettings.fade = true;
			effect.fadeControlEdges.fadeType = NprFadeControl.FadeType.DEPTH_BASED;
			effect.fadeControlEdges.bothDirections = false;
			effect.fadeControlEdges.focus = 0.2f;
			effect.fadeControlEdges.changeSpeed = 5f;
			effect.fadeControlEdges.maxValue = 1;
			effect.fadeControlEdges.levels = 1;
			break;
		case EffectType.BLUR_RADIAL_FADE:
			ColorComicBase();
			effect.edges = false;
			effect.simplificationSettings.fade = true;
			effect.fadeControlSimplification.fadeType = NprFadeControl.FadeType.RADIAL;
			effect.fadeControlSimplification.bothDirections = false;
			effect.fadeControlSimplification.focus = 0.1f;
			effect.fadeControlSimplification.changeSpeed = 30.0f;
			effect.fadeControlSimplification.maxValue = 2.0f;
			effect.fadeControlSimplification.levels = 1;
			break;
		case EffectType.PASTEL:
			BWComicBase();
			effect.edgeSettings.threshold = 50.0f;
			break;
		case EffectType.COLOR_PASTEL:
			ColorComicBase();
			effect.edgeSettings.threshold = 50.0f;
			break;
		default: break;
		}
	}

	void BWComicBase() {
		effect.edges = true;
		effect.simplify = false;
		effect.shadowEffects = false;
		effect.desaturate = false;
		
		effect.edgeSettings.onlyEdges = true;
		effect.edgeSettings.edgeType = NprEdgeSettings.EDGETYPE.Combined;
		effect.edgeSettings.edgeColor = Color.black;
		effect.edgeSettings.useBackgroundColor = false;
		effect.edgeSettings.sharpness = 2;
		effect.edgeSettings.thickness = 2;
		effect.edgeSettings.detailedness = 3;
		effect.edgeSettings.threshold = 0;
		effect.edgeSettings.fade = false;
	}

	void ColorComicBase() {
		effect.edges = true;
		effect.simplify = true;
		effect.shadowEffects = false;
		effect.desaturate = false;
		
		effect.edgeSettings.onlyEdges = false;
		effect.edgeSettings.edgeType = NprEdgeSettings.EDGETYPE.Combined;
		effect.edgeSettings.edgeColor = Color.black;
		effect.edgeSettings.useBackgroundColor = false;
		effect.edgeSettings.sharpness = 2;
		effect.edgeSettings.thickness = 2;
		effect.edgeSettings.detailedness = 3;
		effect.edgeSettings.threshold = 0;
		effect.edgeSettings.fade = false;
		effect.simplificationSettings.quantizationAmount = 3;
		effect.simplificationSettings.smoothingAmount = 3;
		effect.simplificationSettings.fade = false;
	}

	void PaintingBase() {
		effect.edges = true;
		effect.simplify = false;
		effect.shadowEffects = false;
		effect.desaturate = false;
		
		effect.edgeSettings.onlyEdges = true;
		effect.edgeSettings.edgeType = NprEdgeSettings.EDGETYPE.Combined;
		effect.edgeSettings.useBackgroundColor = true;
		effect.edgeSettings.sharpness = 2;
		effect.edgeSettings.thickness = 5;
		effect.edgeSettings.detailedness = 1;
		effect.edgeSettings.threshold = 0;
		effect.edgeSettings.fade = false;
	}

	void BlackShadows(bool shadowEdges=false, bool negativeEdges=false, bool smooth=false) {
		effect.shadowEffects = true;
		effect.shadowSettings.shadowColor = Color.black;
		effect.shadowSettings.hueOffset = 0;
		effect.shadowSettings.intensityScaler = 2;
		effect.shadowSettings.smoothShadow = smooth;
		effect.shadowSettings.hideEdges = !shadowEdges;
		effect.shadowSettings.negativeEdges = negativeEdges;
	}

	void ComplementaryShadows(bool shadowEdges=false) {
		effect.shadowEffects = true;
		effect.shadowSettings.shadowColor = Color.white;
		effect.shadowSettings.hueOffset = 0.5f;
		effect.shadowSettings.intensityScaler = 0;
		effect.shadowSettings.smoothShadow = false;
		effect.shadowSettings.hideEdges = !shadowEdges;
		effect.shadowSettings.negativeEdges = false;
	}

	void DesaturationBase() {
		effect.edges = false;
		effect.simplify = false;
		effect.shadowEffects = false;
		effect.desaturate = true;
	}

	protected NprEffects effect = null;
	protected EffectType oldEffectType;
}
