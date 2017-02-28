Shader "Hidden/NPR/HSVToRGB" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	#include "NprCommonColorSpaceTransform.cginc"

	float4 frag_hsv2rgb (v2f i) : COLOR
	{
		float4 original = tex2D(_MainTex, i.uv);
		float3 rgb=hsv2rgb(original.xyz);
		return float4(rgb,original.a);
	}

	ENDCG

	SubShader {
		Pass {
			Name "NPR_HSV_TO_RGB"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_hsv2rgb
			#pragma target 3.0
			#include "UnityCG.cginc"
			ENDCG
		}
	}
	FallBack Off
}
