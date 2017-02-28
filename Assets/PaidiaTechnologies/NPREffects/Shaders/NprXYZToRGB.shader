Shader "Hidden/NPR/XYZToRGB" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	#include "NprCommonColorSpaceTransform.cginc"

	float4 frag_xyz2rgb (v2f i) : COLOR
	{
		float4 original = tex2D(_MainTex, i.uv);
		return float4(XYZ_to_RGB(original.xyz),original.a);
	}

	ENDCG

	SubShader {
		Pass {
			Name "NPR_XYZ_TO_RGB"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_xyz2rgb
			#include "UnityCG.cginc"
			ENDCG
		}
	}
	
	FallBack Off
}
