Shader "Custom/UnlitTransparent"
{
	Properties
	{
    _Color ("Color", Color) = (1,1,1,1)
    _MainTex ("Texture", 2D) = "white" {}
		_TimeOfDayMod ("Time of Day Modifier", float) = 1
		
		[Enum(Off,0,On,1)] 
		_ZWrite ("ZWrite", Float) = 1
		
		[Enum(Always, 0, Less, 2, Equal, 3, LEqual, 4, GEqual, 5)] 
		_ZTest ("ZTest", Float) = 4
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
    Blend SrcAlpha OneMinusSrcAlpha
		ZWrite [_ZWrite]
		ZTest [_ZTest]

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
        float3 worldPos : TEXCOORD1;
			};

      sampler2D _MainTex;
      float4 _MainTex_ST;
      float4 _Color;
			float4 _TintColor;
			float4 _TimeOfDayTint;
			float _TimeOfDayMod;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.color = v.color;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float3 timeTint = float3(1, 1, 1) * (1 - _TimeOfDayMod) + _TimeOfDayTint.rgb * _TimeOfDayMod;
        float4 color = _Color * tex2D(_MainTex, i.uv) * i.color;
        color.rgb *= timeTint;
        return color;
			}
			ENDCG
		}
	}
}
