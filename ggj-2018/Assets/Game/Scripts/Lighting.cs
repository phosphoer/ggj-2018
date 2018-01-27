using UnityEngine;

[ExecuteInEditMode]
public class Lighting : MonoBehaviour
{
  [SerializeField]
  private Light _sunLight = null;

  [SerializeField]
  private Color _lightingTint = Color.white;

  private void Update()
  {
    if (_sunLight != null)
    {
      Shader.SetGlobalVector("_SkyLightDir", _sunLight.transform.forward);
      Shader.SetGlobalColor("_SkyLightColor", _sunLight.color);
      Shader.SetGlobalColor("_TimeOfDayTint", _lightingTint);
    }
  }
}