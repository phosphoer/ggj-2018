using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : Singleton<GameStateManager>
{
  private GameState _currentState;
  private string _nextSceneName;

  public void GoToScene(string sceneName)
  {
    _nextSceneName = sceneName;
  }

  public void RestartScene()
  {
    if (GameState.Instance != null)
    {
      _nextSceneName = GameState.Instance.gameObject.scene.name;
    }
  }

  private void OnGameStateStart(GameState state)
  {
    _currentState = state;
    SceneManager.SetActiveScene(_currentState.gameObject.scene);
  }

  private void Awake()
  {
    Application.runInBackground = true;
    GameState.GameStateStart += OnGameStateStart;
    Instance = this;
  }

  private void Update()
  {
    if (!string.IsNullOrEmpty(_nextSceneName))
    {
      // Unload current level
      if (_currentState != null)
      {
        SceneManager.UnloadSceneAsync(_currentState.gameObject.scene);
      }

      // Load next level
      SceneManager.LoadSceneAsync(_nextSceneName, LoadSceneMode.Additive);
      _nextSceneName = null;
    }
  }
}