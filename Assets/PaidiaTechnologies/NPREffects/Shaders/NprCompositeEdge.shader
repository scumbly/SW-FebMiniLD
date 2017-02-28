Shader "Hidden/NPR/CompositeEdge" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	#include "NprCommonComposite.cginc"

	float4 frag_draw_edges (v2f i) : COLOR
	{
		float4 result = tex2D(_BackGroundTex,TRANSFORM_TEX(i.uv,_BackGroundTex));
		result = lerp(edgeColorV(i.uv),result,edgeIntensity(i.uv).x);
		return result;
	}

	ENDCG

	SubShader {
		Pass {
			NAME "NPR_EDGES"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_draw_edges
			#include "UnityCG.cginc"
			
			ENDCG
		}
	} 
	FallBack Off
}
