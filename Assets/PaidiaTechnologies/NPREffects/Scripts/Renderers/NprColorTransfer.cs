using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("")]
[RequireComponent(typeof(Camera))]
/// <summary>
/// Implementation of the color transfer effect. 
/// 	It changes the color distribution of the source image based on a target exemplar image.
/// </summary>
public class NprColorTransfer : NprImageProcessBase {

	public NprColorTransferSettings settings = null;

	public void Init (NprColorTransferSettings settings) {
		useHDRTemporaries = true;
		IsSupported();
		string [] shaderNames = {
			"Hidden/NPR/PixelVariance",
			"Hidden/NPR/ColorTransfer",
			"Hidden/NPR/RGBToHSV",
			"Hidden/NPR/HSVToRGB",
			"Hidden/NPR/RGBToORGB",
			"Hidden/NPR/ORGBToRGB",
			// sometimes these produce nice results, but usually not
			//"Hidden/NPR/RGBToCIELab",
			//"Hidden/NPR/CIELabToRGB",
			//"Hidden/NPR/RGBToXYZ",
			//"Hidden/NPR/XYZToRGB",
			//"Hidden/NPR/RGBToLalphabeta",
			//"Hidden/NPR/LalphabetaToRGB"
		};
		InitMaterials(shaderNames);
		this.settings = settings;
	}

	protected override void Update () {
		base.Update();
		// avoid error message when pressing play in editor and also leaving garbage when the NPREffect is removed
		#if UNITY_EDITOR
		if (gameObject.GetComponent<NprEffects>() == null) DestroyImmediate(this);
		#endif	
	}

	void OnDestroy() {
		ReleaseTemporaryBuff();
	}

	void OnDisable() {
#if UNITY_EDITOR
		// fix: it seems that when start is pressed in editor mode, 
		// Unity releases textures and THEN it calls disable,
		// so the texture reference becomes invalid
		if (!Application.isPlaying) return;
#endif
		ReleaseTemporaryBuff();
	}

	public void Render(RenderTexture source, RenderTexture destination) {
		if (settings.currentTexture == null) {
			Graphics.Blit(source,destination);
			Debug.Log("NPR note: no exemplar texture (\"Source\") has been set for color transfer.");
			return;
		}

		materials[PIXELVARIANCE].SetFloat("_scale", settings.varianceScale);
		materials[COLORTRANSFER].SetFloat("_scale", settings.varianceScale);

		bool settingsChanged = !settings.SameState(transferState);

		RenderTexture sourceMean = null, sourceVariance = null;
		ComputeImageStatistics(source, ref sourceMean, ref sourceVariance);
		if (settingsChanged) {
			ReleaseTemporaryBuff();
			ComputeImageStatistics(settings.currentTexture, ref targetMean, ref targetVariance);
		}

		TransferColor(source, destination, settings.currentTexture, 
		              sourceMean, sourceVariance, targetMean, targetVariance);

		RenderTexture.ReleaseTemporary(sourceMean);
		RenderTexture.ReleaseTemporary(sourceVariance);
		transferState = settings.stateCopy();
	}

	protected void ReleaseTemporaryBuff() {
		if (null != targetMean)
			RenderTexture.ReleaseTemporary(targetMean);
		if (null != targetVariance)
			RenderTexture.ReleaseTemporary(targetVariance);
		targetMean = targetVariance = null;
	}

	protected void TransferColor(RenderTexture source, RenderTexture destination, Texture target,
								 RenderTexture sourceMean, RenderTexture sourceVariance,
	                             RenderTexture targetMean, RenderTexture targetVariance) {
		if (source == null) return;
		Material matTransfer = materials[COLORTRANSFER];
		int maxlevelSource = (int)Mathf.Log (Mathf.Max (source.width, source.height), 2);
		int maxlevelTarget = (int)Mathf.Log (Mathf.Max (target.width, target.height), 2);
		matTransfer.SetTexture("_I1Mean", sourceMean);
		matTransfer.SetTexture("_I2Mean", targetMean);
		matTransfer.SetTexture("_I1Var", sourceVariance);
		matTransfer.SetTexture("_I2Var", targetVariance);
		matTransfer.SetFloat("_Level1", maxlevelSource);
		matTransfer.SetFloat("_Level2", maxlevelTarget);
		
		if (settings.colorSpace == NprColorTransferSettings.COLORSPACE.RGB) {
			Graphics.Blit(source, destination, matTransfer);
		} else {
			RenderTexture final_colourspace=RenderTexture.GetTemporary(source.width, source.height, 0, getBufferFormat());
			Graphics.Blit(source, final_colourspace, matTransfer);
			Graphics.Blit(final_colourspace, destination, getMatFromSpce());
			RenderTexture.ReleaseTemporary(final_colourspace);
		}
	}

	protected void ComputeImageStatistics(Texture source, ref RenderTexture mean, ref RenderTexture variance) {
		if (source == null) return;
		int width = settings.resolution, height = settings.resolution;
		if (mean == null)
			mean = RenderTexture.GetTemporary(width, height, 1, getBufferFormat());
		mean.useMipMap=true;

		if (variance == null)
			variance = RenderTexture.GetTemporary(width, height, 1, getBufferFormat());
		variance.useMipMap=true;

		if (settings.colorSpace == NprColorTransferSettings.COLORSPACE.RGB) {
			Graphics.Blit(source, mean);
		} else
			Graphics.Blit(source, mean, getMatToSpce());

		int maxlevel = (int)Mathf.Log (Mathf.Max (source.width, source.height), 2);
		Material matVariance = materials[PIXELVARIANCE];
		matVariance.SetFloat("_Level", maxlevel);
		
		Graphics.Blit(mean, variance, matVariance);
	}

	protected Material getMatToSpce() {
		switch (settings.colorSpace) {
		case NprColorTransferSettings.COLORSPACE.HSV:
			return materials[TO_HSV];
		case NprColorTransferSettings.COLORSPACE.ORGB:
			return materials[TO_ORGB];
		default:
			return materials[TO_HSV];
		}
	}

	protected Material getMatFromSpce() {
		switch (settings.colorSpace) {
		case NprColorTransferSettings.COLORSPACE.HSV:
			return materials[FROM_HSV];
		case NprColorTransferSettings.COLORSPACE.ORGB:
			return materials[FROM_ORGB];
		default:
			return materials[FROM_HSV];
		}
	}
	
	const int PIXELVARIANCE = 0;
	const int COLORTRANSFER = PIXELVARIANCE + 1;
	const int TO_HSV = COLORTRANSFER + 1;
	const int FROM_HSV = TO_HSV + 1;
	const int TO_ORGB = FROM_HSV + 1;
	const int FROM_ORGB = TO_ORGB + 1;

	NprColorTransferSettings.ColorTransferState transferState = null;
	RenderTexture targetMean = null, targetVariance = null;
}
