// Like unity default sprite shader with normal map support
Shader "Sprites/NormalSprite"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_NormalTex ("Normal Map", 2D) = "bump" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_DiffuseIntensity ("Diffuse Intensity", Float) = 1
		_SpecularColor ("Specular Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Shininess("Shininess", Float) = 5
		_Specularity("Specularity", Float ) = 1
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		Cull Off
		Lighting On
		Fog { Mode Off }
		ZWrite Off
		//ZTest LEqual

		// Doing a ZWrite On pass doesn't solve our draw order problems, use sorting layers.

		//---------------------------------
		// Base Pass
		//---------------------------------
		Pass
		{
			Tags 
			{ 
				"LightMode" = "ForwardBase"
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
			}
			Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			fixed4 _Color;
			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;

			struct vertexInputFirst
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct fragmentInputFirst
			{
				float4 position : POSITION;
				fixed4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			fragmentInputFirst vert( vertexInputFirst IN )
			{
				fragmentInputFirst OUT;

				OUT.position = UnityObjectToClipPos( IN.vertex );
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;

				//#ifdef PIXELSNAP_ON
				//OUT.vertex = UnityPixelSnap (OUT.vertex);
				//#endif

				return OUT;
			}

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

				#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
				{
					color.a = tex2D (_AlphaTex, uv).r;
				}
				#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

				return color;
			}

			fixed4 frag( fragmentInputFirst IN ) : SV_TARGET
			{
				fixed4 c = SampleSpriteTexture( IN.texcoord ) /** IN.color*/ * _Color;
				fixed3 ambientLighting = UNITY_LIGHTMODEL_AMBIENT.rbg * c.rgb;
				c = fixed4( ambientLighting.rgb, c.a );
				return c;
			}
		ENDCG
		}

		//--------------------------------------
		// Lighting Pass
		//--------------------------------------
		// Needs to be separate b/c it runs for each light.
		Pass
		{
			Tags
			{
				"LightMode" = "ForwardAdd"
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
			}
			Blend One One // Additive

		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;    // source diffuse
			uniform sampler2D _NormalTex;  // normal map
			uniform float4 _LightColor0;   // color of light source
			uniform float4 _SpecularColor;
			uniform float _Shininess;
			uniform float _Specularity; // scaling factor for specular lighting
			uniform float _DiffuseIntensity; // scaling factor for diffuse lighting

			struct vertexInput
			{
				float4 vertex: POSITION;
				float4 color: COLOR;
				float4 uv: TEXCOORD0;
			};

			struct fragmentInput
			{
				float4 position: SV_POSITION;
				float4 color: COLOR0;
				float2 uv: TEXCOORD0;
				float4 worldPosition: TEXCOORD1; // distance to light
			};

			fragmentInput vert( vertexInput IN )
			{
				fragmentInput OUT;
				OUT.position = UnityObjectToClipPos( IN.vertex );
				OUT.worldPosition = mul( unity_ObjectToWorld, IN.vertex );
				OUT.uv = float2( IN.uv.x, IN.uv.y );
				OUT.color = IN.color;
				return OUT;
			}

			fixed4 frag( fragmentInput IN ) : COLOR
			{
				fixed4 diffuseColor = tex2D( _MainTex, IN.uv );
				if ( diffuseColor.a == 0.0 ) { return fixed4( 0.0, 0.0, 0.0, 0.0 ); }

				// get value from normal map, convert [0,1] RGB range to [-1,1] normal range
				float3 normalDirection = (tex2D( _NormalTex, IN.uv ).rgb - 0.5f) * 2.0f;

				// multiply by world to object matrix
				normalDirection = mul( float4( normalDirection.x, normalDirection.y, normalDirection.z, 0.5f), unity_WorldToObject ).xyz;

				// negate Z
				normalDirection.z *= -1;

				normalDirection = normalize( normalDirection );

				// Calculate attenuation
				// BUG: seems like it uses vert world position, not this pixel/fragment's position, so it lacks some detail and smoothness.
				float attenuation;
				float3 lightDirection;
				if ( _WorldSpaceLightPos0.w == 0.0 ) // directional light
				{
					attenuation = 1.0; // none
					lightDirection = normalize( _WorldSpaceLightPos0.xyz );
				}
				else
				{
					// distance to point or spot light
					float3 vertexToLightSource = _WorldSpaceLightPos0.xyz - IN.worldPosition; // this only has vertex level fidelity.
					float3 distance = length( vertexToLightSource );
					attenuation = 1.0 / distance * 32.0; // linear attenuation
					lightDirection = normalize( vertexToLightSource );
					// BUG: Spotlight range and angle aren't respected
					//#if defined (SPOT)
					//	float4 positionLight = mul( unity_WorldToLight, IN.worldPosition );
					//	float3 textureCoordinates = float3( float2( positionLight.x, positionLight.y ) + float2( 0.5 * positionLight.w ), positionLight.w );
					//	attentuation = tex2Dproj( _LightTexture0, textureCoordinates ).a;
					//#endif
				}

				// calculate diffuse lighting
				float normalDotLight = dot( normalDirection, lightDirection );
				float diffuseLevel = attenuation * _DiffuseIntensity * max( 0.0, normalDotLight );

				// calculate specular lighting
				float specularLevel = 0.0;
				// make sure the light is on the proper side for specular highlighting. Otherwise, no specular.
				if ( normalDotLight > 0.0 )
				{
					//float3 viewDirection = normalize( _WorldSpaceCameraPos - IN.worldPosition.xyz );
					float3 viewDirection = float3( 0.0, 0.0, -1.0 ); // orthogonal
					specularLevel = attenuation * pow( max( 0.0, dot( reflect( -lightDirection, normalDirection ), viewDirection ) ), _Shininess );
				}

				float3 diffuseReflection = diffuseColor.rgb * diffuseLevel * IN.color * _LightColor0.rgb;
				float3 specularReflection = _Specularity * _SpecularColor.rgb * specularLevel * IN.color * _LightColor0.rgb;
				// use diffuse alpha. multiply to solve transparency issues?
				return fixed4( diffuseColor.a * ( diffuseReflection + specularReflection ), diffuseColor.a );
			}
		ENDCG
		}
	}
	Fallback "Sprites/Default"
}