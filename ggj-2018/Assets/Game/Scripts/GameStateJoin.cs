using System.Collections;
using UnityEngine;

public class GameStateJoin : ActionBase
{
  [SerializeField]
  private int _waitForPlayerCount = 2;

  [SerializeField]
  private Transform[] _spawnPoints = null;

  [SerializeField]
  private Camera _defaultCamera = null;

  protected override IEnumerator DoActionAsync()
  {
    yield return null;

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
    }
  }
}