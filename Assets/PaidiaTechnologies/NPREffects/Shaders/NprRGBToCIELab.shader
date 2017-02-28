Shader "Hidden/NPR/RGBToCIELab" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	#include "NprCommonColorSpaceTransform.cginc"

	float4 frag_rgb2cielab (v2f i) : COLOR
	{
		float4 original = tex2D(_MainTex, i.uv);
		float3 XYZ = RGB_to_XYZ(original.xyz);
		return float4(XYZ_to_CIELab(XYZ),original.a);
	}

	ENDCG

	SubShader {
		Pass {
			Name "NPR_RGB_TO_CIELAB"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_rgb2cielab
			#include "UnityCG.cginc"
			ENDCG
		}
	}
	
	FallBack Off
}

