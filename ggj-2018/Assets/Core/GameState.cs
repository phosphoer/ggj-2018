using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameState : Singleton<GameState>
{
  // Name of the core scene to load
  private const string CoreScene = "core";

  public static event Action<GameState> GameStateStart;

  private bool _deferringGameStart;

  private void Awake()
  {
    Instance = this;
  }

  private void Start()
  {
    // Load core scene
    if (GameStateManager.Instance == null)
    {
      if (!string.IsNullOrEmpty(CoreScene))
      {
        SceneManager.LoadSceneAsync(CoreScene, LoadSceneMode.Additive);
        StartCoroutine(DeferredGameStart());
      }
    }
    else
    {
      if (GameStateStart != null)
        GameStateStart(this);
    }
  }

  private IEnumerator DeferredGameStart()
  {
    while (GameStateManager.Instance == null)
    {
      yield return null;
    }

    if (GameStateStart != null)
      GameStateStart(this);
  }
}