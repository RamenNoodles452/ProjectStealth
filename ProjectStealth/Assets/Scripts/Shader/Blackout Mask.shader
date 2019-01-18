Shader "Sprites/Blackout Mask"
{
	Properties
	{
	}
	SubShader
	{
		// No culling or depth
		Cull Off 
		colormask 0 // don't write alpha or color to stencil/mask


		Tags
		{
			"Queue" = "Geometry-1"      //must be lower than blackout's value
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		Pass
		{
			// Masks
			Stencil
			{
				// Only use 5th stencil buffer bit to avoid clashing: unity built-in masks increment so they use bits 1-?. bits 6-8 (32,64,128) can be used by rendering.
				WriteMask 16 // ONLY write to bit 5 (16) ????1???
				Ref 16       // Write the value that will set bit 5 to 1.
				Comp Always  // 100% of the time, for every pixel in the shaded mesh's area.
				Pass Replace // Put the value from Ref into the stencil buffer (respecting the mask) for every pixel within this mesh's area.
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
			
			//sampler2D _MainTex;

			fixed4 frag (v2f i) : SV_Target
			{
				return fixed4( 0, 0, 0, 0 );
			}
			ENDCG
		}
	}
}
