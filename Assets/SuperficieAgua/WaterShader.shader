// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Arj2D/WaterDeform" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		
		_CausticTex ("Caustic", 2D) = "white" {}

		_waterColour("Colour", Color) = (1,1,1,1)

		_waterPeriod ("Period", Range(0,50)) = 1 //Que tan intenso
		_waterMagnitude ("Magnitude", Range(0,0.05)) = 0.05 //Que tanto afecta el caustico
		_offset ("offset", Range(0,10)) = 1 //Que tanto afecta el noise

		_NoiseTex ("Noise text", 2D) = "white" {}
	}
	
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType" = "Plane" "ForceNoShadowCasting" = "True" }
		ZWrite Off Lighting Off Cull Off Fog { Mode Off } Blend One Zero

		LOD 150

		GrabPass { "_GrabTexture" }
		
		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _GrabTexture;

			sampler2D _MainTex;

			sampler2D _NoiseTex;
			sampler2D _CausticTex;
			
			fixed4 _waterColour;
			float  _waterPeriod;
			float  _waterMagnitude;
			float  _offset;

			float2 sinusoid (float2 x, float2 m, float2 M, float2 periodo)
			{
				float2 escursione   = M - m;
				float2 coefficiente = 6.283 / periodo;
				return escursione / 2.0 * (1.0 + sin(x * coefficiente)) + m;
			}

			struct vin_vct
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			struct v2f_vct
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;

				float4 position_in_world_space : TEXCOORD2;
				float4 grabUV : TEXCOORD3;
			};

			// Vertex function 
			v2f_vct vert (vin_vct v)
			{
				v2f_vct o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;

				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;

				o.position_in_world_space = mul(unity_ObjectToWorld, v.vertex);
				o.grabUV = ComputeGrabScreenPos(o.vertex);
				
				return o;
			}

			// Fragment function
			fixed4 frag (v2f_vct i) : COLOR
			{
				fixed4 noise = tex2D(_NoiseTex, i.texcoord);
				fixed4 mainColour = tex2D(_MainTex, i.texcoord1);
			
				float time = _Time[1];

				float2 waterDisplacement =
				sinusoid
				(
					float2 (time, time) + (noise.xy) * _offset,
					float2(-_waterMagnitude, -_waterMagnitude),
					float2(+_waterMagnitude, +_waterMagnitude),
					float2(_waterPeriod, _waterPeriod)
				);
				
				i.grabUV.xy += waterDisplacement;
				fixed4 col = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(i.grabUV));
				fixed4 causticColour = tex2D(_CausticTex, i.texcoord.xy*0.25 + waterDisplacement*5);
				return col * mainColour * _waterColour  * causticColour;
			}
		
			ENDCG
		} 
	}
}