using UnityEngine;

public class CameraRig : MonoBehaviour
{
  public Camera Camera { get { return _camera; } }

  [SerializeField]
  private Camera _camera = null;
}