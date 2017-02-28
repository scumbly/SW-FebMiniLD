Shader "Hidden/NPR/SimplificationSmooth" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		UsePass "Hidden/NPR/SmoothingFunctions/NPR_BILATERALFILTER_DIRECTIONAL"
	} 
	FallBack Off
}
