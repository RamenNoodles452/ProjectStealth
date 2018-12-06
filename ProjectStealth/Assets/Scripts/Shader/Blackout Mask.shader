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
			"Queue" = "Transparent-1"      //must be lower than blackout's value
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		Pass
		{
			// Masks
			Stencil
			{
				Ref 1
				Comp Always
				Pass Replace
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
