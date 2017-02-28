Shader "Hidden/NPR/CompositeShadow" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	#include "NprCommonComposite.cginc"

	float4 frag_shadow (v2f i) : COLOR
	{
		float4 result = tex2D(_MainTex,i.uv);
		bool shadowed = isShadowed(i.uv);
		result.xyz = shadowed ? shadowColor(i.uv) : result.xyz;
		return result;
	}

	ENDCG

	SubShader {
		Pass {
			NAME "NPR_SHADOW"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag_shadow
			#include "UnityCG.cginc"
			
			ENDCG
		}
	} 
	FallBack Off
}
