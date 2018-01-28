using UnityEngine;
using System.Collections.Generic;

public class Interactable : MonoBehaviour
{
  public static int InstanceCount { get { return _instances.Count; } }
  public static IEnumerable<Interactable> Instances { get { return _instances; } }

  public string InteractionText
  {
    get { return _interactionText; }
    set
    {
      _interactionText = value;
      _interactableUI.InteractionText = value;
    }
  }

  public float InteractionRadius
  {
    get { return _interactionRadius; }
    set { _interactionRadius = value; }
  }

  public Vector3 DesiredPosition
  {
    get { return _interactionUIAnchor.position + Vector3.up * _interactionUIHeight; }
  }

  public event System.Action<PlayerController> InteractionTriggered;
  public event System.Action PromptShown;
  public event System.Action PromptHidden;

  [SerializeField]
  private InteractableUI _interactableUIPrefab = null;

  [SerializeField]
  private Transform _interactionUIAnchor = null;

  [SerializeField]
  private float _interactionUIHeight = 8.0f;

  [SerializeField]
  private string _interactionText = "Interact";

  [SerializeField]
  private float _interactionRadius = 50.0f;

  private InteractableUI _interactableUI;
  private bool _firstUpdate;

  private static List<Interactable> _instances = new List<Interactable>();

  public static Interactable GetInstance(int instanceIndex)
  {
    return _instances[instanceIndex];
  }

  private void OnEnable()
  {
    _instances.Add(this);
    _firstUpdate = true;

    if (_interactableUIPrefab != null)
    {
      _interactableUI = Instantiate(_interactableUIPrefab);
      _interactableUI.gameObject.SetActive(false);
    }
  }

  private void OnDisable()
  {
    _instances.Remove(this);

    if (_interactableUI != null)
    {
      Destroy(_interactableUI.gameObject);
    }
  }

  private void Update()
  {
    if (_interactionUIAnchor != null && _interactableUI != null)
    {
      if (_firstUpdate)
      {
        _interactableUI.transform.position = DesiredPosition;
        _firstUpdate = false;
      }
      else
      {
        _interactableUI.transform.position = Mathfx.Damp(_interactableUI.transform.position, DesiredPosition, 0.5f, Time.deltaTime * 5.0f);
      }
    }
  }

  public void TriggerInteraction(PlayerController fromPlayer)
  {
    if (InteractionTriggered != null)
    {
      InteractionTriggered(fromPlayer);
    }

    HidePrompt();
  }

  public void ShowPrompt()
  {
    if (_interactableUI != null)
    {
      _interactableUI.gameObject.SetActive(true);
      _interactableUI.InteractionText = _interactionText;
    }

    if (PromptShown != null)
    {
      PromptShown();
    }
  }

  public void HidePrompt()
  {
    if (_interactableUI != null)
    {
      _interactableUI.gameObject.SetActive(false);
    }

    if (PromptHidden != null)
    {
      PromptHidden();
    }
  }
}