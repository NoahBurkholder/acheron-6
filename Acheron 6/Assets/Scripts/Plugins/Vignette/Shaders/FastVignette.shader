  // Colorful FX - Unity Asset
// Copyright (c) 2015 - Thomas Hourdel
// http://www.thomashourdel.com

Shader "Hidden/Colorful/Fast Vignette"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Params ("Darkness (W)", Vector) = (0.3)
		_Color ("Vignette Color (RGB)", Color) = (0, 0, 0, 0)
	}

	CGINCLUDE

		#include "UnityCG.cginc"
		#include "./Colorful.cginc"

		sampler2D _MainTex;
		half4 _Params;
		half3 _Color;
		half4 _MainTex_ST;//To correct for VR screen splitting

		half4 frag(v2f_img i) : SV_Target
		{

			half4 color = tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv, _MainTex_ST));//sample the color from the image, using unity to adjust per eye

			half3 c = lerp(_Color, color.rgb, _Params.w);

			return half4(c, color.a);
		}

	ENDCG

	SubShader
	{
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }

		// (2) Colored
		Pass
		{
			CGPROGRAM

				#pragma vertex vert_img
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest 

			ENDCG
		}
	}

	FallBack off
}
