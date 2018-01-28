using UnityEngine;

public class Character : MonoBehaviour
{
  public event System.Action<Item> ItemVomited;

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

  public bool IsBusy { get { return _itemIsInUse; } }

  [SerializeField]
  private float _moveSpeed = 1.0f;

  [SerializeField]
  private Rigidbody _rigidBody = null;

  [SerializeField]
  private Transform _heldItemAnchor = null;

  [SerializeField]
  private Transform _vomitAnchor = null;

  [SerializeField]
  private Vector3 _throwForceLocal = new Vector3(0, 5, 5);

  [SerializeField]
  private CharacterAnimator _characterAnimator = null;

  [SerializeField]
  private Item.TransmitTypeEnum _transmuteType;

  private Vector3 _moveDirection;
  private Vector3 _lastMoveDirection;
  private Item _heldItem;
  private Item _pendingItemPickup;
  private Item _pendingItemVomit;
  private Character _pendingTransmitCharacter;
  private bool _itemIsInUse;
  private Transform _heldItemOriginalParent;

  public void HoldItem(Item item)
  {
    if (_heldItem == null && !_itemIsInUse)
    {
      _heldItemOriginalParent = item.transform.parent;
      _pendingItemPickup = item;
      _itemIsInUse = true;
      item.IsBeingHeld = true;

      if (_characterAnimator != null)
      {
        _characterAnimator.ItemPickedUp += OnAnimationItemPickedUp;
        _characterAnimator.PickUp();
      }
      else
      {
        OnAnimationItemPickedUp();
      }
    }
  }

  public void DropItem()
  {
    if (_heldItem != null && !_itemIsInUse)
    {
      _itemIsInUse = true;
      if (_characterAnimator != null)
      {
        _characterAnimator.ItemThrown += OnAnimationItemThrown;
        _characterAnimator.Throw();
      }
      else
      {
        OnAnimationItemThrown();
      }
    }
  }

  public void TransmitItem(Character targetCharacter)
  {
    if (_heldItem != null && !_itemIsInUse)
    {
      _itemIsInUse = true;
      _pendingTransmitCharacter = targetCharacter;

      if (_characterAnimator != null)
      {
        _characterAnimator.ItemEaten += OnAnimationItemEaten;
        _characterAnimator.Eat();
      }
      else
      {
        OnAnimationItemEaten();
      }
    }
  }

  public void ReceiveItem(Item item)
  {
    _pendingItemVomit = item;
    _itemIsInUse = true;

    if (_characterAnimator != null)
    {
      _characterAnimator.ItemVomited += OnAnimationItemVomited;
      _characterAnimator.Vomit();
    }
    else
    {
      OnAnimationItemVomited();
    }
  }

  private void Update()
  {
    Vector3 moveVector = MoveDirection * _moveSpeed * Time.deltaTime;
    moveVector.y = 0;

    if (IsBusy)
    {
      moveVector = Vector3.zero;
    }

    bool isWalking = moveVector.sqrMagnitude > 0;
    if (isWalking)
    {
      _lastMoveDirection = moveVector;
    }

    if (_characterAnimator != null)
    {
      _characterAnimator.IsWalking = isWalking;
    }

    _rigidBody.MovePosition(_rigidBody.position + moveVector);

    if (_lastMoveDirection.sqrMagnitude > 0)
    {
      Quaternion facingRotation = Quaternion.LookRotation(_lastMoveDirection);
      _rigidBody.rotation = Mathfx.Damp(transform.rotation, facingRotation, 0.5f, Time.deltaTime * 5.0f);
    }
  }

  private void TransmuteItem(Item item)
  {
    switch (_transmuteType)
    {
      case Item.TransmitTypeEnum.Face:
        item.TransmuteFace();
        break;
      case Item.TransmitTypeEnum.Shape:
        item.TransmuteShape();
        break;
    }
  }

  private void OnAnimationItemThrown()
  {
    if (_characterAnimator != null)
    {
      _characterAnimator.ItemThrown -= OnAnimationItemThrown;
      _characterAnimator.IsCarrying = false;
    }

    _heldItem.transform.SetParent(_heldItemOriginalParent);
    _heldItem.IsBeingHeld = false;
    _itemIsInUse = false;

    Vector3 throwForce = transform.TransformDirection(_throwForceLocal);
    _heldItem.Rigidbody.AddForce(throwForce, ForceMode.Impulse);
    _heldItem = null;
  }

  private void OnAnimationItemPickedUp()
  {
    if (_characterAnimator != null)
    {
      _characterAnimator.ItemPickedUp -= OnAnimationItemPickedUp;
      _characterAnimator.IsCarrying = true;
    }

    _heldItem = _pendingItemPickup;
    _heldItem.transform.SetParent(_heldItemAnchor);
    _heldItem.transform.localPosition = Vector3.zero;
    _heldItem.transform.localRotation = Quaternion.identity;
    _itemIsInUse = false;

    _pendingItemPickup = null;
  }

  private void OnAnimationItemEaten()
  {
    if (_characterAnimator != null)
    {
      _characterAnimator.ItemEaten -= OnAnimationItemEaten;
      _characterAnimator.IsCarrying = false;
    }

    _heldItem.transform.SetParent(_heldItemOriginalParent);
    _heldItem.gameObject.SetActive(false);

    TransmuteItem(_heldItem);
    _pendingTransmitCharacter.ReceiveItem(_heldItem);
    _heldItem = null;
    _itemIsInUse = false;

    _pendingTransmitCharacter = null;
  }

  private void OnAnimationItemVomited()
  {
    if (_characterAnimator != null)
    {
      _characterAnimator.ItemVomited -= OnAnimationItemVomited;
    }

    _pendingItemVomit.transform.SetPositionAndRotation(_vomitAnchor.position, _vomitAnchor.rotation);

    Vector3 throwForce = transform.TransformDirection(_throwForceLocal) * 1.5f;
    _pendingItemVomit.gameObject.SetActive(true);
    _pendingItemVomit.IsBeingHeld = false;
    _pendingItemVomit.Rigidbody.AddForce(throwForce, ForceMode.Impulse);

    if (ItemVomited != null)
    {
      ItemVomited(_pendingItemVomit);
    }

    _itemIsInUse = false;
    _pendingItemVomit = null;
  }
}