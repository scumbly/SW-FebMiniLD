Shader "Hidden/NPR/MirrorY" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"

	uniform sampler2D _MainTex;
	uniform float4 _MainTex_TexelSize;

	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};

	v2f vert( appdata_img v )
	{
		v2f o;
		o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
		float2 uv = MultiplyUV( UNITY_MATRIX_TEXTURE0, v.texcoord );
		o.uv = uv;
		o.uv.y = 1-o.uv.y;
		return o;
	}
	
	float4 frag_mirror_y (v2f i) : COLOR
	{
		return tex2D(_MainTex,i.uv);
	}
	

	ENDCG

	SubShader {
		Pass {
			NAME "NPR_MIRROR_Y"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_mirror_y
			#include "UnityCG.cginc"
			
			ENDCG
		}
	} 
	FallBack Off
}
