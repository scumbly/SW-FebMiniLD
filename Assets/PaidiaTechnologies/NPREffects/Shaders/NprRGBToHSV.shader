Shader "Hidden/NPR/RGBToHSV" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	#include "NprCommonColorSpaceTransform.cginc"

	float4 frag_rgb2hsv (v2f i) : COLOR
	{
		float4 original = tex2D(_MainTex, i.uv);
		float3 hsv=rgb2hsv(original.xyz);
		return float4(hsv,original.a);
	}

	ENDCG

	SubShader {
		Pass {
			Name "NPR_RGB_TO_HSV"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_rgb2hsv
			#pragma target 3.0
			#include "UnityCG.cginc"
			ENDCG
		}
	}
	
	FallBack Off
}
