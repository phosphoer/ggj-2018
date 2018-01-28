using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
  public event System.Action ItemPickedUp;
  public event System.Action ItemThrown;
  public event System.Action ItemEaten;
  public event System.Action ItemVomited;

  public bool IsWalking
  {
    get { return _animator.GetBool("Walking"); }
    set { _animator.SetBool("Walking", value); }
  }

  public bool IsCarrying
  {
    get { return _animator.GetBool("Carrying"); }
    set { _animator.SetBool("Carrying", value); }
  }

  public void PickUp()
  {
    _animator.SetTrigger("Pickup");
  }

  public void Throw()
  {
    _animator.SetTrigger("Throw");
  }

  public void Eat()
  {
    _animator.SetTrigger("Eat");
  }

  public void Vomit()
  {
    _animator.SetTrigger("Vomit");
  }

  private void AnimationEventThrow()
  {
    if (ItemThrown != null)
    {
      ItemThrown();
    }
  }

  private void AnimationEventPickup()
  {
    if (ItemPickedUp != null)
    {
      ItemPickedUp();
    }
  }

  private void AnimationEventEat()
  {
    if (ItemEaten != null)
    {
      ItemEaten();
    }
  }

  private void AnimationEventVomit()
  {
    if (ItemVomited != null)
    {
      ItemVomited();
    }
  }

  [SerializeField]
  private Animator _animator = null;
}