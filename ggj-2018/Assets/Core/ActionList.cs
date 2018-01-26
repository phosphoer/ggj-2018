using UnityEngine;
using System.Collections;

public class ActionList : MonoBehaviour
{
  public enum LifetimeModeType
  {
    GameObject,
    Behaviour,
  }

  public bool BeginActionsOnStart;
  public bool LoopActions;
  public LifetimeModeType ActionLifetimeMode;

  private bool _cancelled;

  public Coroutine StartActions()
  {
    _cancelled = false;
    return StartCoroutine(ExecuteActionsAsync());
  }

  public void StopActions()
  {
    _cancelled = true;
  }

  private void Awake()
  {
    foreach (Transform child in transform)
    {
      ActionBase action = child.GetComponent<ActionBase>();
      if (action != null)
      {
        SetActionEnabled(action, false);
      }
    }
  }

  private void Start()
  {
    if (BeginActionsOnStart)
    {
      StartActions();
    }
  }

  private void SetActionEnabled(ActionBase action, bool isEnabled)
  {
    if (ActionLifetimeMode == LifetimeModeType.Behaviour)
    {
      action.enabled = isEnabled;
    }
    else if (ActionLifetimeMode == LifetimeModeType.GameObject)
    {
      action.gameObject.SetActive(isEnabled);
    }
  }

  private IEnumerator ExecuteActionsAsync()
  {
    do
    {
      for (int i = 0; i < transform.childCount; ++i)
      {
        yield return null;

        if (_cancelled)
        {
          yield break;
        }

        Transform child = transform.GetChild(i);
        ActionBase action = child.GetComponent<ActionBase>();
        if (action != null)
        {
          SetActionEnabled(action, true);
          action.ResetActionState();

          yield return action.StartAction();

          SetActionEnabled(action, false);
        }
      }

      yield return null;
    } while (LoopActions && !_cancelled);
  }
}