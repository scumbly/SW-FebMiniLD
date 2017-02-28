Shader "Hidden/NPR/PixelVariance" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Level ("Level", Float) =0
		_scale ("scale", Float) = 10
	}
	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
		
	uniform float _Level;
	uniform float _scale;

	float4 frag_Variance (v2f i) : COLOR
	{
		float4 original = tex2D(_MainTex, i.uv);
		float4 mean = tex2Dlod(_MainTex, float4 (0.5,0.5 , 0, _Level));

		float4 ddd=tex2Dlod(_MainTex, float4 (i.uv , 0, 10));
		//if (_Level==0) return float4 (1,0,0,1);
		//else return float4(0,1,0,1);
		float4 temp=original-mean;
		temp*=temp; //rgh: this makes small numbers zero
		return temp*_scale;
	}
	
	ENDCG
	
	SubShader {
		Pass {
			Name "NPR_VARIANCE"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag_Variance
				//#pragma fragmentoption ARB_precision_hint_fastest 
				//#pragma exclude_renderers opengl gles
				#pragma target 3.0
			ENDCG
		}
	} 
FallBack off
}