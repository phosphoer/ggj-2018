using UnityEngine;

public class InteractableUI : MonoBehaviour
{
  public string InteractionText
  {
    get { return _interactionTextUI.text; }
    set { if (_interactionTextUI != null) _interactionTextUI.text = value; }
  }

  [SerializeField]
  private TextMesh _interactionTextUI = null;
}