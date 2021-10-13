Shader "Hidden/ColorMatrix"
{
	Properties
	{
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		[PerRendererData] _Color ("Tint", Color) = (1,1,1,1)
	}

	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Tags { "Queue" = "Transparent" }

		Blend SrcAlpha OneMinusSrcAlpha, One One
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile

			#include "UnityCG.cginc"

			struct a2v
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;

			fixed4 _Color;
			fixed4 _ColorOffset;
			fixed4x4 _ColorMatrix;

			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color * _Color;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 color = tex2D(_MainTex, i.uv);
				
				#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
					if (_AlphaSplitEnabled)
						color.a = tex2D (_AlphaTex, uv).r;
				#endif

				fixed4 multiply = saturate(mul(_ColorMatrix, color * i.color));
				fixed4 output = saturate(_ColorOffset + multiply);

				return output;
			}
			ENDCG
		}
	}
}