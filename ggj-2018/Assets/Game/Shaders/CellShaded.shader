Shader "Custom/CellShaded"
{
	Properties
	{
    _Color ("Color", Color) = (1,1,1,1)
    _MainTex ("Texture", 2D) = "white" {}
		_LightRamp ("Light Ramp", 2D) = "white" {}
    _VertexColorWeight ("Vertex Color Weight", float) = 1
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
      float _VertexColorWeight;

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        o.worldNormal = mul(unity_ObjectToWorld, v.normal);
				o.color = v.color;

        TRANSFER_SHADOW(o)
        UNITY_TRANSFER_FOG(o, o.pos);

				return o;
			}
	  ENDCG 

		Pass
		{
		  Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }
			ZWrite On 

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
      #pragma multi_compile_fog			
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			fixed4 frag (v2f i) : SV_Target
			{
        // Get base diffuse color
        fixed3 diffuse = _Color.rgb * tex2D(_MainTex, i.uv).rgb * lerp(fixed3(1, 1, 1), i.color.rgb, _VertexColorWeight);
        diffuse *= CalculateLighting(normalize(i.worldNormal), SHADOW_ATTENUATION(i)).rgb;
        UNITY_APPLY_FOG(i.fogCoord, diffuse);

        return fixed4(diffuse, 1);
			}
			ENDCG
		}

	}

	Fallback "Diffuse"
}
