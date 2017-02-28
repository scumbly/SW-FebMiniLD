Shader "Hidden/NPR/CompositeEdgeShadow" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	#include "NprCommonComposite.cginc"

	float4 frag_shadow_and_edge (v2f i) : COLOR
	{
		float4 result = tex2D(_BackGroundTex,TRANSFORM_TEX(i.uv,_BackGroundTex));
		bool shadowed = isShadowed(i.uv);
		float3 edgeColor = lerp(edgeColorV(i.uv).xyz,shadowed?1:result.xyz,edgeIntensity(i.uv));
		result.xyz = shadowed ? shadowColorBG(i.uv) : 1;
		result.xyz = (!showEdgesInShadows() && shadowed) ? result.xyz : result.xyz*edgeColor;
		result.xyz = (isEdge(i.uv) && negativeEdges() && shadowed && showEdgesInShadows()) ? 1-result.xyz : result.xyz;
		return result;
	}
	
	ENDCG

	SubShader {
		Pass {
			NAME "NPR_SHADOWANDEDGE"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag_shadow_and_edge
			#include "UnityCG.cginc"
			
			ENDCG
		}
	} 
	FallBack Off
}
