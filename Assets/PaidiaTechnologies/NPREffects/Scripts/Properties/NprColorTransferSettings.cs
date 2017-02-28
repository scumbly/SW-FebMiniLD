using UnityEngine;
using System.Collections;

[System.Serializable]
public class NprColorTransferSettings {
	public enum COLORSPACE {RGB, HSV, ORGB};

	public Texture2D [] source;
	public bool applyBeforeOtherEffects = false;
	public COLORSPACE colorSpace = COLORSPACE.HSV;
	
	public int currentStyle = 0;
	[Range(0,5)]
	public float varianceScale = 1;

	public Texture2D currentTexture {
		get {
			if (0 == source.Length ||
			    Mathf.Clamp(currentStyle,0,source.Length-1) != currentStyle)
				return null;
			return source[currentStyle];
		}
	}

	public int resolution {
		get { return (int)(Mathf.Pow(2.0f,quality + 4.0f) + 0.01f); }
	}

	public class ColorTransferState {
		public Texture currentTexture;
		public COLORSPACE colorSpace;
		public float varianceScale;
		public int quality;
	}

	public ColorTransferState stateCopy() {
		ColorTransferState s = new ColorTransferState();
		s.currentTexture = currentTexture;
		s.colorSpace = colorSpace;
		s.varianceScale = varianceScale;
		s.quality = quality;
		return s;

	}

	public bool SameState(ColorTransferState s) {
		if (null == s) return false;
		return s.currentTexture == currentTexture &&
			s.colorSpace == colorSpace &&
			s.varianceScale == varianceScale &&
			s.quality == quality;
	}

	[Range(1,5)]
	private int quality = 5; // affects the resolution of the mean/variance buffers, used for testing
}
