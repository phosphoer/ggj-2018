using UnityEngine;
using Rewired;

public class PlayerController : MonoBehaviour
{
  [SerializeField]
  private Character _character = null;

  [SerializeField]
  private int _rewiredInputId;

  private Rewired.Player _rewiredPlayer;

  private void Start()
  {
    _rewiredPlayer = Rewired.ReInput.players.GetPlayer(_rewiredInputId);
  }

  private void Update()
  {
    // Wait for input system 
    if (!Rewired.ReInput.isReady)
    {
      return;
    }

    float axisHorizontal = _rewiredPlayer.GetAxis(InputActions.MoveHorizontal);
    float axisVertical = _rewiredPlayer.GetAxis(InputActions.MoveVertical);
    _character.MoveDirection = new Vector3(axisHorizontal, 0, axisVertical);
  }
}