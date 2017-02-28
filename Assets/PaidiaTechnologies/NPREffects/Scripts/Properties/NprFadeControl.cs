using UnityEngine;
using System.Collections;

[System.Serializable]
/// <summary>
/// Settings for fade-type effects. It can also upload parameter settings to the shaders.
/// Fade basically determines the "strength" (e.g. the amount of blur, desaturation or the edge thickness)
/// 	of an effect based on a given parameter (depth or radial distance from the center of the image).
/// In each case, "focus" is the parameter value where the objects are most detailed and emphasized
/// 	(e.g. most color saturation, thickest edges, least blur), the level of abstraction increases with the
/// 	distance from this focus point. The peak value (e.g. maximum saturation scaler, edge thickness, etc.) 
/// 	and the fading speed are determined by the corresponding attributes.
/// </summary>
public class NprFadeControl {
	/// <summary>
	/// Parameter which determines the effect strength.
	/// </summary>
	public enum FadeType { DEPTH_BASED = 1, RADIAL = 2};

	public FadeType fadeType = FadeType.RADIAL;
	[Range(0.00001f, 1.0f)]
	public float focus = 0.2f;
	[Range(0.1f, 2.0f)]
	public float maxValue = 1.0f;
	[Range(0.00001f, 100.0f)]
	public float changeSpeed = 10.0f;
	[Range(1, 8)]
	public int levels = 1;
	public bool bothDirections = false;

	public void ApplyToMaterial(Material material) {
		material.SetFloat("_focus", focus);
		material.SetFloat("_depthLevels", (float)levels);
		material.SetFloat("_depthScaler", maxValue);
		material.SetFloat("_changeSpeed", changeSpeed);
		material.SetInt("_bothDirections", bothDirections ? 1 : 0);
		material.SetInt("_depthBased", fadeType == FadeType.DEPTH_BASED ? 1 : 2);
	}

    public void copy(NprFadeControl other) {
        this.focus = other.focus;
		this.maxValue = other.maxValue;
        this.changeSpeed = other.changeSpeed;
        this.levels = other.levels;
        this.bothDirections = other.bothDirections;
        this.fadeType = other.fadeType;
    }
}
