Shader "Hidden/NPR/LalphabetaToRGB" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	#include "NprCommonColorSpaceTransform.cginc"

	float4 frag_lalphabeta2rgb (v2f i) : COLOR
	{
		RANGES
		float4 original = tex2D(_MainTex, i.uv);
		float3 data=fromlineartransform(original.xyz, myminlalphabeta, mymaxlalphabeta);
		return float4(Lalphabeta_to_RGB(data),original.a); 
	}

	ENDCG

	SubShader {
		Pass {
			Name "NPR_LALPHABETA_TO_RGB"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_lalphabeta2rgb
			#include "UnityCG.cginc"
			ENDCG
		}
	}
	
	FallBack Off
}
