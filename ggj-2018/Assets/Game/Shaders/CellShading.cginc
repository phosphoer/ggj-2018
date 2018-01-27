#include "UnityLightingCommon.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

sampler2D _LightRamp; 
float4 _TimeOfDayTint; 
float4 _SkyLightColor; 
float3 _SkyLightDir; 

fixed NDotL(float3 worldNormal, float3 lightDir)
{
  // Calculate lighting 
  fixed nl = max(0, dot(normalize(worldNormal), -lightDir));
  return nl;
}

fixed4 CalculateLighting(float3 worldNormal, float shadowAtten, float extraLight = 0)
{
  // Lighting
  fixed shadowAmount = saturate(abs(_SkyLightDir.y));
  fixed shadow = lerp(1.0, shadowAtten, shadowAmount);
  fixed nl = NDotL(worldNormal, _SkyLightDir) * shadow;
  fixed3 nlRamp = tex2D(_LightRamp, float2(0.5, nl + extraLight)).rgb;

  fixed3 lighting = _SkyLightColor * nlRamp;
  lighting += _TimeOfDayTint * 0.1 + ShadeSH9(half4(normalize(worldNormal), 1)) * 0.5;

  // Get base time tinted color
  fixed4 diffuse = _TimeOfDayTint;
  diffuse.rgb *= lighting;
  diffuse.a = 1;

  return saturate(diffuse);
}