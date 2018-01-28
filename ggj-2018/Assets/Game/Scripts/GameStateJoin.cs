using System.Collections;
using UnityEngine;

public class GameStateJoin : ActionBase
{
  [SerializeField]
  private int _waitForPlayerCount = 2;

  [SerializeField]
  private Transform[] _spawnPoints = null;

  private PlayerTeam _currentTeam;

  protected override IEnumerator DoActionAsync()
  {
    yield return null;

    PlayerJoinManager.Instance.enabled = true;
    Player.PlayerJoined += OnPlayerJoined;

    while (Player.PlayerCount < _waitForPlayerCount)
    {
      yield return null;
    }

    Player.PlayerJoined -= OnPlayerJoined;
  }

  private void OnPlayerJoined(Player player)
  {
    int spawnIndex = Player.PlayerCount - 1;
    if (_spawnPoints.Length > spawnIndex)
    {
      player.Spawn(_spawnPoints[spawnIndex]);

      if (_currentTeam == null || _currentTeam.IsFull)
      {
        _currentTeam = new PlayerTeam();
      }

      PlayerController playerController = player.GetComponent<PlayerController>();
      if (playerController != null)
      {
        _currentTeam.AddPlayer(playerController);
      }
    }
  }
}