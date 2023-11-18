// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Noah/Glass" {
	Properties{
		_DisplaceTex("Displacement Normal", 2D) = "white" {}
		_Magnitude("Magnitude", Range(0, 1)) = 1
		_Control("Control", Vector) = (0,0,0,0)
	}
	SubShader{

		// Queue is important! this object must be rendered after
		// Opaque objects.
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "DisableBatching" = "True" }
		LOD 100
		GrabPass{
			
		// "_BGTex"
		// if the name is assigned to the grab pass
		// all objects that use this shader also use a shared
		// texture grabbed before rendering them.
		// otherwise a new _GrabTexture will be created for each object.
		}
		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"

			
			struct appdata {
				float4 vertex : POSITION;
				float3 normal: NORMAL;
				float4 tangent : TANGENT;
				float2 uv : TEXCOORD0;

			};

			struct v2f {
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				// this is a slot to put our screen coordinates into
				// it is a float4 instead of float2
				// because we need to use tex2Dproj() instead of tex2D()
				float4 screenUV : TEXCOORD2;
				float3 tbn[3] : TEXCOORD3;

			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			// builtin variable to get Grabbed Texture if GrabPass has no name
			sampler2D _GrabTexture;

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				float3 normal = UnityObjectToWorldNormal(v.normal);
				float3 tangent = UnityObjectToWorldNormal(v.tangent);
				float3 bitangent = cross(tangent, normal);

				o.tbn[0] = tangent;
				o.tbn[1] = bitangent;
				o.tbn[2] = normal;

				// Draw fog in front.
				UNITY_TRANSFER_FOG(o,o.vertex);

				// Get screen coordinates between 0 and 1.
				o.screenUV = ComputeGrabScreenPos(o.vertex);
				return o;
			}

			sampler2D _DisplaceTex;
			float _Magnitude;
			float4 _Control;
			fixed4 frag(v2f i) : SV_Target{

				float3 tangentNormal = tex2D(_DisplaceTex, i.uv) * 2 - 1;
				float3 worldNormal = i.tbn[2];
				float3 compositeNormal = float3(i.tbn[0] * tangentNormal.r + i.tbn[1] * tangentNormal.g + i.tbn[2] * tangentNormal.b);
				float4 disp = float4(compositeNormal.x, compositeNormal.y, compositeNormal.z, 0);
				float4 grab = tex2Dproj(_GrabTexture, (i.screenUV + (disp * _Control.x)) + _Control);
				return grab;//float4(compositeNormal.x, compositeNormal.y, compositeNormal.z, 1);
			}
		ENDCG
		}
	}
}
