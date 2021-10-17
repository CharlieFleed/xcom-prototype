// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/StandardOccluded" 
{
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_OccludedColor("Occluded Color", Color) = (1,1,1,1)
	}

		SubShader{
			Tags{ "RenderType" = "Opaque" }
			LOD 200
			ZTest LEqual

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _MainTex;

			struct Input {
				float2 uv_MainTex;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			void surf(Input IN, inout SurfaceOutputStandard o) {
				// Albedo comes from a texture tinted by color
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
			}
			ENDCG

			Pass
			{
				ZTest Greater
				Cull Back

				CGPROGRAM
				#pragma vertex vert            
				#pragma fragment frag

				half4 _OccludedColor;

				float4 vert(float4 pos : POSITION) : SV_POSITION
				{
					float4 viewPos = UnityObjectToClipPos(pos);
					return viewPos;
				}

				half4 frag(float4 pos : SV_POSITION) : COLOR
				{
					return _OccludedColor;
				}

				ENDCG
			}
		}
			FallBack "Diffuse"
}

