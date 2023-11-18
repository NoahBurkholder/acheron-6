Shader "Funix/Hologram Shader" {
	Properties {
	    _InnerColor ("Inner Color", Color) = (0,0.28,0.02,1)
	    _OuterColor ("Outer Color", Color) = (0,1,0.13,1)
		_ColorIntensity ("Color Intensity", Range(0.1,10.0)) = 1.4
		_InOutRange ("In/Out", Range(0,1.0)) = 1.0

	    _MainTex ("Diffuse", 2D) = "white" {}
		_BumpMap ("Bumpmap", 2D) = "bump" {}

		_PatternTex ("Pattern (R)", 2D) = "white" {}
		_PatternScale ("Pattern Scale Factor", Range(0,100.0)) = 0.08
		_PatternOffsetX ("Pattern Offset X", float) = 0
		_PatternOffsetY ("Pattern Offset Y", float) = 0
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 200
		Lighting Off
		Cull Off 

		Pass {
			ZWrite On
			ColorMask 0
		}

	    CGPROGRAM
		#pragma surface surf Unlit alpha vertex:vert

		half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
	    {
	    	return half4(s.Albedo, s.Alpha);
	    }

		struct Input {
			float2 uv_MainTex;
			float3 viewDir;
			float2 localPos;
	    };
	        
		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _PatternTex;
		fixed4 _InnerColor;
		float4 _OuterColor;
		float _ColorIntensity;
		float _InOutRange;
		float _PatternOffsetX;
		float _PatternOffsetY;
		float _PatternScale;


		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.localPos = mul(UNITY_MATRIX_IT_MV, v.vertex);
		}

	    void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _InnerColor;

			o.Albedo = c.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

			half intensity = abs(_InOutRange - saturate(dot (normalize(IN.viewDir), o.Normal)));

			float2 screenUV = IN.localPos.xy;
			half4 pattern = tex2D(_PatternTex, float2( (screenUV.x /(_PatternScale*0.1)) + _PatternOffsetX, (screenUV.y /(_PatternScale*0.1)) + _PatternOffsetY  ) );

			o.Emission = pattern * _OuterColor.rgb * pow (intensity, _ColorIntensity);
			o.Alpha = pattern * pow (intensity, _ColorIntensity ) * _OuterColor.a;
	    }
	    ENDCG
	}

	Fallback "Transparent/VertexLit"
}