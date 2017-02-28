#ifndef NPR_COMMON_FADE_INC
#define NPR_COMMON_FADE_INC

sampler2D _CameraDepthTexture;

uniform int _depthBased; // 0 = off, 1 = depth-based, 2 = radial
uniform float _focus;
uniform float _changeSpeed;
uniform float _depthScaler;
uniform float _depthLevels;
uniform int _bothDirections;

float fadeLevel(float2 uv, float scaler, float exponent, bool invert = false)
{
	// each pixel follow the same path so dynamic branching is efficient here
	float pos = 0;
	pos = ( 1 == _depthBased ) ? Linear01Depth(tex2D(_CameraDepthTexture,uv).x) : 
								((0.5f-uv.x)*(0.5f-uv.x) + (0.5f-uv.y)*(0.5f-uv.y)) * 2;
	
	float distFromFocus = 0;
	// distance of the pixel level (e.g. pixel depth) from the focus level (e.g. focus depth)
	// division by 4 is an empirical value (a.k.a. black magic)
	distFromFocus = pos <= _focus ? 
		(_bothDirections ? (_focus-pos) / _focus / 4 : 0 ) :
		(pos-_focus) / (1.0f-_focus);
	
	// some effects use pow(1-d,x) instead of pow(d,x)
	distFromFocus = invert ? 1.0f - distFromFocus : distFromFocus;
	
	distFromFocus = pow(distFromFocus,exponent)*scaler;
	distFromFocus = _depthLevels > 1 ? distFromFocus - fmod(distFromFocus,1/_depthLevels) : distFromFocus;
	
	return distFromFocus;
}

#endif