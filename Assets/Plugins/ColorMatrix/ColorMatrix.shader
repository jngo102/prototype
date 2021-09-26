Shader "Hidden/ColorMatrix"
{
	Properties
	{
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		[PerRendererData] _Color ("Tint", Color) = (1,1,1,1)
		[PerRendererData] _AlphaTex ("Alpha Texture", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

		_EnableETC1 ("Enable ETC 1", float) = 0
	}

	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Tags { "Queue" = "Transparent" }

		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA

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

			// Input
			sampler2D _MainTex;

			fixed4 _Color;
			float4 _ColorOffset;
			float4x4 _ColorMatrix;

			float _EnableETC1;

			sampler2D _AlphaTex;
			float _EnableExternalAlpha;

			// Temporary (for TRANSFORM_TEX)
			float4 _MainTex_ST;

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
				
				// TODO : Способ не работает на Android с ETC1
				// ETC1_EXTERNAL_ALPHA

				if(_EnableETC1) {
					fixed4 alpha = tex2D(_AlphaTex, i.uv);
    				color.a = lerp(color.a, alpha.r, 1);
				}

				color = _ColorOffset + mul(_ColorMatrix, color);
				return color * i.color;
			}
			ENDCG
		}
	}
}
