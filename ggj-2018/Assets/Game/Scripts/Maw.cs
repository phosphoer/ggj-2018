using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Maw : MonoBehaviour
{
  public bool IsOpen
  {
    get { return _isOpen; }
    set
    {
      _isOpen = value;
      _animator.SetBool("Open", _isOpen);
    }
  }

  [SerializeField]
  private Interactable _interactable = null;

  [SerializeField]
  private Animator _animator = null;

  [SerializeField]
  private Transform _eyesTransform = null;

  [SerializeField]
  private Item _itemPrefab = null;

  [SerializeField]
  private int _losingMistakeCount = 5;

  [SerializeField]
  private int _winningCorrectCount = 5;

  [SerializeField]
  private float _eyeBlinkTime = 0.25f;

  [SerializeField]
  private AnimationCurve _eyeBlinkScaleCurve;

  [SerializeField]
  private float _eyeBlinkIntervalMin = 3.0f;

  [SerializeField]
  private float _eyeBlinkIntervalMax = 10.0f;

  [SerializeField]
  private GameObject[] _victoryTorches = null;

  [SerializeField]
  private SoundBank _angrySound = null;

  [SerializeField]
  private SoundBank _eatSound = null;

  [SerializeField]
  private SoundBank _talkSound = null;

  [SerializeField]
  private int _typeCountPerGame = 3;

  [SerializeField]
  private int _creatureCount = 50;

  [SerializeField]
  private Transform[] _leftSpawnPoints = null;

  [SerializeField]
  private Transform[] _rightSpawnPoints = null;

  [SerializeField]
  private Transform _desiredItemSpawn = null;

  [SerializeField]
  private Transform _speechBubbleUI = null;

  [SerializeField]
  private GameObject _speechXIcon = null;

  private int _mistakeCount;
  private int _correctCount;
  private Dictionary<PlayerController, Item.ItemDefinition> _desiredItems;
  private Dictionary<PlayerController, Item> _desiredItemsDisplay;
  private List<int> _faceIndicesThisGame = new List<int>();
  private List<int> _shapeIndicesThisGame = new List<int>();
  private float _eyeBlinkTimer;
  private Vector3 _eyeOriginalScale;
  private float _eyeBlinkScale = 1.0f;
  private float _eyeOpenScale = 1.0f;
  private bool _isOpen;
  private int _nearbyPlayerCount;
  private float _spawnTimer;

  private void Awake()
  {
    _desiredItems = new Dictionary<PlayerController, Item.ItemDefinition>();
    _desiredItemsDisplay = new Dictionary<PlayerController, Item>();
    _eyeOriginalScale = _eyesTransform.localScale;
    _interactable.PromptShown += OnPromptShown;
    _interactable.PromptHidden += OnPromptHidden;
    PlayerController.Spawned += OnPlayerSpawned;

    // Pick which types will be in this game
    List<int> faceIndices = new List<int>();
    List<int> shapeIndices = new List<int>();
    for (int i = 0; i < _itemPrefab.TypeCount; ++i)
    {
      faceIndices.Add(i);
      shapeIndices.Add(i);
    }

    for (int i = 0; i < _typeCountPerGame; ++i)
    {
      int chosenFaceIndex = faceIndices[Random.Range(0, faceIndices.Count)];
      int chosenShapeIndex = shapeIndices[Random.Range(0, shapeIndices.Count)];
      _faceIndicesThisGame.Add(chosenFaceIndex);
      _shapeIndicesThisGame.Add(chosenShapeIndex);
      faceIndices.Remove(chosenFaceIndex);
      shapeIndices.Remove(chosenShapeIndex);
    }

    foreach (GameObject victoryTorch in _victoryTorches)
    {
      victoryTorch.transform.localScale = Vector3.one;
    }

    _speechBubbleUI.localScale = Vector3.zero;
  }

  private void Update()
  {
    // Blinking
    _eyeBlinkTimer -= Time.deltaTime;
    if (_eyeBlinkTimer < 0)
    {
      _eyeBlinkTimer = Random.Range(_eyeBlinkIntervalMin, _eyeBlinkIntervalMax);
      StartCoroutine(EyeBlinkRoutine());
    }

    // Scale the eyes
    _eyeOpenScale = Mathfx.Damp(_eyeOpenScale, IsOpen ? 1.5f : 1.0f, 0.5f, Time.deltaTime * 5.0f);
    Vector3 scale = _eyesTransform.localScale;
    scale.x = _eyeOriginalScale.x * _eyeBlinkScale * _eyeOpenScale;
    scale.z = _eyeOriginalScale.z * _eyeOpenScale;
    _eyesTransform.localScale = scale;

    // Light torches
    for (int i = 0; i < _victoryTorches.Length; ++i)
    {
      GameObject victoryTorch = _victoryTorches[i];
      bool isLit = _mistakeCount <= i;
      victoryTorch.transform.localScale = Mathfx.Damp(victoryTorch.transform.localScale, isLit ? Vector3.one : Vector3.zero, 0.5f, Time.deltaTime);
      victoryTorch.gameObject.SetActive(victoryTorch.transform.localScale.x > 0.05f);
    }

    // Show speech bubble 
    Vector3 desiredScale = _nearbyPlayerCount > 0 ? Vector3.one : Vector3.zero;
    _speechBubbleUI.localScale = Mathfx.Damp(_speechBubbleUI.localScale, desiredScale, 0.5f, Time.deltaTime * 3.0f);

    // Spawn creatures 
    _spawnTimer -= Time.deltaTime;
    if (_spawnTimer < 0 && Item.InstanceCount < _creatureCount)
    {
      _spawnTimer = 5.0f;

      Item.ItemDefinition itemDef = new Item.ItemDefinition();
      itemDef.FaceIndex = _faceIndicesThisGame[Random.Range(0, _faceIndicesThisGame.Count)];
      itemDef.ShapeIndex = _shapeIndicesThisGame[Random.Range(0, _shapeIndicesThisGame.Count)];

      Item creature = Instantiate(_itemPrefab);
      creature.Definition = itemDef;

      Transform spawnPoint = Random.value > 0.5f ?
        _leftSpawnPoints[Random.Range(0, _leftSpawnPoints.Length)] :
        _rightSpawnPoints[Random.Range(0, _rightSpawnPoints.Length)];

      creature.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
    }
  }

  private IEnumerator EyeBlinkRoutine()
  {
    for (float time = 0; time < _eyeBlinkTime; time += Time.deltaTime)
    {
      float blinkT = time / _eyeBlinkTime;
      _eyeBlinkScale = _eyeBlinkScaleCurve.Evaluate(blinkT);
      yield return null;
    }
  }

  private void OnCollisionEnter(Collision collision)
  {
    // If the thing dropped into the maw is an item
    Item item = collision.gameObject.GetComponent<Item>();
    if (item != null)
    {
      _animator.SetTrigger("Eat");

      // If this is the item we wanted from this player
      if (IsCorrectItem(item, item.OwnedByPlayer))
      {
        CorrectChoiceMade(item.OwnedByPlayer);
      }
      // Otherwise we should get mad 
      else
      {
        IncorrectChoiceMade(item.OwnedByPlayer);
      }

      Destroy(item.gameObject);
      IsOpen = false;
    }

    CheckWinLose();
  }

  private void OnPromptShown()
  {
    ++_nearbyPlayerCount;

    if (_talkSound != null)
    {
      AudioManager.Instance.PlaySound(_talkSound);
    }
  }

  private void OnPromptHidden()
  {
    --_nearbyPlayerCount;
    _speechXIcon.SetActive(false);
  }

  private void OnPlayerSpawned(PlayerController playerController)
  {
    PickNewDesiredItem(playerController);
  }

  private void CorrectChoiceMade(PlayerController byPlayer)
  {
    _animator.SetBool("Happy", true);
    AudioManager.Instance.PlaySound(_eatSound);
    PickNewDesiredItem(byPlayer);
    ++_correctCount;
  }

  private void IncorrectChoiceMade(PlayerController byPlayer)
  {
    ++_mistakeCount;

    _animator.SetBool("Happy", false);
    AudioManager.Instance.PlaySound(_angrySound);
    _speechXIcon.SetActive(true);

    foreach (PlayerController playerController in _desiredItems.Keys)
    {
      playerController.CameraRig.Shake(2.0f, 0.5f);
    }
  }

  private bool IsCorrectItem(Item item, PlayerController forPlayer)
  {
    Item.ItemDefinition desiredItem;
    if (_desiredItems.TryGetValue(forPlayer, out desiredItem))
    {
      return desiredItem.FaceIndex == item.Definition.FaceIndex && desiredItem.ShapeIndex == item.Definition.ShapeIndex;
    }

    return false;
  }

  private void CheckWinLose()
  {

  }

  private void PickNewDesiredItem(PlayerController forPlayer)
  {
    Item.ItemDefinition desiredItem;
    desiredItem.FaceIndex = Random.Range(0, _itemPrefab.TypeCount);
    desiredItem.ShapeIndex = Random.Range(0, _itemPrefab.TypeCount);

    _desiredItems[forPlayer] = desiredItem;

    Item itemDisplay;
    if (!_desiredItemsDisplay.TryGetValue(forPlayer, out itemDisplay))
    {
      itemDisplay = Instantiate(_itemPrefab, _desiredItemSpawn);
      itemDisplay.IsBeingHeld = true;
      itemDisplay.transform.localPosition = Vector3.zero;
      itemDisplay.transform.localRotation = Quaternion.identity;
      itemDisplay.transform.localScale = Vector3.one;
      _desiredItemsDisplay[forPlayer] = itemDisplay;
    }

    itemDisplay.Definition = desiredItem;

    Renderer[] renderers = itemDisplay.GetComponentsInChildren<Renderer>();
    foreach (Renderer r in renderers)
    {
      r.gameObject.layer = LayerMask.NameToLayer(forPlayer.GetComponent<Player>().ExclusiveLayerName);
    }
  }
}