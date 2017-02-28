Shader "Hidden/NPR/SmoothingFunctions" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	#include "NprCommonMath.cginc"
	#include "NprCommonFade.cginc"

	uniform float _blurSigma;
	
	uniform sampler2D _StructureTensorTex;
	uniform float _pixelSpaceSigma;
	uniform float _imageSpaceSigma;
	uniform float _blurDepthScaler;
	uniform int _tangentialBlur;
	
	float imageSpaceSigmaSq(float2 uv)
	{		
		if (_depthBased == 0) return _imageSpaceSigma*_imageSpaceSigma;
					
		float fade = fadeLevel(uv,100.0f*(_depthScaler+1.9f),1/_changeSpeed*50.0f,false);
		// playing with the maths. for t in [0,1] s is defined as
		// lerp(_imageScapeSigma,_depthScaler*imageSpaceSigma,t)
		// assuming fade = t*_depthScaler the lerp is transformed as:
		// lerp(a,c*a,t) = ... = lerp( (1-t)/(1-ct)*a, a, c*t)
		float lowBound = (fade == 1.0f) ? 0 : (1-fade/_depthScaler)/(1-fade)*_imageSpaceSigma;
		float s = lerp(lowBound,_imageSpaceSigma,fade);
		s = max(s,0.1f);
		
		return s*s;
	}

#ifndef G_FILTER_SIZE
#define G_FILTER_SIZE 5
#endif

#ifndef G_FILTER_HALF
#define G_FILTER_HALF (G_FILTER_SIZE/2)
#endif

	float4 frag_gaussian_x (v2f i) : COLOR
	{
		float denom =  1.0f / (2.0f*_blurSigma*_blurSigma);
		float weightSum = 0.0f, weight;
		float4 result = 0.0f;

		for ( int u = -G_FILTER_HALF; u <= G_FILTER_HALF; ++u )
		{
			weight = gauss_1D(u,denom);
			result += weight*tex2D(_MainTex,i.uv + float2(u*_MainTex_TexelSize.x,0.0f));
			weightSum += weight;
		}
		result /= weightSum;

		return result;
	}
	
	float4 frag_gaussian_y (v2f i) : COLOR
	{
		float denom =  1.0f / (2.0f*_blurSigma*_blurSigma);
		float weightSum = 0.0f, weight;
		float4 result = 0.0f;
		
		for ( int v = -G_FILTER_HALF; v <= G_FILTER_HALF; ++v )
		{
			weight = gauss_1D(v,denom);
			result += weight*tex2D(_MainTex,i.uv + float2(0.0f,v*_MainTex_TexelSize.y));
			weightSum += weight;
		}
		result /= weightSum;

		return result;
	}
	
	half4 frag_bilateral_directional (v2f i) : COLOR
	{
		float3 original = tex2D(_MainTex, i.uv).xyz;
		float3 result = 0;
		// getting gradient
		float2 dir = normalize( tex2D(_StructureTensorTex,i.uv).xy );
		const float EPSILON = 0.0000001f;
		dir = abs(dir.x*dir.y) >= EPSILON ? dir : float2(1,0); // handle the case when the direction is zero
		// blur in the gradient or the tangent direction (global setting)
		if ( _tangentialBlur != 0 )
		{
			dir = dir.yx;
			dir.x *= -1.0f;
		}

		float weightSum = 0;
		float weight = 0;
		float imssq = imageSpaceSigmaSq(i.uv);	// depth-based blur is coded here
		const int filterSize = 5;
		const int filterHalf = filterSize / 2;
				
		for(int u=-filterHalf; u<=filterHalf; ++u)
		{
			float2 coord = i.uv + u*dir*_MainTex_TexelSize.xy;
			float3 sample = tex2D(_MainTex, coord).xyz;
			float3 dVecImg = sample-original;
			float dImgSq = dot(dVecImg,dVecImg);
			float dPixelSq = u*u;	// dot(u*dir,u*dir), but dir is normalized
			weight = 
				exp(-(dPixelSq)/2.0f/_pixelSpaceSigma/_pixelSpaceSigma)*
				exp(-(dImgSq)/2.0f/imssq);
			result += weight*sample;
			weightSum += weight;
		}
		result /= weightSum;

		return float4(result,1);
	}


	ENDCG

	SubShader {
		Pass {
			NAME "NPR_GAUSSIAN_X"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_gaussian_x
			#include "UnityCG.cginc"
			
			ENDCG
		}

		Pass {
			NAME "NPR_GAUSSIAN_Y"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_gaussian_y
			#include "UnityCG.cginc"
			
			ENDCG
		}
		
		Pass {
			Name "NPR_BILATERALFILTER_DIRECTIONAL"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_bilateral_directional
			#pragma fragmentoption ARB_precision_hint_fastest 

			#pragma glsl
			#pragma target 3.0
			#pragma profileoption NumInstructionSlots=1500
			#pragma profileoption NumTemps=13
			#pragma profileoption NumMathInstructionSlots=1500
			#include "UnityCG.cginc"
			ENDCG
		}
	}
	FallBack Off
}
