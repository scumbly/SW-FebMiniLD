Shader "Hidden/NPR/Desaturate" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	#include "NprCommonFade.cginc"

	float4 frag_depth_desaturate (v2f i) : COLOR
	{
		float4 outColor = tex2D(_MainTex,i.uv);
		outColor.y *= fadeLevel(i.uv,_depthScaler,_changeSpeed,true); // we are in HSV
		return outColor;
	}

	ENDCG

	SubShader {
		Pass {
			NAME "NPR_DESATURATE"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_depth_desaturate
			#include "UnityCG.cginc"
			
			ENDCG
		}
	} 
	FallBack off
}
