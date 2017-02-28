Shader "Hidden/NPR/CompositeAll" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	#include "NprCommonComposite.cginc"

	float4 frag_edge_and_shadow (v2f i) : COLOR
	{
		float4 result = tex2D(_MainTex,i.uv);
		bool shadowed = isShadowed(i.uv);
		result.xyz = shadowed ? shadowColor(i.uv) : result.xyz;
		float3 edgeColor = lerp(_edgeColor.xyz,result.xyz,edgeIntensity(i.uv));
		edgeColor = shadowed ? lerp(result.xyz,edgeColor,0.5f) : edgeColor;
		result.xyz = (showEdgesInShadows() || !shadowed) ? edgeColor : result.xyz;
		return result;
	}

	ENDCG

	SubShader {
		Pass {
			NAME "NPR_COMPOSITE_ALL"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag_edge_and_shadow
			#include "UnityCG.cginc"
			
			ENDCG
		}
	} 
	FallBack Off
}

