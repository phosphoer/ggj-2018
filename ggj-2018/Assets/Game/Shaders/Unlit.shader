Shader "Custom/Unlit"
{
	Properties
	{
    _Color ("Color", Color) = (1,1,1,1)
    _MainTex ("Texture", 2D) = "white" {}
		_TimeOfDayMod ("Time of Day Modifier", float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
        float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

      sampler2D _MainTex;
      float4 _Color;
			float4 _TintColor;
			float4 _TimeOfDayTint;
			float _TimeOfDayMod;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
				o.color = v.color;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float4 timeTint = float4(1, 1, 1, 1) * (1 - _TimeOfDayMod) + _TimeOfDayTint * _TimeOfDayMod;
        return _Color * timeTint * tex2D(_MainTex, i.uv) * i.color;
			}
			ENDCG
		}
	}

  FallBack "VertexLit"
}
