using UnityEngine;
using Rewired;
using System;

public class PlayerController : MonoBehaviour
{
  [SerializeField]
  private Player _player = null;

  [SerializeField]
  private PlayerRewiredLink _rewiredLink = null;

  [SerializeField]
  private Character[] _characterPrefabs = null;

  private Rewired.Player _rewiredPlayer;
  private Character _character;

  private void Awake()
  {
    _player.Spawned += OnPlayerSpawned;
  }

  private void Start()
  {
    _rewiredPlayer = _rewiredLink.RewiredPlayer;
  }

  private void Update()
  {
    // Wait for input system 
    if (!Rewired.ReInput.isReady || _character == null)
    {
      return;
    }

    float axisHorizontal = _rewiredPlayer.GetAxis(InputActions.MoveHorizontal);
    float axisVertical = _rewiredPlayer.GetAxis(InputActions.MoveVertical);
    _character.MoveDirection = new Vector3(axisHorizontal, 0, axisVertical);
  }

  private void OnPlayerSpawned(Transform spawnPoint)
  {
    if (_character != null)
    {
      Destroy(_character.gameObject);
      _character = null;
    }

    // Spawn the appropriate character based on which player we are
    Character characterPrefab = _characterPrefabs[Player.PlayerCount - 1];
    _character = Instantiate(characterPrefab, transform);
    _character.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
  }
}