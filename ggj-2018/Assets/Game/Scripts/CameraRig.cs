using UnityEngine;

public class CameraRig : MonoBehaviour
{
  public Camera Camera { get { return _camera; } }
  public Transform TrackedTransform { get; set; }
  public bool IsZoomedOut { get; set; }

  [SerializeField]
  private Camera _camera = null;

  [SerializeField]
  private Vector3 _cameraOffset = new Vector3(0, 4, -10);

  [SerializeField]
  private float _panOutScale = 1.25f;

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
      Vector3 desiredPos = TrackedTransform.position + _cameraOffset * (IsZoomedOut ? _panOutScale : 1.0f);
      if (Vector3.Distance(transform.position, desiredPos) < 0.01f)
      {
        _interpolating = false;
      }
      if (IsZoomedOut)
      {
        _interpolating = true;
      }

      if (_interpolating)
      {
        transform.position = Mathfx.Damp(transform.position, desiredPos, 0.5f, Time.deltaTime * 5.0f);
      }
      else
      {
        transform.position = desiredPos;
      }
    }

    _shakeTimer -= Time.deltaTime;
    if (_shakeTimer > 0)
    {
      float shakeT = Mathf.Clamp01(_shakeTimer / _shakeTime);
      transform.position += Random.onUnitSphere * Random.value * _shakeMagnitude * shakeT;
    }

    Quaternion desiredRotation = Quaternion.LookRotation((TrackedTransform.position - _camera.transform.position).normalized);
    transform.rotation = Mathfx.Damp(transform.rotation, desiredRotation, 0.5f, Time.deltaTime * 5.0f);
  }
}