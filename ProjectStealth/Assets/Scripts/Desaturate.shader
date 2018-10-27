// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Desaturate"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Desaturate( "Desaturate", Range(0, 1) ) = 0
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex:   POSITION;
				float4 color:    COLOR;
				float2 texcoord: TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex:  SV_POSITION;
				fixed4 color:   COLOR;
				half2 texcoord: TEXCOORD0;
			};

			fixed4 _Color;

			v2f vert ( appdata_t IN )
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos( IN.vertex );
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				return OUT;
			}

			sampler2D _MainTex;
			uniform float _Desaturate;

			fixed4 frag( v2f IN ): COLOR 
			{
				float4 c = tex2D( _MainTex, IN.texcoord );
				float luminosity = c.r * 0.3 + c.g * 0.59 + c.b * 0.11;
				float3 greyscale = float3( luminosity, luminosity, luminosity );

				float4 result = c;
				result. rgb = lerp( c.rgb, greyscale, _Desaturate );
				return result;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
