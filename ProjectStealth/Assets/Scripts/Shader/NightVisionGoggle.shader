// "Night Vision Goggles" effect shader
// - Gabriel Violette
Shader "Hidden/NVG"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		// color1, color2, width
	}
	SubShader
	{
		// No culling or depth
		Cull Off 
		ZWrite Off 
		ZTest Always
		Blend One OneMinusSrcAlpha

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;

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

			fixed4 frag( v2f IN ): SV_Target
			{
				fixed p = IN.uv.y;
				fixed4 color = tex2D( _MainTex, IN.uv );

				fixed line_width = 2.0;
				fixed mod = (uint) ( p * _ScreenParams.y / line_width ) % 4;
				fixed factor = 0.5;
				if ( mod == 0 ) { factor = 0; }
				if ( mod == 2 ) { factor = 1; }

				color.rgb *= lerp( fixed3( 0.25, 0.75, 0.25), fixed3( 0.5, 1.0, 0.5 ), factor );

				return color;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
