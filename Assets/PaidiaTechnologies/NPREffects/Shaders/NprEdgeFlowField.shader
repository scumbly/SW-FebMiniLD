Shader "Hidden/NPR/EdgeFlowField" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
	
	// Structure tensor of an RGB image
	// NOTE: although the Sobel filter it uses is separable, structure tensor computation
	//       would require a lot of temporary data per pixel, thus, it is implemented in one pass
	float4 frag_structure_tensor (v2f i) : COLOR
	{
		// derivatives in x, y (or u, v) directions 
		float3 dfdx = 0.0f;
		float3 dfdy = 0.0f;

		// Sobel filter to get directional derivatives of R, G, B
		dfdx += -1*tex2D(_MainTex, i.uv + float2(-1.0f*_MainTex_TexelSize.x,-1.0f*_MainTex_TexelSize.y)).xyz;
		dfdy += -1*tex2D(_MainTex, i.uv + float2(-1.0f*_MainTex_TexelSize.x,-1.0f*_MainTex_TexelSize.y)).xyz;
		dfdx += -2*tex2D(_MainTex, i.uv + float2(-1.0f*_MainTex_TexelSize.x, 0.0f*_MainTex_TexelSize.y)).xyz;
	//	dfdy +=  0*tex2D(_MainTex, i.uv + float2(-1.0f*_MainTex_TexelSize.x, 0.0f*_MainTex_TexelSize.y)).xyz;
		dfdx += -1*tex2D(_MainTex, i.uv + float2(-1.0f*_MainTex_TexelSize.x, 1.0f*_MainTex_TexelSize.y)).xyz;
		dfdy +=  1*tex2D(_MainTex, i.uv + float2(-1.0f*_MainTex_TexelSize.x, 1.0f*_MainTex_TexelSize.y)).xyz;

	//	dfdx +=  0*tex2D(_MainTex, i.uv + float2( 0.0f*_MainTex_TexelSize.x,-1.0f*_MainTex_TexelSize.y)).xyz;
		dfdy += -2*tex2D(_MainTex, i.uv + float2( 0.0f*_MainTex_TexelSize.x,-1.0f*_MainTex_TexelSize.y)).xyz;
	//	dfdx +=  0*tex2D(_MainTex, i.uv + float2( 0.0f*_MainTex_TexelSize.x, 0.0f*_MainTex_TexelSize.y)).xyz;
	//	dfdy +=  0*tex2D(_MainTex, i.uv + float2( 0.0f*_MainTex_TexelSize.x, 0.0f*_MainTex_TexelSize.y)).xyz;
	//	dfdx +=  0*tex2D(_MainTex, i.uv + float2( 0.0f*_MainTex_TexelSize.x, 1.0f*_MainTex_TexelSize.y)).xyz;
		dfdy +=  2*tex2D(_MainTex, i.uv + float2( 0.0f*_MainTex_TexelSize.x, 1.0f*_MainTex_TexelSize.y)).xyz;

		dfdx +=  1*tex2D(_MainTex, i.uv + float2( 1.0f*_MainTex_TexelSize.x,-1.0f*_MainTex_TexelSize.y)).xyz;
		dfdy += -1*tex2D(_MainTex, i.uv + float2( 1.0f*_MainTex_TexelSize.x,-1.0f*_MainTex_TexelSize.y)).xyz;
		dfdx +=  2*tex2D(_MainTex, i.uv + float2( 1.0f*_MainTex_TexelSize.x, 0.0f*_MainTex_TexelSize.y)).xyz;
	//	dfdy +=  0*tex2D(_MainTex, i.uv + float2( 1.0f*_MainTex_TexelSize.x, 0.0f*_MainTex_TexelSize.y)).xyz;
		dfdx +=  1*tex2D(_MainTex, i.uv + float2( 1.0f*_MainTex_TexelSize.x, 1.0f*_MainTex_TexelSize.y)).xyz;
		dfdy +=  1*tex2D(_MainTex, i.uv + float2( 1.0f*_MainTex_TexelSize.x, 1.0f*_MainTex_TexelSize.y)).xyz;

		dfdx /= 4; dfdy /= 4;

		// see "Image Abstraction by Structure Adaptive Filtering" Kyprianidis et al. (2008) for notations

		// structure tensor matrix (E F; F G)
		float E = dot(dfdx,dfdx);
		float F = dot(dfdx,dfdy);
		float G = dot(dfdy,dfdy);

		return float4(E,F,G,0);
	}
	
	inline float4 getGradientAndTangentFromStructureTensor( float3 structureTensor )
	{
		// see "Image Abstraction by Structure Adaptive Filtering" Kyprianidis et al. (2008) for notations
		float E, F, G;
		E = structureTensor.x;
		F = structureTensor.y;
		G = structureTensor.z;

		// eigenvalues
		float lambda1 = (E + G + sqrt( (E-G)*(E-G) + 4*F*F )) * 0.5f;
		float lambda2 = (E + G - sqrt( (E-G)*(E-G) + 4*F*F )) * 0.5f;

		// eigenvectors: v1 is the gradient, v2 is the tangent orientation (without sign/direction)
		float2 v1 = float2(F, lambda1-E);
		float2 v2 = float2(lambda2-G, F);
		// minor change in the formula: the original one fails when dfdx = 0 or dfdy = 0
		// dfdx=0 means E=F=lambda2=0, G=lambda1; dfdy=0 means F=G=lambda2=0, E=lambda1
		// dfdx=dfdy=0 implies that lambda1=0, so gradient is properly zero
		v1 = E != 0.0f ? v1 : float2(0,(lambda1));
		v2 = E != 0.0f ? v2 : float2(0,0);
		v1 = G != 0.0f ? v1 : float2((lambda1),0);
		v2 = G != 0.0f ? v2 : float2(0,0);
		
		v1 = sqrt(lambda1) * normalize(v1);
		v2 = sqrt(lambda2) * normalize(v2);

		return float4(v1,v2);
	}

	// generates the flor field (gradient and tangent vectors) from the tensor matrix
	float4 frag_structure_tensor_flow_field (v2f i) : COLOR
	{
		float E, F, G;
		float3 structureTensor = tex2D(_MainTex,i.uv).xyz;
		return getGradientAndTangentFromStructureTensor(structureTensor);
	}
	
	ENDCG
	
	SubShader {
		// 0: structure tensor matrix of an RGB image
		Pass {
			Name "NPR_STRUCTURE_TENSOR"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_structure_tensor
			#pragma target 3.0
			#include "UnityCG.cginc"
			
			ENDCG
		}
		// 1-2: separable Gaussian blur on the tensor matrix
		UsePass "Hidden/NPR/SmoothingFunctions/NPR_GAUSSIAN_X"
		UsePass "Hidden/NPR/SmoothingFunctions/NPR_GAUSSIAN_Y"
		// 3: flow field from the tensor matrix
		Pass {
			Name "NPR_STRUCTURE_TENSOR_FLOW_FIELD"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_structure_tensor_flow_field
			#include "UnityCG.cginc"
			
			ENDCG
		}
	} 
	FallBack off
}
