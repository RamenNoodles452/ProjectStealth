Shader "Sprites/Outline"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Distance( "Distance", Float ) = 1
		_OutlineColor( "Color", Color) = ( 1, 1, 1, 1 )
		[PerRendererData] _AlphaTex( "External Alpha", 2D ) = "white" {}
		[PerRendererData] _EnableExternalAlpha( "Enable External Alpha", Float ) = 0
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		// No culling or depth
		Cull Off 
		Lighting Off
		ZWrite Off 
		Blend SrcAlpha OneMinusSrcAlpha
		Fog { Mode off }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;
			float4 _MainTex_TexelSize;
			float _Distance;
			fixed4 _OutlineColor;


			struct appdata_t
			{
				float4 vertex: POSITION;
				fixed4 color:  COLOR;
				float2 uv:     TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex: SV_POSITION;
				fixed4 color:  COLOR;
				float2  uv:    TEXCOORD0;
			};

			v2f vert ( appdata_t IN )
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos( IN.vertex );
				OUT.uv     = IN.uv;
				OUT.color  = IN.color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap( OUT.vertex );
				#endif
				return OUT;
			}

			// Gets the alpha of a UV coordinate
			float alpha( v2f IN, float distance, float2 offset )
			{
				float a = tex2D( _MainTex, IN.uv + distance * offset ).a;
				#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if ( _AlphaSplitEnabled )
				{
					a = tex2D( _AlphaTex, IN.uv ).r;
				}
				#endif
				return a;
			}

			// Makes an outline in the specified color
			fixed4 frag( v2f IN ): SV_Target
			{
				// Sobel operation
				float distance = _MainTex_TexelSize.xy * _Distance;

				float upper_left  = alpha( IN, distance, float2( -1,  1) );
				float left        = alpha( IN, distance, float2( -1,  0) );
				float lower_left  = alpha( IN, distance, float2( -1, -1) );
				float upper_right = alpha( IN, distance, float2(  1,  1) );
				float right       = alpha( IN, distance, float2(  1,  0) );
				float lower_right = alpha( IN, distance, float2(  1, -1) );
				float up          = alpha( IN, distance, float2(  0, -1) );
				float down        = alpha( IN, distance, float2(  0,  1) );

				float gx = - upper_left - 2 * left - lower_left + upper_right + 2 * right + lower_right;
				float gy = - lower_left - 2 * down - lower_right + upper_left + 2 * up + upper_right;
				fixed4 source = tex2D( _MainTex, IN.uv );
				source.a = alpha( IN, distance, float2( 0, 0 ) );
				return fixed4( lerp( source.rgba, _OutlineColor.rgba, sqrt( gx * gx + gy * gy ) / 4 ) );
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
