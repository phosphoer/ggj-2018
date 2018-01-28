public class PlayerTeam
{
  public bool IsFull
  {
    get { return PlayerA != null && PlayerB != null; }
  }

  public PlayerController PlayerA;
  public PlayerController PlayerB;

  public bool AddPlayer(PlayerController playerController)
  {
    bool success = false;

    if (PlayerA == null)
    {
      PlayerA = playerController;
      success = true;
    }
    else if (PlayerB == null)
    {
      PlayerB = playerController;
      success = true;
    }

    playerController.PlayerTeam = this;

    return success;
  }

  public PlayerController GetOtherPlayer(PlayerController forPlayer)
  {
    if (PlayerA == forPlayer || PlayerB == forPlayer)
    {
      return PlayerA == forPlayer ? PlayerB : PlayerA;
    }

    return null;
  }
}