Shader "Sprites/Blackout"
{
	Properties
	{
		_Color( "Blackout Color", Color ) = (0, 0, 0, 0.5)
	}
	SubShader
	{
		// No culling or depth
		Cull Off 
		//Lighting Off
		ZWrite Off
		ZTest Always
		//Blend One OneMinusSrcAlpha
		Blend SrcAlpha OneMinusSrcAlpha

		Tags
		{
			"Queue" = "Transparent" // must be higher than blackoutmask's value
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Pass
		{
			Stencil
			{
				// Uses bit 5 to prevent clashing with unity builtin masks (which use increment, so bits 1-?), and Unity rendering (use bits 6-8).
				WriteMask 16 // ONLY change bit 5
				ReadMask 16  // ONLY read from bit 5 (value is 16 or 0)
				Ref 0
				// We're only looking at the value of bit 5. We care if it's 1 or 0 (= Ref 16 or = Ref 0).
				Comp Equal
				// Outside the mesh area, bit 5 is 0, just preserve current state of bit 5 (and the stencil buffer in general). Could set to 0, but this is more performant.
				Pass Keep
				// Within the mesh area, flip bit 5 from 1 to 0, only if it is 1. 
				// This "restores" the value in the stencil buffer, preserving any changes to lower or higher bits while undoing the blackout mask stencil write.
				// Unity sprite masks read the value of all 8 bits as a single (0-255) and do >/<= checks on its value, so setting bit 5 and keeping the change around broke them.
				Fail Invert
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			
			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 _Color;

			fixed4 frag (v2f i) : SV_Target
			{
				//return fixed4( 0, 0, 0, 0.5 );
				return _Color;
			}
			ENDCG
		}
	}
}
