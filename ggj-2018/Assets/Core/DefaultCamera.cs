using UnityEngine;

public class DefaultCamera : MonoBehaviour
{
  private void OnPlayerJoined(SplitscreenPlayer player)
  {
    gameObject.SetActive(false);
  }

  private void OnPlayerLeft(SplitscreenPlayer player)
  {
    gameObject.SetActive(SplitscreenPlayer.JoinedPlayers.Count == 0);
  }

  private void Start()
  {
    SplitscreenPlayer.PlayerJoined += OnPlayerJoined;
    SplitscreenPlayer.PlayerLeft += OnPlayerLeft;
  }

  private void OnDestroy()
  {
    SplitscreenPlayer.PlayerJoined -= OnPlayerJoined;
    SplitscreenPlayer.PlayerLeft -= OnPlayerLeft;
  }
}