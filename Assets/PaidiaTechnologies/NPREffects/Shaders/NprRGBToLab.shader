Shader "Hidden/NPR/RGBToLalphabeta" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	#include "NprCommonColorSpaceTransform.cginc"
	
	float4 frag_rgb2lalphabeta (v2f i) : COLOR
	{
		RANGES
		float4 original = tex2D(_MainTex, i.uv);
		float3 lalphabeta=RGB_to_Lalphabeta(original.xyz);
		float3 norm=lineartransform(lalphabeta, myminlalphabeta, mymaxlalphabeta);
		return float4(norm,original.a);
	}

	ENDCG

	SubShader {
		Pass {
			Name "NPR_RGB_TO_LALPHABETA"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_rgb2lalphabeta
			#include "UnityCG.cginc"
			ENDCG
		}
	}
	
	FallBack Off
}

