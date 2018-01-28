#include "UnityLightingCommon.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

sampler2D _LightRamp; 
float4 _TimeOfDayTint; 
float4 _SkyLightColor0; 
float4 _SkyLightColor1; 
float3 _SkyLightDir0; 
float3 _SkyLightDir1; 

fixed NDotL(float3 worldNormal, float3 lightDir)
{
  // Calculate lighting 
  fixed nl = max(0, dot(normalize(worldNormal), -lightDir));
  return nl;
}

fixed4 CalculateLighting(float3 worldNormal, float shadowAtten, float extraLight = 0)
{
  // Lighting
  fixed shadowAmount = saturate((abs(_SkyLightDir0.y) + abs(_SkyLightDir1.y)));
  fixed shadow = lerp(1.0, shadowAtten, shadowAmount);
  fixed nl0 = NDotL(worldNormal, _SkyLightDir0) * shadow;
  fixed nl1 = NDotL(worldNormal, _SkyLightDir1) * shadow;
  fixed3 nlRamp0 = tex2D(_LightRamp, float2(0.5, nl0 + extraLight)).rgb;
  fixed3 nlRamp1 = tex2D(_LightRamp, float2(0.5, nl1 + extraLight)).rgb;

  fixed3 lighting = _SkyLightColor0 * nlRamp0 + _SkyLightColor1 * nlRamp1;
  lighting += _TimeOfDayTint * 0.1 + ShadeSH9(half4(normalize(worldNormal), 1)) * 0.5;

  // Get base time tinted color
  fixed4 diffuse = _TimeOfDayTint;
  diffuse.rgb *= lighting;
  diffuse.a = 1;

  return saturate(diffuse);
}