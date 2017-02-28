#ifndef NPR_COMMON_MATH_INC
#define NPR_COMMON_MATH_INC

#ifndef TANH
#define TANH
	// alternatively: exclude gles renderer - http://forum.unity3d.com/threads/accessing-tanh-from-a-cg-shader-in-unity.85200/
	float tanh(float x)
	{
		float exp2x = exp(2*x);
		return (exp2x - 1) / (exp2x + 1);
	}
#endif

#ifndef GAUSS_1D
#define GAUSS_1D
	float gauss_1D(float t, float denom)
	{
		return exp(-t*t * denom);
	}
#endif

#endif