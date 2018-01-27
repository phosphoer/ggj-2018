using UnityEngine;

public class Character : MonoBehaviour
{
  public Vector3 MoveDirection
  {
    get { return _moveDirection; }
    set { _moveDirection = value.normalized; }
  }

  public Item HeldItem
  {
    get { return _heldItem; }
    set { _heldItem = value; }
  }

  [SerializeField]
  private float _moveSpeed = 1.0f;

  [SerializeField]
  private Rigidbody _rigidBody = null;

  [SerializeField]
  private Transform _heldItemAnchor = null;

  [SerializeField]
  private Vector3 _throwForceLocal = new Vector3(0, 5, 5);

  private Vector3 _moveDirection;
  private Vector3 _lastMoveDirection;
  private Item _heldItem;
  private Transform _heldItemOriginalParent;

  public void HoldItem(Item item)
  {
    _heldItemOriginalParent = item.transform.parent;

    _heldItem = item;
    _heldItem.transform.SetParent(_heldItemAnchor);
    _heldItem.transform.localPosition = Vector3.zero;
    _heldItem.transform.localRotation = Quaternion.identity;
    _heldItem.IsBeingHeld = true;
  }

  public void DropItem()
  {
    if (_heldItem != null)
    {
      _heldItem.transform.SetParent(_heldItemOriginalParent);
      _heldItem.IsBeingHeld = false;

      Vector3 throwForce = transform.TransformDirection(_throwForceLocal);
      _heldItem.Rigidbody.AddForce(throwForce, ForceMode.Impulse);
      _heldItem = null;
    }
    else
    {
      Debug.LogError("Tried to drop an item but not holding anything");
    }
  }

  public void TransmitItem(Character targetCharacter)
  {
    if (_heldItem != null)
    {
      _heldItem.transform.SetParent(_heldItemOriginalParent);
      _heldItem.IsBeingHeld = false;
      targetCharacter.ReceiveItem(_heldItem);
      _heldItem = null;
    }
    else
    {
      Debug.LogError("Tried to transmit an item but not holding anything");
    }
  }

  public void ReceiveItem(Item item)
  {
    item.transform.SetPositionAndRotation(_heldItemAnchor.position, _heldItemAnchor.rotation);

    Vector3 throwForce = transform.TransformDirection(_throwForceLocal) * 1.5f;
    item.Rigidbody.AddForce(throwForce, ForceMode.Impulse);
  }

  private void Update()
  {
    Vector3 moveVector = MoveDirection * _moveSpeed * Time.deltaTime;
    moveVector.y = 0;

    if (moveVector.sqrMagnitude > 0)
    {
      _lastMoveDirection = moveVector;
    }

    _rigidBody.MovePosition(_rigidBody.position + moveVector);

    if (_lastMoveDirection.sqrMagnitude > 0)
    {
      Quaternion facingRotation = Quaternion.LookRotation(_lastMoveDirection);
      _rigidBody.rotation = Mathfx.Damp(transform.rotation, facingRotation, 0.5f, Time.deltaTime * 5.0f);
    }
  }
}