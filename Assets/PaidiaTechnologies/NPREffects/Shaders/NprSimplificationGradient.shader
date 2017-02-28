Shader "Hidden/NPR/SimplificationGradient" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	
	float4 _mapping;
	
	// Derivative of the luminance in x-direction, implemented with Sobel-filter
	// Assumes that luminance is in the x component (red channel)
	float4 frag_lumgrad_x (v2f i) : COLOR
	{
		float4 original = tex2D(_MainTex, i.uv);
		float left = tex2D(_MainTex, i.uv - float2(_MainTex_TexelSize.x,0.0f)).x;
		float right = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x,0.0f)).x;
		float Gx = 
			-1.0f * left +
		//	 0.0f * original.x +
			 1.0f * right;
		float Gy = 
			1.0f * left +
			2.0f * original.x +
			1.0f * right;
		return float4(Gx,Gy,0,0);
	}

	// Derivative of the luminance in y-direction, implemented with Sobel-filter
	// Assumes that luminance is in the x component (red channel)
	float4 frag_lumgrad_y (v2f i) : COLOR
	{
		float4 original = tex2D(_MainTex, i.uv);
		float up = tex2D(_MainTex, i.uv - float2(0.0f,_MainTex_TexelSize.y)).x;
		float down = tex2D(_MainTex, i.uv + float2(0.0f,_MainTex_TexelSize.y)).x;
		float Gx = 
			1.0f * up +
			2.0f * original.x +
			1.0f * down;
		float Gy = 
			-1.0f * up +
		//	 0.0f * original.x +
			 1.0f * down;
		return float4(Gx,Gy,0,0);
	}
	
	// Computes magnitude and angle of the luminance gradient
	// assumes that gradients are computed and stored in x,y (red and green)
	float4 frag_lumgrad (v2f i) : COLOR
	{
		float2 gradient = tex2D(_MainTex, i.uv).xy;
		float magnitude = length(gradient);
		float angle = 0.0f;
		if ( 0.0f!=gradient.x || 0.0f!=gradient.y ) angle = atan2(gradient.x,gradient.y);
		return float4(gradient,magnitude,angle);
	}
	
	// clamps each component to the same range
	// range is given by [_mapping.x,_mapping.y]
	float4 frag_clamp_all (v2f i) : COLOR
	{
		float4 pixValue = tex2D(_MainTex, i.uv);
		pixValue = clamp(pixValue,_mapping.x,_mapping.y);
		return pixValue;
	}
	
	// linear mapping of each component to the same range
	// [_mapping.x,_mapping.y] defines range before, [_mapping.z,_mapping.w] range after the mapping
	float4 frag_linear_transform_all (v2f i) : COLOR
	{
		float4 pixValue = tex2D(_MainTex, i.uv);
		// map to [0,1]
		pixValue -= _mapping.x;
		pixValue *= _mapping.y-_mapping.x;
		// map to [_mapping.z,_mapping.w]
		pixValue = lerp(_mapping.z,_mapping.w,pixValue);
		return pixValue;
	}
	
	ENDCG

	SubShader {
		// 0: separable pass applied for X-direction
		Pass {
			Name "NPR_GRADIENT_X"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_lumgrad_x
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			ENDCG
		}
		// 1: separable pass applied for Y-direction
		Pass {
			Name "NPR_GRADIENT_Y"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_lumgrad_y
			#pragma fragmentoption ARB_precision_hint_fastest 
			#include "UnityCG.cginc"
			
			ENDCG
		}
		// 2: Computes gradient magnitude and angle
		// assumes that gradients are computed and stored in x,y (red and green)
		Pass {
			Name "NPR_GRADIENT_STATS"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_lumgrad
			#include "UnityCG.cginc"
			
			ENDCG
		}
		// 3: clamps each component to the same range [_mapping.x,_mapping.y]
		Pass {
			Name "NPR_CLAMP_ALL"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_clamp_all
			#include "UnityCG.cginc"
			ENDCG
		}
		// 4: linearly maps each component to the same range from [_mapping.x,_mapping.y] to [_mapping.z,_mapping.w]
		Pass {
			Name "NPR_LINEARMAP_ALL"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_linear_transform_all
			#include "UnityCG.cginc"
			ENDCG
		}
	} 
	FallBack Off
}
