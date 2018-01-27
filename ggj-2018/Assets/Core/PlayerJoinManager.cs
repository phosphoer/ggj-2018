using System.Collections.Generic;

public class PlayerJoinManager : Singleton<PlayerJoinManager>
{
  public Player PlayerPrefab;
  public string[] ExcludedJoinActions;
  public int MaxPlayers = 1;

  private bool[] _joinedStates;
  private List<Player> _joinedPlayers = new List<Player>();

  private void Awake()
  {
    Player.PlayerJoined += OnPlayerJoined;
    Player.PlayerLeft += OnPlayerLeft;
  }

  private void Update()
  {
    if (!Rewired.ReInput.isReady || Rewired.ReInput.players.playerCount == 0)
      return;

    if (_joinedStates == null || _joinedStates.Length != Rewired.ReInput.players.playerCount)
      _joinedStates = new bool[Rewired.ReInput.players.playerCount];

    for (int i = 0; i < Rewired.ReInput.players.playerCount; ++i)
    {
      Rewired.Player p = Rewired.ReInput.players.Players[i];
      if (!_joinedStates[p.id] && p.GetAnyButtonDown())
      {
        bool excluded = false;
        if (ExcludedJoinActions != null)
        {
          foreach (string action in ExcludedJoinActions)
            excluded |= p.GetButton(action);
        }

        if (_joinedPlayers.Count >= MaxPlayers)
          excluded = true;

        if (!excluded)
        {
          Player playerObj = Instantiate(PlayerPrefab);
          playerObj.transform.SetParent(transform, false);
          playerObj.name = string.Format("player-{0}", i);

          PlayerRewiredLink rewiredLink = playerObj.GetComponent<PlayerRewiredLink>();
          if (rewiredLink != null)
          {
            rewiredLink.RewiredPlayerID = p.id;
          }

          playerObj.Join();
          _joinedPlayers.Add(playerObj);
        }
      }
    }
  }

  private void OnDestroy()
  {
    Player.PlayerJoined -= OnPlayerJoined;
    Player.PlayerLeft -= OnPlayerLeft;

    for (int i = 0; i < _joinedPlayers.Count; ++i)
    {
      if (_joinedPlayers[i] != null)
      {
        _joinedPlayers[i].Leave();
        Destroy(_joinedPlayers[i].gameObject);
      }
    }
  }

  private void OnPlayerJoined(Player player)
  {
    PlayerRewiredLink rewiredLink = player.GetComponent<PlayerRewiredLink>();
    if (rewiredLink != null)
    {
      _joinedStates[rewiredLink.RewiredPlayerID] = true;
    }
  }

  private void OnPlayerLeft(Player player)
  {
    PlayerRewiredLink rewiredLink = player.GetComponent<PlayerRewiredLink>();
    if (rewiredLink != null)
    {
      _joinedStates[rewiredLink.RewiredPlayerID] = false;
    }
  }
}