Shader "Hidden/NPR/ExtractShadows" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ShadowTex ("ShadowTex", 2D) = "white" {}
		_maxShadowDist ("MaxShadowDist", Float) = 0
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"

	uniform sampler2D _ShadowTex;
	uniform float _maxShadowDist;
	uniform int _smoothShadow;
	
	float4 frag_shadowExtract (v2f i) : COLOR
	{
		float3 shadowedColor = tex2D(_ShadowTex,i.uv).xyz;
		float3 noShadows = tex2D(_MainTex,i.uv).xyz;
		float3 diff = 1.0f-length(noShadows-shadowedColor);
		float4 outColor = (_smoothShadow == 0) ?
				(length(noShadows-shadowedColor) < _maxShadowDist ? float4(1,1,1,1) : float4(0,0,0,1)) :
				float4(diff,1);
		return outColor;
	}

	ENDCG

	SubShader {
		Pass {
			NAME "NPR_SHADOW_IMAGE"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_shadowExtract
			#include "UnityCG.cginc"
			
			ENDCG
		}
	} 
	FallBack off
}
