// Shadow map second pass shader
// Takes a 540 degree shadow map and consolidates it down to a 360 degree shadow map.
//
// Based on Rob Ware's 1D shadowmapping blogpost
Shader "Hidden/ShadowMapConsolidator"
{
	Properties
	{
		[PerRendererData] _ShadowMap ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off 
		Lighting Off
		ZWrite Off 
		Blend One Zero

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

			// Properties
			sampler2D _ShadowMap;

			// Data we pass to the vertex shader.
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0; //x & y to write to in the consolidated shadowmap ( in uv space (0,1) )
			};

			// Vertex shader. Just pass out what was passed in.
			appdata vert (appdata IN)
			{
				return IN;
			}

			// Fragment shader.
			// Reads data from the shadow map, and blends the (0 - 180) degree range with the (360 - 540) degree range, so the (360 - 540) degree range can be dropped.
			float4 frag (appdata IN) : SV_Target
			{
				// Get the x/y coordinates to read from the non-consolidated shadowmap
				// uv coordinates have a range of (0,1).
				// To get the x coordinate of the pixel in the non-consolidated shadowmap that corresponds to the pixel in the new shadowmap we are making:
				// 540 -> 360 degrees, multiply by 2/3.
				float u = IN.uv.x * 2.0f / 3.0f;
				float v = IN.uv.y; // use the same Y coordinate: so it corresponds to the same light source in both shadow maps.
				// Read the distance value to the closest geometry from the non-consolidated shadow map.
				float distance = tex2D( _ShadowMap, float2(u, v) ).r;

				// For angles (0 - 180) degrees, we need to double sample from the (360 - 540) range, and use the smallest value.
				// THIS is the whole reason we run the shadowmap through this consolidator, to remove double sampling by baking the result into (0 - 180).
				if ( u < 1.0f / 3.0f )
				{
					distance = min( distance, tex2D( _ShadowMap, float2( u + 2.0f / 3.0f, v ) ).r );
				}

				// return the minimum distance from the light source to light-occluding geometry, as a color, since we're saving to an image.
				return float4( distance, 0, 0, 1 );
			}
			ENDCG
		}
	}
}
