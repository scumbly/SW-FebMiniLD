Shader "Hidden/NPR/GeometricEdge" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	#include "NprCommonFade.cginc"
	
	sampler2D _CameraDepthNormalsTexture;
	uniform float _geometryEdgeOffset;
	uniform float _depthThreshold;
	uniform float _normalThreshold;
	
	// calculates depth and normal based edges
	float4 frag_depthNormalEdge (v2f i) : COLOR
	{
		float4 result = float4(0,0,0,1);

		float pixelDepth = Linear01Depth(tex2D(_CameraDepthTexture, i.uv).x);
		float offset;
		offset = _depthBased == 0 ? _geometryEdgeOffset : fadeLevel(i.uv,_depthScaler,_changeSpeed,true) * _geometryEdgeOffset;
		
		float4 s1 = tex2D(_CameraDepthNormalsTexture, i.uv + offset*float2(-1.0f*_MainTex_TexelSize.x,-1.0f*_MainTex_TexelSize.y));
		float4 s2 = tex2D(_CameraDepthNormalsTexture, i.uv + offset*float2( 1.0f*_MainTex_TexelSize.x,-1.0f*_MainTex_TexelSize.y));
		float4 s3 = tex2D(_CameraDepthNormalsTexture, i.uv + offset*float2(-1.0f*_MainTex_TexelSize.x, 1.0f*_MainTex_TexelSize.y));
		float4 s4 = tex2D(_CameraDepthNormalsTexture, i.uv + offset*float2( 1.0f*_MainTex_TexelSize.x, 1.0f*_MainTex_TexelSize.y));
		
		float d1 = tex2D(_CameraDepthTexture, i.uv + offset*float2(-1.0f*_MainTex_TexelSize.x,-1.0f*_MainTex_TexelSize.y)).x;
		float d2 = tex2D(_CameraDepthTexture, i.uv + offset*float2( 1.0f*_MainTex_TexelSize.x,-1.0f*_MainTex_TexelSize.y)).x;
		float d3 = tex2D(_CameraDepthTexture, i.uv + offset*float2(-1.0f*_MainTex_TexelSize.x, 1.0f*_MainTex_TexelSize.y)).x;
		float d4 = tex2D(_CameraDepthTexture, i.uv + offset*float2( 1.0f*_MainTex_TexelSize.x, 1.0f*_MainTex_TexelSize.y)).x;

		float d = abs(d1-d4) + abs(d2-d3);
		float2 n2 = (abs(s1-s4) + abs(s2-s3)).xy;
		float n = n2.x + n2.y;

		const float magicDistFromLinearThreshold = 0.05f;
		if ( 
			(d < _depthThreshold && 
			n < _normalThreshold)
			||
			((abs((Linear01Depth(d1)+Linear01Depth(d4))*0.5f - pixelDepth))/pixelDepth < magicDistFromLinearThreshold)
			||
			((abs((Linear01Depth(d2)+Linear01Depth(d3))*0.5f - pixelDepth))/pixelDepth < magicDistFromLinearThreshold)
		) // no edge
			result.xyz = 1;
		else // edge
			result.xyz = 0;

		return result;
	}
	
	ENDCG
	
	SubShader {
		Pass {
			Name "NPR_GEOMETRYEDGE"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_depthNormalEdge
			#pragma target 3.0
			#pragma glsl
			#include "UnityCG.cginc"
			
			ENDCG
		}
		// edge "intensity" is a small hack: it is implemented by blurring the binary edge values
		UsePass "Hidden/NPR/SmoothingFunctions/NPR_GAUSSIAN_X"
		UsePass "Hidden/NPR/SmoothingFunctions/NPR_GAUSSIAN_Y"
	} 
	FallBack Off
}
