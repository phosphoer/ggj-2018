using UnityEngine;
using System;
using System.Collections.Generic;

public class SplitscreenPlayer : MonoBehaviour
{
  public enum Side
  {
    TopLeft,
    TopRight,
    BottomRight,
    BottomLeft,
  }

  public static event Action ViewportUpdated;

  public static List<SplitscreenPlayer> Players = new List<SplitscreenPlayer>();
  public static List<SplitscreenPlayer> JoinedPlayers = new List<SplitscreenPlayer>();
  public static bool SinglePlayer { get { return JoinedPlayers.Count == 1; } }

  public static event Action<SplitscreenPlayer> PlayerJoined;
  public static event Action<SplitscreenPlayer> PlayerLeft;

  public event Action Joined;
  public event Action Left;

  public Side CurrentSide { get; private set; }
  public bool IsJoined { get { return _isJoined; } }
  public Camera PlayerCamera { get; set; }

  public bool EnableDisableControlsLifetime;

  private bool _isJoined;

  private static Rect[] gridLayout = new Rect[4]
  {
    new Rect(0.0f, 0.5f, 0.5f, 0.5f),
    new Rect(0.5f, 0.5f, 0.5f, 0.5f),
    new Rect(0.5f, 0.0f, 0.5f, 0.5f),
    new Rect(0.0f, 0.0f, 0.5f, 0.5f),
  };
  private static Rect[] threeGridLayout = new Rect[3]
  {
    new Rect(0, 0, 0.5f, 1.0f),
    new Rect(0.5f, 0.5f, 0.5f, 0.5f),
    new Rect(0.5f, 0.0f, 0.5f, 0.5f),
  };

  public void MakeUICanvasExclusive(Canvas ui)
  {
    ui.renderMode = RenderMode.ScreenSpaceCamera;
    ui.worldCamera = PlayerCamera;
    ui.planeDistance = 1.25f;
  }

  public void Join()
  {
    if (_isJoined)
      return;

    JoinedPlayers.Add(this);
    _isJoined = true;
    var handler = Joined;
    if (handler != null)
      handler();
    if (PlayerJoined != null)
      PlayerJoined(this);
    UpdateViewports();
  }

  public void Leave()
  {
    if (!_isJoined)
      return;

    JoinedPlayers.Remove(this);
    _isJoined = false;
    var handler = Left;
    if (handler != null)
      handler();
    if (PlayerLeft != null)
      PlayerLeft(this);
    UpdateViewports();
  }

  public static void UpdateViewports()
  {
    // Vertical split screen for 2 players (also handles 1 player)
    if (JoinedPlayers.Count <= 2)
    {
      for (var i = 0; i < JoinedPlayers.Count; ++i)
      {
        var p = JoinedPlayers[i];
        var rectX = (1.0f / JoinedPlayers.Count) * i;
        if (p.PlayerCamera != null)
          p.PlayerCamera.rect = new Rect(rectX, 0, 1.0f / JoinedPlayers.Count, 1.0f);
        p.CurrentSide = i == 0 ? Side.TopLeft : Side.TopRight;
      }
    }
    // Grid layout for 3 and 4 players
    else
    {
      var grid = gridLayout;
      if (JoinedPlayers.Count == 3)
        grid = threeGridLayout;

      for (var i = 0; i < JoinedPlayers.Count; ++i)
      {
        var p = JoinedPlayers[i];
        p.CurrentSide = (Side)i;
        if (p.PlayerCamera != null)
          p.PlayerCamera.rect = grid[i];
      }
    }

    if (ViewportUpdated != null)
    {
      ViewportUpdated();
    }
  }

  public static void DisableCameras()
  {
    for (int i = 0; i < JoinedPlayers.Count; ++i)
    {
      if (JoinedPlayers[i].PlayerCamera != null)
      {
        JoinedPlayers[i].PlayerCamera.enabled = false;
      }
    }
  }

  public static void EnableCameras()
  {
    for (int i = 0; i < JoinedPlayers.Count; ++i)
    {
      if (JoinedPlayers[i].PlayerCamera != null)
      {
        JoinedPlayers[i].PlayerCamera.enabled = true;
      }
    }
  }

  private void Awake()
  {
    Players.Add(this);
  }

  private void OnDestroy()
  {
    Players.Remove(this);
  }

  private void OnEnable()
  {
    if (EnableDisableControlsLifetime)
    {
      Join();
    }
  }

  private void OnDisable()
  {
    if (EnableDisableControlsLifetime)
    {
      Leave();
    }
  }
}