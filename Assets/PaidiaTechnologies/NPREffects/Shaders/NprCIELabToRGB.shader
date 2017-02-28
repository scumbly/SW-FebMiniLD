Shader "Hidden/NPR/CIELabToRGB" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	#include "NprCommonColorSpaceTransform.cginc"

	float4 frag_cielab2rgb (v2f i) : COLOR
	{
		float4 original = tex2D(_MainTex, i.uv);

		float3 XYZ = CIELab_to_XYZ(original.xyz);
		return float4(XYZ_to_RGB(XYZ),original.a);
	}

	ENDCG

	SubShader {
		Pass {
			Name "NPR_CIELAB_TO_RGB"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_cielab2rgb
			#include "UnityCG.cginc"
			ENDCG
		}
	}
	
	FallBack Off
}
