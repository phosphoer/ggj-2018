using UnityEngine;

public class InteractableUI : MonoBehaviour
{
  public string InteractionText
  {
    get { return _interactionTextUI.text; }
    set { _interactionTextUI.text = value; }
  }

  [SerializeField]
  private TextMesh _interactionTextUI = null;
}