using UnityEngine;
using Rewired;
using System;

public class PlayerController : MonoBehaviour
{
  [SerializeField]
  private Player _player = null;

  [SerializeField]
  private SplitscreenPlayer _splitscreenPlayer = null;

  [SerializeField]
  private PlayerRewiredLink _rewiredLink = null;

  [SerializeField]
  private InteractionController _interactionController = null;

  [SerializeField]
  private Character[] _characterPrefabs = null;

  [SerializeField]
  private CameraRig _playerCameraPrefab = null;

  private Rewired.Player _rewiredPlayer;
  private Character _character;
  private CameraRig _cameraRig;

  private void Awake()
  {
    _player.Spawned += OnPlayerSpawned;
  }

  private void Start()
  {
    _rewiredPlayer = _rewiredLink.RewiredPlayer;
  }

  private void OnDestroy()
  {
    _player.Spawned -= OnPlayerSpawned;
  }

  private void Update()
  {
    // Wait for input system 
    if (!Rewired.ReInput.isReady || _character == null)
    {
      return;
    }

    // Move character 
    float axisHorizontal = _rewiredPlayer.GetAxis(InputActions.MoveHorizontal);
    float axisVertical = _rewiredPlayer.GetAxis(InputActions.MoveVertical);
    _character.MoveDirection = new Vector3(axisHorizontal, 0, axisVertical);

    // Try to pick up an interactable if we aren't holding one
    if (_rewiredPlayer.GetButtonDown(InputActions.PickupDrop))
    {
      if (_character.HeldItem != null)
      {
        _character.DropItem();
      }
      else if (_interactionController.ClosestInteractable != null)
      {
        Item item = _interactionController.ClosestInteractable.GetComponent<Item>();
        if (item != null)
        {
          _character.HoldItem(item);
        }
      }
    }
  }

  private void OnPlayerSpawned(Transform spawnPoint)
  {
    // Remove existing character
    if (_character != null)
    {
      Destroy(_character.gameObject);
      _character = null;
    }

    // Spawn the appropriate character based on which player we are
    Character characterPrefab = _characterPrefabs[Player.PlayerCount - 1];
    _character = Instantiate(characterPrefab, transform);
    _character.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

    // Set up interaction controller 
    _interactionController.TrackedTransform = _character.transform;

    // Spawn the camera
    _cameraRig = Instantiate(_playerCameraPrefab, transform);
    _cameraRig.transform.position = new Vector3(0, 4, -10);
    _cameraRig.transform.LookAt(Vector3.zero);

    // Update splitscreen
    _splitscreenPlayer.PlayerCamera = _cameraRig.Camera;
    SplitscreenPlayer.UpdateViewports();
  }
}