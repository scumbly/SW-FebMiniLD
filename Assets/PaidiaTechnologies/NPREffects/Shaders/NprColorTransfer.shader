Shader "Hidden/NPR/ColorTransfer" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_I1Mean ("I1Mean", 2D) = "white" {}
		_I1Var  ("I1Var", 2D) = "white" {}
		_I2Mean ("I2Mean", 2D) = "white" {}
		_I2Var  ("I2Var", 2D) = "white" {}		
		_Level1 ("Level1", Float) =0
		_Level2 ("Level2", Float) =0
		_scale ("scale", Float) = 10

	}
	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#include "NprCommonBasic.cginc"
		
	uniform sampler2D _I1Mean;
	uniform sampler2D _I1Var;
	uniform sampler2D _I2Mean;
	uniform sampler2D _I2Var;
	uniform float _Level1;
	uniform float _Level2;
	uniform float _scale;
	
	float4 frag_Transfer (v2f i) : COLOR
	{	
		float4 i1m = tex2Dlod(_I1Mean, float4(0.5,0.5, 0, _Level1));
		float4 i2m = tex2Dlod(_I2Mean, float4(0.5,0.5, 0, _Level2));
		float4 i1v = tex2Dlod(_I1Var, float4(0.5,0.5, 0, _Level1));
		float4 i2v = tex2Dlod(_I2Var, float4(0.5,0.5,0, _Level2));
		float4 mypixel=tex2D(_MainTex, i.uv);
	
		float4 temp=mypixel-i1m;
		float4 s;
		//rgh: avoid problems when variances are very small
		int c;
		for (c=0;c<4;c++)
			if (abs(i2v[c]-i1v[c])<0.01) s[c]=1;
			else s[c]=sqrt(i2v[c]/_scale)/sqrt(i1v[c]/_scale);
		//temp*=s;
		temp*=s;
		temp+=i2m;
		
		return float4 (temp.xyz, 1);
	}
	
	ENDCG
	
	SubShader {
		Pass {
			Name "NPR_COLOR_TRANSFER"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag_Transfer
				//#pragma fragmentoption ARB_precision_hint_fastest 
				#pragma target 3.0
			ENDCG
		}
	} 
FallBack off
}
