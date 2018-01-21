using UnityEngine;

public class FullscreenShader : MonoBehaviour
{
  public float EffectAlpha;

  public Material Material
  {
    get
    {
      if (_materialInstance == null)
      {
        _materialInstance = Instantiate(_material);
      }

      return _materialInstance;
    }
  }

  [SerializeField]
  private Material _material;

  private Material _materialInstance;

  protected virtual void OnDestroy()
  {
    if (_materialInstance != null)
    {
      Destroy(_materialInstance);
    }
  }

  protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
  {
    Material.SetFloat("_EffectAlpha", EffectAlpha);
    Graphics.Blit(source, destination, Material);
  }
}