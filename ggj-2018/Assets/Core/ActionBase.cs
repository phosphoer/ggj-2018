using UnityEngine;
using System.Collections;

public abstract class ActionBase : MonoBehaviour
{
  public Coroutine StartAction()
  {
    return StartCoroutine(DoActionAsync());
  }

  public virtual void ResetActionState()
  {

  }

  protected abstract IEnumerator DoActionAsync();
}