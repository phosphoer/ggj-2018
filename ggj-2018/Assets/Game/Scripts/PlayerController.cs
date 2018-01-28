using UnityEngine;
using Rewired;
using System;

public class PlayerController : MonoBehaviour
{
  public static event System.Action<PlayerController> Spawned;

  public PlayerTeam PlayerTeam { get; set; }
  public Character Character { get { return _character; } }
  public CameraRig CameraRig { get { return _cameraRig; } }

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

  [SerializeField]
  private float _transmitScreenShakeDuration = 0.25f;

  [SerializeField]
  private float _transmitScreenShakeMagnitude = 0.1f;

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

    if (_interactionController.ClosestInteractable != null)
    {
      Maw maw = _interactionController.ClosestInteractable.GetComponent<Maw>();
      _cameraRig.IsZoomedOut = maw != null;
      if (maw != null && _character.HeldItem != null)
      {
        maw.IsOpen = true;
      }
    }
    else
    {
      _cameraRig.IsZoomedOut = false;
    }

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
          item.OwnedByPlayer = this;
          _character.HoldItem(item);
        }
      }
    }

    // Transmit an item if we have one and the button is pressed
    if (_character.HeldItem != null && _rewiredPlayer.GetButtonDown(InputActions.Transmit))
    {
      PlayerController targetPlayer = PlayerTeam.GetOtherPlayer(this);
      if (targetPlayer != null)
      {
        _character.TransmitItem(targetPlayer.Character);
      }
      else
      {
        Debug.LogError("Tried to transmit an item but no second player");
      }
    }
  }

  private void OnCharacterVomited(Item item)
  {
    item.OwnedByPlayer = this;
    _cameraRig.Shake(_transmitScreenShakeDuration, _transmitScreenShakeMagnitude);
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
    Character characterPrefab = _characterPrefabs[Player.GetPlayerIndex(_player)];
    _character = Instantiate(characterPrefab, transform);
    _character.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
    _character.ItemVomited += OnCharacterVomited;

    // Set up interaction controller 
    _interactionController.TrackedTransform = _character.transform;

    // Spawn the camera
    _cameraRig = Instantiate(_playerCameraPrefab, transform);
    _cameraRig.TrackedTransform = _character.transform;
    _cameraRig.Camera.cullingMask |= 1 << LayerMask.NameToLayer(_player.ExclusiveLayerName);

    if (Camera.main != null)
    {
      _cameraRig.transform.SetPositionAndRotation(Camera.main.transform.position, Camera.main.transform.rotation);
    }
    else
    {
      _cameraRig.transform.position = new Vector3(0, 4, -10);
      _cameraRig.transform.LookAt(Vector3.zero);
    }

    // Update splitscreen
    _splitscreenPlayer.PlayerCamera = _cameraRig.Camera;
    SplitscreenPlayer.UpdateViewports();

    if (Spawned != null)
    {
      Spawned(this);
    }
  }
}