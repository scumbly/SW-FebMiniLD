Shader "Hidden/NPR/DOGEdge" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	#include "NprCommonFade.cginc"
	#include "NprCommonMath.cginc"
	
	uniform sampler2D _flowTex;
	
	uniform float _dogSigma;
	uniform float _dogSensitivity;
	uniform float _dogThreshold;
	uniform float _dogSharpness;
	uniform float _xdogSmoothing;
	
	float edgeLevel( float2 uv )
	{
		const float edgeLevels = 7.0f;
		return fadeLevel(uv,_depthScaler,_changeSpeed,true) * edgeLevels;
	}

	float dogSigmaSq( float2 uv )
	{
		float s = _dogSigma;
		if ( _depthBased != 0 )
		{
			float l = edgeLevel( uv );
			if ( l < 1 )
				s = 0.5f;
			else if ( l < 2 )
				s = 1.0f;
			else if ( l < 3 )
				s = 2.0f;
			else if ( l < 4 )
				s = 3.0f;
			else if ( l < 5 )
				s = 4.0f;
			else if ( l < 6 )
				s = 5.0f;
			else
				s = 6.0f;
		}
		return s*s;
	}
	
	float dogSensitivity( float2 uv )
	{
		float s = _dogSensitivity;
		if ( _depthBased != 0 )
		{
			float l = edgeLevel( uv );
			if ( l < 1 )
				s = 4.02f;
			else if ( l < 2 )
				s = 3.76f;
			else if ( l < 3 )
				s = 4.98f;
			else if ( l < 4 )
				s = 8.61f;
			else if ( l < 5 )
				s = 14.07f;
			else if ( l < 6 )
				s = 21.19f;
			else
				s = 29.95f;
		}
		return s;
	}
	
	// Directional difference-of-Gaussians
	// From  Winnemoeller et al. 2012: XDoG: An eXtended difference-of-Gaussians compendium including advanced image stylization
	half4 frag_dog_directional (v2f i) : COLOR
	{
		float3 result = 0;
		// getting gradient
		float2 dir = normalize( tex2D(_flowTex,i.uv).xy );

		// these weights could be precomputed, such as weight sum
		float w1 = - 1.0f/(2.0f*dogSigmaSq(i.uv));
		float w2 = - 1.0f/(2.0f*1.6f*dogSigmaSq(i.uv)); // see Marr et al. (1980) about 1.6
		float ws = 0.0f;

		const int filterSize = 9;
		const int filterHalf = filterSize / 2;
		float sensitivity = dogSensitivity(i.uv);
		// split the loop so the compiler can unroll it for larger filter size
		for ( int u = -filterHalf; u <= 0; u++ )
		{
			float2 coord = i.uv + u*dir*_MainTex_TexelSize.xy;
			float sample = tex2D(_MainTex, coord).x;
			float dSq = u*u; 	// dot(u*dir,u*dir), but dir is normalized
			// see the referred article: S = (1+p)*G_sigma - p*G_k*sigma
			float w = (1+sensitivity)*exp(dSq*w1) - sensitivity*exp(dSq*w2);
			result += w*sample;
			ws += w;
		}
		for ( int u = 1; u <= filterHalf; u++ )
		{
			float2 coord = i.uv + u*dir*_MainTex_TexelSize.xy;
			float sample = tex2D(_MainTex, coord).x;
			float dSq = u*u; 	// dot(u*dir,u*dir), but dir is normalized
			// see the referred article: S = (1+p)*G_sigma - p*G_k*sigma
			float w = (1+sensitivity)*exp(dSq*w1) - sensitivity*exp(dSq*w2);
			result += w*sample;
			ws += w;
		}

		result /= ws;
		return float4(result,1);
	}

	inline float2 getTangent (float2 texCoord)
	{
		float2 gradient = tex2D(_flowTex,texCoord).xy;
		gradient = normalize(float2(-gradient.y,gradient.x));
		return isnan(gradient) ? float2(1,0) : gradient;
	}

	// we will use this to split loops until figuring out how to avoid the corresponding compiler error
	inline void integrate_flowCurve( inout float2 c, inout float2 t, float denom, inout float ws, inout float result, int startIdx, int endIdx )
	{
		float w;
		float2 tt, tt2;
		for ( int i = startIdx; i <= endIdx; ++i )
		{
			w = gauss_1D(i,denom);
			result += w*tex2D(_MainTex,c).x;
			ws += w;

			tt = getTangent(c);
			tt = sign(dot(t,tt))*tt;
			tt2 = getTangent(c+0.5f*tt);
			t = sign(dot(tt,tt2))*tt2;
			c += t*_MainTex_TexelSize.xy;
		}
	}
	
	// flow along tangent direction as in Kyprianidis et al. 2011 (Image and Video Abstraction by Coherence-Enhancing Filtering)
	// that is: 2nd order Runge-Kutta method to get line integral convolution
	float4 frag_flowcurve_tangent_filter (v2f i) : COLOR
	{
		float result = 0.0f;
		float ws = 0.0f, w = 0.0f;
		float2 t, tt, tt2; // tangent; temporary variables

		float denom = 1.0f / (2.0f*_xdogSmoothing*_xdogSmoothing);	
		// sampling positions
		float2 c = i.uv;
		
		w = gauss_1D(0.0f, denom);
		result += w*tex2D(_MainTex,c).x;
		ws += w;

		t = getTangent(i.uv);

		c = i.uv + t*_MainTex_TexelSize.xy;
		// unrolling loops manually
		integrate_flowCurve(c,t,denom,ws,result, 1, 3);
		t = -getTangent(i.uv);
		c = i.uv + t*_MainTex_TexelSize.xy;
		integrate_flowCurve(c,t,denom,ws,result, 1, 3);

		result /= ws;

		return float4(result,result,result,1);
	}
	
	// smooth step of the XDoG (see above)
	// From  Winnemoeller et al. 2012: XDoG: An eXtended difference-of-Gaussians compendium including advanced image stylization
	float4 frag_xdog_smoothstep (v2f i) : COLOR
	{
		float u = tex2D(_MainTex, i.uv).x;
		float epsilon = _dogThreshold;
		float phi = _dogSharpness;
		float3 result = u > epsilon ? 1.0f : 1.0f + tanh(phi*(u-epsilon));
		return float4(result,1.0f);
	}
	
	ENDCG
	
	SubShader {
		UsePass "Hidden/NPR/RGBToCIELab/NPR_RGB_TO_CIELAB"							// 0
		// 1: directional DoG (along gradient, which is stored in _flowTex.xy)
		Pass {
			Name "NPR_DOG_DIRECTIONAL"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_dog_directional
			#pragma target 3.0
			//#pragma glsl
			//#pragma profileoption NumInstructionSlots=1500
			//#pragma profileoption NumMathInstructionSlots=1500
			#include "UnityCG.cginc"
			
			ENDCG
		}
		// 2: smoothing along gradient direction
		Pass {
			Name "NPR_DOG_SMOOTHING"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_flowcurve_tangent_filter
			#pragma target 3.0
			//#pragma glsl
			//#pragma profileoption NumInstructionSlots=1500
			//#pragma profileoption NumMathInstructionSlots=1500
			#include "UnityCG.cginc"
			
			ENDCG
		}
		// 3: smoothstep pass of XDoG
		Pass {
			Name "NPR_XDOG_SMOOTHSTEP"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_xdog_smoothstep
			#include "UnityCG.cginc"
			
			ENDCG
		}
	} 
	FallBack Off
}
