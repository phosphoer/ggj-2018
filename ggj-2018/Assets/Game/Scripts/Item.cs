using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour
{
  public static int InstanceCount { get; private set; }

  public event System.Action IsHeldChanged;

  public enum TransmitTypeEnum
  {
    Face,
    Shape
  }

  public struct ItemDefinition
  {
    public int FaceIndex;
    public int ShapeIndex;
  }

  public Rigidbody Rigidbody { get { return _rigidBody; } }
  public Collider Collider { get { return _collider; } }
  public Interactable Interactable { get { return _interactable; } }
  public int TypeCount { get { return _faces.Length; } }
  public ItemDefinition Definition
  {
    get { return new ItemDefinition() { FaceIndex = _currentFaceIndex, ShapeIndex = _currentShapeIndex }; }
    set
    {
      _currentFaceIndex = value.FaceIndex;
      _currentShapeIndex = value.ShapeIndex;
      UpdateVisual();
    }
  }

  public PlayerController OwnedByPlayer { get; set; }
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

      if (IsHeldChanged != null)
      {
        IsHeldChanged();
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

  [SerializeField]
  private SoundBank _creatureSound = null;

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

  public void Randomize()
  {
    _currentFaceIndex = Random.Range(0, _faces.Length);
    _currentShapeIndex = Random.Range(0, _shapes.Length);
    UpdateVisual();
  }

  private void Start()
  {
    ++InstanceCount;
    _interactable.PromptShown += OnPromptShown;
  }

  private void OnDestroy()
  {
    --InstanceCount;
  }

  private void OnPromptShown()
  {
    AudioManager.Instance.PlaySound(_creatureSound);
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