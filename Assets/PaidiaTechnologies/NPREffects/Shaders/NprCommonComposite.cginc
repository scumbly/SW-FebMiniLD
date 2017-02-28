#ifndef NPR_COMMON_COMPOSITE_INC
#define NPR_COMMON_COMPOSITE_INC

uniform sampler2D _ShadowImageTex;
uniform sampler2D _EdgeMapTex;
uniform sampler2D _BackGroundTex;

uniform float4 _BackGroundTex_ST;
uniform int _showShadowEdge;
uniform int _negativeEdges;

uniform float4 _edgeColor;
uniform int _useBackgroundEdgeColor;
uniform float4 _shadowColor;
uniform float _shadowHueOffset;
uniform float _shadowIntensityScaler;

#include "NprCommonColorSpaceTransform.cginc"

float3 shadowColor( float2 uv)
{
	float3 mainColor = tex2D(_MainTex,uv).xyz;

	// no real dynamic branching is here
	if ( _shadowHueOffset > 0.0f )
	{
		mainColor = rgb2hsl(mainColor);
		mainColor.x += frac(_shadowHueOffset);
		mainColor = hsl2rgb(mainColor);
	}

	float shadowColor = tex2D(_ShadowImageTex,uv).x;
	return lerp(mainColor,_shadowColor,_shadowIntensityScaler*(1-shadowColor));
}

float3 shadowColorBG( float2 uv)
{
	float3 mainColor = tex2D(_BackGroundTex,uv).xyz;

	// no real dynamic branching is here
	if ( _shadowHueOffset > 0.0f )
	{
		mainColor = rgb2hsl(mainColor);
		mainColor.x += frac(_shadowHueOffset);
		mainColor = hsl2rgb(mainColor);
	}

	float shadowColor = tex2D(_ShadowImageTex,uv).x;
	return lerp(mainColor,_shadowColor,_shadowIntensityScaler*(1-shadowColor));
}

bool isShadowed( float2 uv )
{
	float shadowIntensity = tex2D(_ShadowImageTex,uv).x;
	return shadowIntensity < 0.99f;
}

bool showEdgesInShadows()
{
	return _showShadowEdge != 0;
}

bool negativeEdges()
{
	return _negativeEdges != 0;
}

bool isEdge( float2 uv )
{
	return tex2D(_EdgeMapTex,uv).xyz < 0.999f;
}

float3 edgeIntensity( float2 uv ) // inverted, i.e. 0 means strongest edge
{
	return tex2D(_EdgeMapTex,uv).xyz;
}

float4 edgeColorV( float2 uv )
{
	// NOTE: color map is in _MainTex
	return _useBackgroundEdgeColor!=0 ? tex2D(_MainTex,uv) : _edgeColor;
}

#endif