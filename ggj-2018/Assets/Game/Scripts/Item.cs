using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour
{
  public enum TransmitTypeEnum
  {
    Face,
    Shape
  }

  public Rigidbody Rigidbody { get { return _rigidBody; } }
  public Collider Collider { get { return _collider; } }
  public Interactable Interactable { get { return _interactable; } }

  public bool IsBeingHeld
  {
    get { return _isBeingHeld; }
    set
    {
      _isBeingHeld = value;
      _rigidBody.isKinematic = _isBeingHeld;
      _collider.enabled = !_isBeingHeld;

      if (_isBeingHeld)
      {
        _interactable.enabled = false;
      }
      else if (_reEnableRoutine == null)
      {
        StartCoroutine(ReEnableRoutine());
      }
    }
  }

  [SerializeField]
  private Collider _collider = null;

  [SerializeField]
  private Rigidbody _rigidBody = null;

  [SerializeField]
  private Interactable _interactable = null;

  [SerializeField]
  private GameObject[] _faces = null;

  [SerializeField]
  private GameObject[] _shapes = null;

  [SerializeField]
  private Color[] _colors = null;

  private bool _isBeingHeld;
  private Coroutine _reEnableRoutine;
  private int _currentFaceIndex;
  private int _currentShapeIndex;

  public void TransmuteFace()
  {
    _currentFaceIndex = (_currentFaceIndex + 1) % _faces.Length;
    UpdateVisual();
  }

  public void TransmuteShape()
  {
    _currentShapeIndex = (_currentShapeIndex + 1) % _shapes.Length;
    UpdateVisual();
  }

  private void Start()
  {
    _currentFaceIndex = Random.Range(0, _faces.Length);
    _currentShapeIndex = Random.Range(0, _shapes.Length);
    UpdateVisual();
  }

  private void UpdateVisual()
  {
    foreach (GameObject faceObj in _faces)
    {
      faceObj.SetActive(false);
    }

    foreach (GameObject shapeObj in _shapes)
    {
      shapeObj.SetActive(false);
    }

    _faces[_currentFaceIndex].SetActive(true);
    _shapes[_currentShapeIndex].SetActive(true);

    Renderer[] renderers = _shapes[_currentShapeIndex].GetComponentsInChildren<Renderer>();
    foreach (Renderer r in renderers)
    {
      r.material.color = _colors[_currentFaceIndex];
    }
  }

  private IEnumerator ReEnableRoutine()
  {
    yield return new WaitForSeconds(1.0f);
    _interactable.enabled = true;
    _reEnableRoutine = null;
  }
}