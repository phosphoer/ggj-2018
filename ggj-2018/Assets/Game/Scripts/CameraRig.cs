using UnityEngine;

public class CameraRig : MonoBehaviour
{
  public Camera Camera { get { return _camera; } }
  public Transform TrackedTransform { get; set; }
  public bool IsZoomedOut { get; set; }
  public bool IsZoomedIn { get; set; }

  [SerializeField]
  private Camera _camera = null;

  [SerializeField]
  private Vector3 _cameraOffset = new Vector3(0, 4, -10);

  [SerializeField]
  private float _panOutScale = 1.25f;

  [SerializeField]
  private float _zoomInScale = 0.5f;

  private float _shakeTimer;
  private float _shakeTime;
  private float _shakeMagnitude;
  private bool _interpolating = true;

  public void Shake(float duration, float magnitude)
  {
    _shakeTime = duration;
    _shakeTimer = duration;
    _shakeMagnitude = magnitude;
  }

  private void Update()
  {
    if (TrackedTransform != null)
    {
      float zoomScale = 1.0f;
      zoomScale *= IsZoomedIn ? _zoomInScale : 1.0f;
      zoomScale *= IsZoomedOut ? _panOutScale : 1.0f;

      Vector3 desiredPos = TrackedTransform.position + _cameraOffset * zoomScale;
      if (Vector3.Distance(transform.position, desiredPos) < 0.01f)
      {
        _interpolating = false;
      }
      if (IsZoomedOut || IsZoomedIn)
      {
        _interpolating = true;
      }

      if (_interpolating)
      {
        transform.position = Mathfx.Damp(transform.position, desiredPos, 0.5f, Time.deltaTime * 1.0f);
      }
      else
      {
        transform.position = desiredPos;
      }

      Quaternion desiredRotation = Quaternion.LookRotation((TrackedTransform.position - _camera.transform.position).normalized);
      transform.rotation = Mathfx.Damp(transform.rotation, desiredRotation, 0.5f, Time.deltaTime * 5.0f);
    }

    _shakeTimer -= Time.deltaTime;
    if (_shakeTimer > 0)
    {
      float shakeT = Mathf.Clamp01(_shakeTimer / _shakeTime);
      transform.position += Random.onUnitSphere * Random.value * _shakeMagnitude * shakeT;
    }
  }
}