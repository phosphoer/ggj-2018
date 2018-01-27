using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour
{
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

  private bool _isBeingHeld;
  private Coroutine _reEnableRoutine;

  private IEnumerator ReEnableRoutine()
  {
    yield return new WaitForSeconds(1.0f);
    _interactable.enabled = true;
    _reEnableRoutine = null;
  }
}