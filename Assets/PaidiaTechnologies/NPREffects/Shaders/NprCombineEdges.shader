Shader "Hidden/NPR/CombineEdges" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	
	uniform sampler2D _OtherTex;
	
	// simply takes the minimum of the 2 textures
	float4 frag_min (v2f i) : COLOR
	{
		float4 result = min( tex2D(_OtherTex,i.uv), tex2D(_MainTex,i.uv) );
		return result;
	}
	
	ENDCG
	
	SubShader {
		Pass {
			NAME "NPR_MIN"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_min
			#include "UnityCG.cginc"
			
			ENDCG
		}
	} 
	FallBack Off
}
