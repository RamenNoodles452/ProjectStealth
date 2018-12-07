Shader "Sprites/Blackout"
{
	Properties
	{
	}
	SubShader
	{
		// No culling or depth
		Cull Off 
		//Lighting Off
		ZWrite Off
		ZTest Always
		Blend One OneMinusSrcAlpha

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
				Ref 1
				Comp NotEqual
				Pass Keep
				Fail Zero
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

			fixed4 frag (v2f i) : SV_Target
			{
				return fixed4( 0, 0, 0, 0.5 );
			}
			ENDCG
		}
	}
}
