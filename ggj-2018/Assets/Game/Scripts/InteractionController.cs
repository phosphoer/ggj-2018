using UnityEngine;

public class InteractionController : MonoBehaviour
{
  public Transform TrackedTransform
  {
    get { return _trackedTransform; }
    set { _trackedTransform = value; }
  }

  public Interactable ClosestInteractable { get { return _closestInteractable; } }

  [SerializeField]
  private Transform _trackedTransform = null;

  private int _lazyUpdateIndex;
  private Interactable _closestInteractable;

  private void Update()
  {
    if (_lazyUpdateIndex < Interactable.InstanceCount)
    {
      float distToClosest = Mathf.Infinity;
      if (_closestInteractable != null)
      {
        distToClosest = Vector3.Distance(_trackedTransform.position, _closestInteractable.transform.position);
        if (distToClosest >= _closestInteractable.InteractionRadius)
        {
          _closestInteractable.HidePrompt();
          _closestInteractable = null;
          distToClosest = Mathf.Infinity;
        }
      }

      Interactable interactable = Interactable.GetInstance(_lazyUpdateIndex);
      float distToInteractable = Vector3.Distance(_trackedTransform.position, interactable.transform.position);
      if (distToInteractable < distToClosest && distToInteractable < interactable.InteractionRadius && interactable != _closestInteractable)
      {
        if (_closestInteractable != null)
        {
          _closestInteractable.HidePrompt();
        }

        _closestInteractable = interactable;
        _closestInteractable.ShowPrompt();
      }
    }
    else
    {
      _closestInteractable = null;
    }

    if (Interactable.InstanceCount > 0)
    {
      _lazyUpdateIndex = (_lazyUpdateIndex + 1) % Interactable.InstanceCount;
    }
  }
}