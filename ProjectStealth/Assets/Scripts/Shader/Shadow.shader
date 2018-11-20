// Reads shadow maps, and draws lighting / shadowing.
Shader "Sprites/Shadow"
{
	Properties
	{
		[PerRendererData] _ShadowMap ( "ShadowMap", 2D ) = "white" {}
		[PerRendererData] _BlendTarget( "BlendTarget", 2D ) = "black" {}
		[PerRendererData] _Color( "Color", Color ) = ( 1, 1, 1, 1 )
		[PerRendererData] _LightPosition( "LightPosition", Vector ) = ( 0, 0, 1, 0 )
		[PerRendererData] _ShadowMapY( "ShadowMapY", Float ) = 0
		[PerRendererData] _Range( "Range", Float ) = 512
	}
	SubShader
	{
		// No culling or depth
		Cull Off 
		Lighting Off
		ZWrite Off 
		ZTest Always
		BlendOp Max // Blends with consecutive passes using max (prevents super-stacking of multiple lights to absurd brightness).
		//Blend One One // Additive

		Tags
		{
			"Queue" = "Geometry-1"
			"IgnoreProjector" = "True"
			"RenderType" = "Opaque" 
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			// Properties
			sampler2D _ShadowMap;
			sampler2D _BlendTarget;
			float4    _LightPosition;
			float     _ShadowMapY;
			fixed4    _Color;
			float     _Range;

			// Vertex shader input
			struct appdata
			{
				float4 vertex : POSITION;  // world space
				float2 uv     : TEXCOORD0; // used to blend with existing shadows from _BlendTarget
			};

			// Vertex shader output, fragment shader input
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv     : TEXCOORD0;
				float2 world_position : TEXCOORD1; // nonsense semantic
			};

			// Utility
			// Reads the value of the shadow map for the given light at the given position
			// shadow_sampler reads from the shadow map texture.
			// sample_position is  x = angle y = distance pair
			// v is the "light key", which row in the shadow map corresponds to this light.
			float SampleShadowTexture( sampler2D shadow_sampler, float2 sample_position, float v )
			{
				float u = (sample_position.x + UNITY_PI) / ( UNITY_PI * 2.0 ); // convert polar angle to shadow texture x
				float max_cast_distance = 512.0; // what distance corresponds with the shadow map's 1 value
				// step = 1 if min distance from shadow map >= distance from sample_position, 0 otherwise
				float total = step( sample_position.y, tex2D( shadow_sampler, float2( u, v ) ).r * max_cast_distance );
				// multi-sample from around the point for blurring.
				total += step( sample_position.y, tex2D( shadow_sampler, float2( u - (1.0 / 1080.0 ), v ) ).r * max_cast_distance );
				total += step( sample_position.y, tex2D( shadow_sampler, float2( u + (1.0 / 1080.0 ), v ) ).r * max_cast_distance );
				return total / 3.0;
			}

			// converts 2 points into the angle and distance from the centerpoint to the other point.
			float2 Polar( float2 xy, float2 center_point )
			{
				float2 delta = xy - center_point;
				float distance = length( delta );
				float angle = atan2( delta.y, delta.x );
				return float2( angle, distance );
			}

			// vertex shader
			v2f vert (appdata v)
			{
				v2f o;
				o.world_position = v.vertex; // store world space coordinate, b/c you need it for distance calculations
				o.vertex = UnityObjectToClipPos( v.vertex ); // vertex needs to be in clip space, not world space to render properly.
				o.uv = v.uv;
				return o;
			}

			// Fragment shader
			fixed4 frag ( v2f IN ) : SV_Target
			{
				fixed4 color = _Color;
				// Read whether this is lit/shadowed from the shadow map
				float shadow = SampleShadowTexture( _ShadowMap, Polar( IN.world_position.xy, _LightPosition.xy ), _ShadowMapY );
				if ( shadow < 0.5 ) // shadowed pixel
				{
					color = fixed4( 0.0, 0.0, 0.0, 0.0 );
				}

				// fall off over distance
				float falloff = 1.0 - clamp( max( 0.0f, length( IN.world_position.xy - _LightPosition.xy ) - 32.0 ) / _Range, 0.0, 1.0 );
				color = color * falloff;

				// blend with existing light / shadow
				fixed4 blend = tex2D( _BlendTarget, IN.uv.xy );
				float weight = step( blend.r, color.r ); //max: color if color is >= blend
				color = color * weight + blend * ( 1.0 - weight ); // color xor blend

				return color;
			}
			ENDCG
		}
	}
}
