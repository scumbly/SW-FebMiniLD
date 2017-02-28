#ifndef NPR_COMMON_BASIC_INC
#define NPR_COMMON_BASIC_INC

uniform sampler2D _MainTex;
uniform float4 _MainTex_TexelSize;

struct v2f 
{
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;
};
	
v2f vert( appdata_img v )
{
	v2f o;
	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	float2 uv = MultiplyUV( UNITY_MATRIX_TEXTURE0, v.texcoord );
	o.uv = uv;
#if UNITY_UV_STARTS_AT_TOP
	if (_MainTex_TexelSize.y < 0)
        o.uv.y = 1-o.uv.y;
#endif
	return o;
}

#endif