// Shadow map first pass shader
// Creates a shadow map, a 2D array of values, or in this case, a 2D black and white (or red and black) "image".
// Each row in the shadowmap represents a 360 degree view of the level from a light source's perspective.
// Each row is 540 (* 3) pixels wide, representing 360 degrees, plus an additional 180 degrees that need to be aliased with the first 180 later on.
//   (This is handled by another shader).
// Each row corresponds to one light source. Multiple light sources = multiple rows.
// Each pixel in a row indicates the MINIMUM DISTANCE before a ray of light shot from the light source at the angle hits some solid geometry.
// The following shader code executes for each light.
//
// Based on Rob Ware's 1D shadowmapping blog post

Shader "Hidden/ShadowMap"
{
	Properties
	{
		//_MainTex ("Texture", 2D) = "white" {}
		[PerRendererData] _LightPosition( "LightPosition", Vector ) = ( 0, 0, 0, 0 )
		[PerRendererData] _ShadowMapY( "ShadowMapY", Float ) = -1
	}
	SubShader
	{
		// No culling or depth
		Cull Off 
		ZWrite Off 
		ZTest Always
		Blend One One
		BlendOp Min

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			// Properties
			float4 _LightPosition; // x and y are the 2D position, the rest is unused.
			float  _ShadowMapY;    // y coordinate to write to in the shadow map image, in clip space (-1, 1).

			// Due to rigid shader restrictions, some of the data passing is heavily codified.
			// We feed all "solid", shadow-casting geometry to this shader, one edge at a time.
			struct appdata
			{
				// the 2 2D vertices that make up the edges
				float3 vertex1 : POSITION;  // Vertex 1 = xy
				float2 vertex2 : TEXCOORD0; // Vertex 2 = xy. Nonsense semantic.
			};

			// We convert the edge data to 
			struct v2f
			{
				// stores the X,Y in the shadowmap image we are writing to.
				float4 vertex : SV_POSITION;
				// stores the 2 2D vertices that make up the edge
				float4 edge   : TEXCOORD0;   // Vertex 1 = xy, vertex 2 = zw. Nonsense semantic.
				// stores the angle and distance from the light source to the edge
				float2 polar  : TEXCOORD1;   // x = angle, y = distance. Nonsense semantic.
			};

			// Utility functions
			// converts 2 points into the angle and distance from the centerpoint to the other point.
			float2 Polar( float2 xy, float2 center_point )
			{
				float2 delta = xy - center_point;
				float distance = length( delta );
				float angle = atan2( delta.y, delta.x );
				return float2( angle, distance );
			}
			
			// given two line segments, a and b, find their intersection.
			float IntersectT( float2 start_a, float2 end_a, float2 start_b, float2 end_b )
			{
				float2 perpindicular_to_b = float2( end_b.y - start_b.y, end_b.x - start_b.x );
				// dot product, or projection returns: how "similar' a is to the segment perpindicular to b. 1 = colinear, 0 = perpindicular
				float similarity = dot( end_a - start_a, perpindicular_to_b );
				if ( abs( similarity ) < 1e-4 ) { return 0.0; } // no intersect
				// get the similarity of the vector connecting the start points of a and b to the perpindicular
				float similarity2 = dot( start_b - start_a, perpindicular_to_b );
				// find the value of the number of lengths of a down a the intersect is.
				float t = similarity2 / similarity;
				// intersect pt = (t * (end_a.x - start_a.x) + start_a.x, t * (end_a.y - start_a.y) + start_a.y ) 
				return t;
			}

			// convert (-PI, 2PI) -> (-1,1)
			// it's a 3PI range to deal with angle wraparound.
			// for values in 0-180 range, also check 360-540.
			float PolarAngleToClipSpace( float angle )
			{
				return ( ( angle + UNITY_PI ) * 2.0f / ( UNITY_PI * 3.0f ) ) - 1.0f;
			}

			// Vertex shader.
			// takes in appdata, and processes it into v2f for the fragment shader to manipulate.
			v2f vert (appdata v)
			{
				v2f OUT;

				// get the angle and distance from the light source to the edge endpoints
				float polar1 = Polar( v.vertex1.xy, _LightPosition.xy );
				float polar2 = Polar( v.vertex2.xy, _LightPosition.xy );

				// store the 2 vertices that make up the edge
				OUT.edge = float4( v.vertex1.x, v.vertex1.y, v.vertex2.x, v.vertex2.y );
				OUT.edge = lerp( OUT.edge, OUT.edge.zwxy, step( polar1, polar2 ) ); // reverse the order of vertices if the angles are reversed.

				float difference = abs( polar1.x - polar2.x ); // angle difference between endpoints

				// keep it in range?
				if ( difference >= UNITY_PI )
				{
					float maximum = max( polar1.x, polar2.x ); // larger angle
					if ( polar1.x == maximum )
					{
						polar1.x = maximum + 2* UNITY_PI - difference;
					}
					else
					{
						polar1.x = maximum;
					}
				}

				// store the shadowmap image coordinates to write to.
				OUT.vertex = float4( PolarAngleToClipSpace( polar1.x ), _ShadowMapY, 0.0f, 1.0f );
				// store the angle and distance.
				OUT.polar = polar1;
				// Pass this to "frag"
				return OUT;
			}

			// Fragment shader
			// higher precision for higher distance fidelity.
			float4 frag (v2f i) : SV_Target
			{
				float angle = i.polar.x;
				// create a "ray" of light
				float2 end = _LightPosition.xy + float2( cos( angle ), sin( angle ) ) * 512.0; // 512 is max distance

				// Find how far away the light ray @ angle intersects the edge.
				float t = IntersectT( _LightPosition.xy, end, i.edge.xy, i.edge.zw );

				return float4( t, 0, 0, 1 );
			}
			ENDCG
		}
	}
}
