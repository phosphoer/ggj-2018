using UnityEngine;

[ExecuteInEditMode]
public class Lighting : MonoBehaviour
{
  [SerializeField]
  private Light _sunLight = null;

  [SerializeField]
  private Light _secondaryLight = null;

  [SerializeField]
  private Color _lightingTint = Color.white;

  private void Update()
  {
    if (_sunLight != null)
    {
      Shader.SetGlobalVector("_SkyLightDir0", _sunLight.transform.forward);
      Shader.SetGlobalColor("_SkyLightColor0", _sunLight.color);
      Shader.SetGlobalVector("_SkyLightDir0", _secondaryLight.transform.forward);
      Shader.SetGlobalColor("_SkyLightColor0", _secondaryLight.color);
      Shader.SetGlobalColor("_TimeOfDayTint", _lightingTint);
    }
  }
}