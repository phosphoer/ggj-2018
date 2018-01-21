using UnityEngine;
using System.Collections;

public class PlayerRewiredLink : MonoBehaviour
{
  public Rewired.Player RewiredPlayer
  {
    get { return Rewired.ReInput.players.GetPlayer(RewiredPlayerID); }
  }

  public Rewired.ControllerType ControllerType
  {
    get
    {
      return Rewired.ReInput.controllers.GetLastActiveControllerType();
    }
  }

  public int RewiredPlayerID;
}