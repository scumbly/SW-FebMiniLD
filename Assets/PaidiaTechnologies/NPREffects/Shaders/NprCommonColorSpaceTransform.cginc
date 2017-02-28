#ifndef NPR_COLORSPACECOMMON_INC
#define NPR_COLORSPACECOMMON_INC

	float min3 (float r, float g, float b) {
		return min (r, min (g, b));
	}
	float max3 (float r, float g, float b) {
		return max (r, max (g, b));
	}

#ifndef F_EPSILON
#define F_EPSILON 0.0001f
#endif

	float3 rgb2hsv (float3 rgb)
	{ // HSV in [0,1]
	//http://www.easyrgb.com/index.php?X=MATH&H=20#text20
		
		float var_Min, var_Max, del_Max;
		float r=rgb.r; float g=rgb.g; float b=rgb.b;
		float h, s, v;
		var_Min = min3( r, g, b );
		var_Max = max3( r, g, b );
		del_Max = var_Max - var_Min;

		v = var_Max;
		
		if( del_Max == 0 )
		{
			h = 0;
			s = 0;
		}
		else
		{
			s = del_Max / var_Max;

			r = (((var_Max - r)/6.0) + (del_Max*0.5)) / del_Max;
			g = (((var_Max - g)/6.0) + (del_Max*0.5)) / del_Max;
			b = (((var_Max - b)/6.0) + (del_Max*0.5)) / del_Max;

			if ( abs(rgb.r-var_Max) < F_EPSILON )
				h = b - g;
			else if ( abs(rgb.g-var_Max) < F_EPSILON )
				h = 1/3.0 + r - b;
			else
				h = 2/3.0 + g - r;

			if( h < 0 )
				h += 1;
			if( h > 1 )
				h -= 1;
		}
		
		return float3(h, s, v);
	}

	float3 hsv2rgb (float3 hsv)
	{
		// http://www.easyrgb.com/index.php?X=MATH&H=21#text21
		int i;
		float h, s, v;
		float r, g, b;
		h = hsv.x;
		s = hsv.y;
		v = hsv.z;

		if ( s == 0 )
		{
			r = g = b = v;
		}
		else
		{
			h = h*6;
			if ( h == 6 ) h = 0;
			i = floor( h );
			float v1, v2, v3;
			v1 = v * (1 - s);
			v2 = v * (1 - s*(h-i));
			v3 = v * (1 - s*(1 - (h-i)));

			r = g = b = 0;
			if ( h < 1 )
			{
				r = v;
				g = v3;
				b = v1;
			}
			else if ( h < 2 )
			{
				r = v2;
				g = v;
				b = v1;
			}
			else if ( h < 3 )
			{
				r = v1;
				g = v;
				b = v3;
			}
			else if ( h < 4 )
			{
				r = v1;
				g = v2;
				b = v;
			}
			else if ( h < 5 )
			{
				r = v3;
				g = v1;
				b = v;
			}
			r = ( h >= 5 ) ? v : r;
			g = ( h >= 5 ) ? v1 : g;
			b = ( h >= 5 ) ? v2 : b;
		}
		return float3(r,g,b);
	}


	float3 rgb2hsl (float3 rgb)
	{ // HSL in [0,1]
	//http://www.easyrgb.com/index.php?X=MATH&H=18#text18
		
		float var_Min, var_Max, del_Max;
		float r=rgb.r; float g=rgb.g; float b=rgb.b;
		float h, s, l;
		var_Min = min3( r, g, b );
		var_Max = max3( r, g, b );
		del_Max = var_Max - var_Min;

		l = (var_Max + var_Min) / 2;
		
		if( del_Max == 0 )
		{
			h = 0;
			s = 0;
		}
		else
		{
			if ( l < 0.5f ) s = del_Max / ( var_Max + var_Min );
			else s = del_Max / ( 2 - var_Max - var_Min );

			r = (((var_Max - r)/6.0) + (del_Max*0.5)) / del_Max;
			g = (((var_Max - g)/6.0) + (del_Max*0.5)) / del_Max;
			b = (((var_Max - b)/6.0) + (del_Max*0.5)) / del_Max;

			if ( abs(rgb.r-var_Max) < F_EPSILON )
				h = b - g;
			else if ( abs(rgb.g-var_Max) < F_EPSILON )
				h = 1/3.0 + r - b;
			else
				h = 2/3.0 + g - r;

			if( h < 0 )
				h += 1;
			if( h > 1 )
				h -= 1;
		}
		
		return float3(h, s, l);
	}

	float Hue_2_RGB( float v1, float v2, float vH )             //Function Hue_2_RGB
	// http://www.easyrgb.com/index.php?X=MATH&H=19#text19
	{
		if ( vH < 0 ) vH += 1;
		else if ( vH > 1 ) vH -= 1;
		if ( ( 6 * vH ) < 1 ) return ( v1 + ( v2 - v1 ) * 6 * vH );
		if ( ( 2 * vH ) < 1 ) return ( v2 );
		if ( ( 3 * vH ) < 2 ) return ( v1 + ( v2 - v1 ) * ( ( 2.0f / 3.0f ) - vH ) * 6 );
		return ( v1 );
	}

	float3 hsl2rgb (float3 hsl)
	{
		// http://www.easyrgb.com/index.php?X=MATH&H=19#text19
		float h, s, l;
		float r, g, b;
		float var_1, var_2;
		h = hsl.x;
		s = hsl.y;
		l = hsl.z;

		if ( s == 0 )
		{
			r = g = b = l;
		}
		else
		{
			if ( l < 0.5 ) var_2 = l * ( 1 + s );
			else           var_2 = ( l + s ) - ( s * l );

			var_1 = 2 * l - var_2;

			r = Hue_2_RGB( var_1, var_2, h + ( 1.0f / 3.0f ) ) ;
			g = Hue_2_RGB( var_1, var_2, h );
			b = Hue_2_RGB( var_1, var_2, h - ( 1.0f / 3.0f ) );
		}
		return float3(r,g,b);
	}
	
	float3 RGB_to_XYZ( float3 rgb_vec )
	{
		float3x3 RGB2XYZ = {0.412453, 0.357580, 0.180423, 0.212671, 0.71516, 0.072169, 0.019334, 0.119193, 0.950227};
		return mul(RGB2XYZ, rgb_vec);
	}

	float3 XYZ_to_RGB( float3 xyz_vec )
	{
		float3x3 XYZ2RGB = {3.240479, -1.537150, -0.498535, -0.969256, 1.875992, 0.041556, 0.055648, -0.204043, 1.057311};
		return mul(XYZ2RGB, xyz_vec);
	}

	float CIELab_f( float x )
	{
		float result = 0.0f;
		if (x > 0.008856f) result = pow(x,1.0f/3.0f);
		else result = 7.787f*x + 16.0f/116.0f;
		return result;
	}

#ifndef D65_WHITE
#define D65_WHITE (float3(95.047f,100.0f, 108.883f))
#endif

	float3 XYZ_to_CIELab( float3 xyz_vec )
	{
		xyz_vec /= D65_WHITE;

		float fX = CIELab_f(xyz_vec.x);
		float fY = CIELab_f(xyz_vec.y);
		float fZ = CIELab_f(xyz_vec.z);

		return float3( (116.0f*fY-16.0f), 500.0f*(fX-fY), 200.0f*(fY-fZ)  );
	}

	float cbc(float x)
	{
		return x*x*x;
	}

	float CIELab_if(float x)
	{
		float result = cbc(x);
		if ( result <= 0.008856f ) result = (x-16.0f/116.0f)/7.787f;
		return result;
	}

	float3 CIELab_to_XYZ( float3 cielab_vec )
	{
		float P = (cielab_vec.x + 16.0f) / 116.0f;

		float3 XYZ = {
			CIELab_if(P + cielab_vec.y/500.0f),
			CIELab_if(P),
			CIELab_if(P - cielab_vec.z/200.0f)
		};

		return D65_WHITE*XYZ;
	}
	
	float3 RGB_to_XYZ_Normalized (float3 rgb_vec)
	{
		float3x3 trans={0.5141, 0.3239, 0.1604, 0.2651, 0.6702, 0.0641, 0.0241, 0.1228, 0.08444};
		return mul(trans, rgb_vec);
	}
	
	float3 XYZ_Normalized_to_LMS (float3 xyz_vec)
	{
		float3x3 trans={0.3897, 0.6890, -0.0787, -0.2298, 1.1834, 0.0464, 0, 0, 1};
		return mul(trans, xyz_vec);
	}
	
	float3 RGB_to_LMS (float3 rgb_vec)
	{
		float3x3 trans={0.3811, 0.5783, 0.0402, 0.1967, 0.7244, 0.0782, 0.0241, 0.1288, 0.8444};
		return mul(trans, rgb_vec);
	}	
	
	float3 log3 (float3 x) 
	{
		return float3 (log10(x.x), log10(x.y), log10(x.z));
	}
	
	float3 LMS_to_LogLMS (float3 lms)
	{
		return log3 (lms);
	}
	
	float3 LogLMS_to_lalphabeta (float3 loglms)
	{
		//diagonal 1/sqrt[3], 1/sqrt[6], 1/sqrt[2]
		float3x3 t1={0.577350269, 0,0, 0, 0.40824829, 0, 0, 0, 0.707106781};
		float3x3 t2={1,1,1, 1, 1, -2, 1, -1, 0};
		//t1*t2=
		float3x3 trans=mul(t1, t2);
		return mul (trans, loglms);
	}
	
	float3 lalphabeta_to_LogLMS (float3 lalphabeta)
	{
		float3x3 t1={0.577350269, 0,0, 0, 0.40824829, 0, 0, 0, 0.707106781};
		float3x3 t2={1,1,1, 1, 1, -1, 1, -2, 0};	
		float3x3 trans=mul(t2, t1);
		return mul (trans, lalphabeta);
	}
	
	float3 pow3 (float3 x)
	{
		return float3 (pow (10, x.x), pow (10, x.y), pow (10, x.z));
	}
	
	float3 LogLMS_to_LMS (float3 loglms) {
		return pow3 (loglms);
	}
	
	float3 LMS_to_RGB (float3 lms) {
		float3x3 trans={4.4679, -3.5873, 0.1193, -1.2186, 2.3809, -0.1624, 0.0497, -0.2439, 1.2045};
		return mul (trans, lms);
	}
	
	float3 RGB_to_Lalphabeta (float3 rgb_vec) 
	{
		float3 lms=RGB_to_LMS(rgb_vec);
		float3 loglms=LMS_to_LogLMS(lms);
		float3 lalphabeta=LogLMS_to_lalphabeta(loglms);
		return lalphabeta;
	}
	
	float3 Lalphabeta_to_RGB (float3 lalphabeta) 
	{
		float3 loglms=lalphabeta_to_LogLMS (lalphabeta);
		float3 lms=LogLMS_to_LMS (loglms);
		return LMS_to_RGB (lms);
	}
	
	inline float3 lineartransform (float3 v, float3 mymin, float3 mymax) 
	{
		float3 dif=mymax-mymin;
		return (v-mymin) / dif;
	}

	inline float3 fromlineartransform (float3 v,float3 mymin, float3 mymax ) 
	{
		float3 dif=mymax-mymin;
		return v * dif + mymin;
	}
	
		//rgh: even with hdr, range is too small.
	//Ranges seem to be approximately ([-5,0], [-1,1], [-1,1]), so clamping to [0,1] gives problems.
	//Use a linear transform
	//-5 gives precission issues, -2 gives more precission and clamped values are not noticeable.
	//#define RANGES const float3 myminlalphabeta=float3 (-5,-1,-1); const float3 mymaxlalphabeta=float3(0,1,1);
	#define RANGES const float3 myminlalphabeta=float3 (-2,-1,-1); const float3 mymaxlalphabeta=float3(0,1,1);
	
	float3 RGB_to_ORGB_P1 (float3 original) 
	{
		//rgh warning, LCC colour space is not clamped to [0,1]^3, but to [0,1]*[-1,1]^2. The same happens to ORGB. 
		//We will use a linear transform from [-1,1] to [0,1] for storage on GPU textures
		// 1st pass: RGB to L'C'C'
		float3x3 mat={{0.2990, 0.5870, 0.1140}, {0.5000, 0.5000, -1.000}, {0.8660, -0.8660, 0.0000}};
		float3 lcc = mul (mat, original);
		return lcc;	
	}
	
	float3 RGB_to_ORGB_P2 (float3 lcc) 
	{
		// 2nd pass: L'C'C' to ORGB
		float pi = 3.14159265358979323846264338327;
		float l=lcc.x;
		float2 c12=lcc.yz;
		float theta= abs(c12.y*c12.x) > F_EPSILON ? atan2 (c12.y, c12.x) : 0;
		float theta0;
		//rgh: Points below the yellow blue axis require a different treatment.
		if (theta <= pi /3 && theta >= -pi / 3)
			theta0=3.0/2.0*theta;
		else if (theta > pi /3 )
			theta0=pi/2 + 3.0/4.0* (theta - pi/3);
		else 
			theta0=(theta + pi/3)*3.0/4.0 - pi/2;
		
		float angle= theta0 - theta;
		float2x2 rot;
		float cosa, sina;
		sincos (angle, sina, cosa);
		
		float2 C;
		C.x=cosa*c12.x -sina*c12.y;
		C.y=sina*c12.x +cosa*c12.y;	

		C+=float2 (1,1); C/=2; // transform to [0,1]
		return float3 (l, C);
	}
	
	float3 RGB_to_ORGB (float3 original) 
	{
		float3 lcc = RGB_to_ORGB_P1 (original);
		float3 orgb = RGB_to_ORGB_P2 (lcc);
		return orgb;
	}	
	
	float3 ORGB_to_RGB_P1 (float3 original) 
	{
		float pi = 3.14159265358979323846264338327;
		//http://ieeexplore.ieee.org/xpls/abs_all.jsp?arnumber=5246916&tag=1
		//rgh warning, LCC colour space is not clamped to [0,1]^3, but to [0,1]*[-1,1]^2. The same happens to ORGB. 
		//We will use a linear transform from [-1,1] to [0,1] for storage on GPU textures
		// 1st pass: ORGB to L'C'C'
		float l=original.x;
		float2 C=original.yz;
		C*=2; C-=float2(1,1);

		float theta0=abs(C.y*C.x) > F_EPSILON ? atan2 (C.y, C.x) : 0;
		
		float theta;
		//rgh: Points below the yellow blue axis require a different treatment.
		if (theta0 <= pi/2 && theta0 >= -pi/2)
			theta=2.0/3.0*theta0;
		else if (theta0 > pi/2)
			theta=pi/3.0 + 4.0/3.0*(theta0-pi/2.0);
		else 
			theta=(theta0+pi/2.0)*4.0/3.0 - pi/3.0;
		
		float angle=theta -theta0;
		float cosa, sina;
		sincos (angle, sina, cosa);
		
		float2 c12;
		c12.x=cosa*C.x -sina*C.y;
		c12.y=sina*C.x +cosa*C.y;
		
		float3 lcc=float3 (l, c12);	
		return lcc;
	}
	
	float3 ORGB_to_RGB_P2 (float3 lcc) 
	{	
		// 2nd pass: L'C'C' to RGB
		//float3 lcc=original;
		float3x3 mat={{1.000,0.1140, 0.7436}, {1.000, 0.1140, -0.4111}, {1.0000, -0.8660, 0.1663}};
		float3 myrgb = mul (mat, lcc);
		return myrgb; 
	}
	
	float3 ORGB_to_RGB (float3 original)
	{
		float3 lcc=ORGB_to_RGB_P1 (original);
		float3 myrgb=ORGB_to_RGB_P2 (lcc);
		return myrgb;
	}

#endif