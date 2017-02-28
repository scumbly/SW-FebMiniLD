Shader "Hidden/NPR/SimplificationQuantize" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	#include "NprCommonMath.cginc"
	
	uniform sampler2D _sharpnessMap;
	uniform float _bins;
	
	// luminance quantization with sharpness guided by a texture (z-comp, or blue channel), assumes that the x component contains luminance
	// based on Winnemoller et al. 2006, Real-time video abstraction
	float4 frag_cielab_quant_guided (v2f i) : COLOR
	{
		const float yMax = 10.0f;
		const float yMin = 0.0f;
		float dQ = (yMax-yMin) / _bins;
		float4 original = tex2D(_MainTex, i.uv);
		float Y = original.x;

		// round
		float remainder = fmod(Y-yMin,dQ);
		float qNearest = Y - remainder;
		if ( remainder > dQ*0.5f ) qNearest += dQ;

		float sharpness = tex2D(_sharpnessMap, i.uv).z;
		float arg = clamp(sharpness*(Y-qNearest),-10.0f,10.0f); // prevent overflow for large values
		Y = qNearest + dQ*0.5f*tanh(arg); // smooth transition

		return float4(Y, original.yzw);
	}

	ENDCG

	SubShader {
		Pass {
			Name "NPR_CIE_LAB_LUMINANCE_QUANTIZATION_SMOOTHED"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_cielab_quant_guided
			#include "UnityCG.cginc"
			ENDCG
		}
	}
	
	FallBack Off
}
