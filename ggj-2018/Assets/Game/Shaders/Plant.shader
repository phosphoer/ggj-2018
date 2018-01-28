Shader "Custom/Plant"
{
	Properties
	{
    _Color ("Color", Color) = (1,1,1,1)
    _MainTex ("Texture", 2D) = "white" {}
		_LightRamp ("Light Ramp", 2D) = "white" {}
    _WindDirection ("Wind Scale", Vector) = (1, 0, 0, 0)
    _WindScale ("Wind Scale", float) = 0.25
    _WindFrequency ("Wind Frequency", float) = 5.0
    _SwayScale ("Sway Scale", float) = 0.25
    _WobbleFrequency ("Wobble Frequency", float) = 2.0
	}
	SubShader
	{
    CGINCLUDE 
			#include "UnityCG.cginc"
      #include "CellShading.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
        float3 normal : NORMAL;
				fixed4 color : COLOR;
        float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
        float2 uv : TEXCOORD0;
        SHADOW_COORDS(1)
        float3 worldNormal : TEXCOORD2;
        UNITY_FOG_COORDS(3)
			};

      sampler2D _MainTex;
      float4 _Color;
			float4 _TintColor;
      float4 _WindDirection;
      float _WindScale;
      float _WindFrequency;
      float _SwayScale;
      float _WobbleFrequency;

			v2f vert (appdata v)
			{
				v2f o;

        float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
        float windVariance = (sin(_Time.y * _WindFrequency + v.vertex.x * _WobbleFrequency) * 0.5 + 1);
        worldPos += _WindDirection * ((_WindScale + _WindScale * windVariance * _SwayScale) * v.color.r * v.color.r);
        float4 objPos = mul(unity_WorldToObject, float4(worldPos, 1));

				o.pos = UnityObjectToClipPos(objPos);
        o.uv = v.uv;
        o.worldNormal = mul(unity_ObjectToWorld, v.normal);
				o.color = v.color;

        TRANSFER_SHADOW(o)
        UNITY_TRANSFER_FOG(o, o.pos);

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
        // Get base diffuse color
        fixed4 diffuse = _Color * tex2D(_MainTex, i.uv);
        diffuse *= CalculateLighting(normalize(i.worldNormal), SHADOW_ATTENUATION(i));
        UNITY_APPLY_FOG(i.fogCoord, diffuse);

        return diffuse;
			}
    ENDCG

    Pass 
    {
      Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
      Fog { Mode Off }
      ZWrite On 
			ZTest Less 
			Cull Back
      
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment shadowCastFrag

      float4 shadowCastFrag(v2f i) : COLOR
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
    }

		Pass
		{
		  Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
      #pragma multi_compile_fog
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			ENDCG
		}
	}

  FallBack "Diffuse"
}
