using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Maw : Singleton<Maw>
{
  public bool IsOpen
  {
    get { return _isOpen; }
    set
    {
      _isOpen = value;
      _animator.SetBool("Open", _isOpen);
      AudioManager.Instance.PlaySound(_openSound);
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
  private SoundBank _happySound = null;

  [SerializeField]
  private SoundBank _openSound = null;

  [SerializeField]
  private SoundBank _transformSound = null;

  [SerializeField]
  private SoundBank _fanfareSound = null;

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

  [SerializeField]
  private SkullLightController _skullAltar = null;

  [SerializeField]
  private ParticleSystem _confettiParticle = null;

  [SerializeField]
  private Item _trophyItemPrefab = null;

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

  public int GetNextFaceIndex(int index)
  {
    int nextIndex = _faceIndicesThisGame.IndexOf(index);
    nextIndex = (nextIndex + 1) % _faceIndicesThisGame.Count;
    return _faceIndicesThisGame[nextIndex];
  }

  public int GetNextShapeIndex(int index)
  {
    int nextIndex = _shapeIndicesThisGame.IndexOf(index);
    nextIndex = (nextIndex + 1) % _shapeIndicesThisGame.Count;
    return _shapeIndicesThisGame[nextIndex];
  }

  private void Awake()
  {
    Instance = this;

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

    if (_nearbyPlayerCount == 0)
    {
      IsOpen = false;
    }
  }

  private void OnPlayerSpawned(PlayerController playerController)
  {
    PickNewDesiredItem(playerController);
  }

  private void CorrectChoiceMade(PlayerController byPlayer)
  {
    _animator.SetBool("Happy", true);
    AudioManager.Instance.PlaySound(_eatSound);
    AudioManager.Instance.PlaySound(_happySound);
    PickNewDesiredItem(byPlayer);
    ++_correctCount;

    _skullAltar.EnableNext();
  }

  private void IncorrectChoiceMade(PlayerController byPlayer)
  {
    ++_mistakeCount;

    _animator.SetBool("Happy", false);
    AudioManager.Instance.PlaySound(_eatSound);
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
    if (_correctCount >= _winningCorrectCount)
    {
      Win();
    }
    else if (_mistakeCount >= _losingMistakeCount)
    {
      Lose();
    }
  }

  [ContextMenu("Lose")]
  private void Lose()
  {
    StartCoroutine(LoseRoutine());
  }

  [ContextMenu("Win")]
  private void Win()
  {
    StartCoroutine(WinRoutine());
  }

  private IEnumerator WinRoutine()
  {
    // Zoom out
    PlayerController[] players = FindObjectsOfType<PlayerController>();
    foreach (PlayerController playerController in players)
    {
      playerController.enabled = false;
      playerController.CameraRig.IsZoomedOut = true;
      playerController.CameraRig.IsZoomedIn = false;
      playerController.Character.transform.rotation = Quaternion.LookRotation(Vector3.back);
    }

    yield return new WaitForSeconds(3.0f);

    // Wheeee /s
    AudioManager.Instance.PlaySound(_fanfareSound);
    _confettiParticle.Play();

    yield return new WaitForSeconds(3.0f);

    // Zoom in
    foreach (PlayerController playerController in players)
    {
      playerController.CameraRig.IsZoomedOut = false;
      playerController.CameraRig.IsZoomedIn = true;
    }

    // Give the player a trophy
    yield return new WaitForSeconds(3.0f);
    Item trophyItem = Instantiate(_trophyItemPrefab);
    trophyItem.IsBeingHeld = true;
    trophyItem.gameObject.SetActive(false);

    foreach (PlayerController playerController in players)
    {
      playerController.Character.ReceiveItem(trophyItem);
    }

    yield return new WaitForSeconds(5.0f);

    // Zoom back out
    foreach (PlayerController playerController in players)
    {
      playerController.enabled = true;
      CameraRig cameraRig = playerController.CameraRig;
      cameraRig.IsZoomedIn = false;
      cameraRig.IsZoomedOut = false;
      PickNewDesiredItem(playerController);
    }

    // Wait a bit for good measure
    yield return new WaitForSeconds(5.0f);

    // Reset the maw 
    ResetMaw();
  }

  private IEnumerator LoseRoutine()
  {
    PlayerController[] players = FindObjectsOfType<PlayerController>();
    foreach (PlayerController playerController in players)
    {
      // Zoom into the player 
      playerController.enabled = false;
      CameraRig cameraRig = playerController.CameraRig;
      cameraRig.IsZoomedIn = true;
      cameraRig.IsZoomedOut = false;
      yield return new WaitForSeconds(3.0f);

      // Turn player into a creature 
      Item creature = Instantiate(_itemPrefab);
      creature.Randomize();
      creature.Interactable.enabled = false;
      creature.transform.SetPositionAndRotation(playerController.Character.transform.position, playerController.Character.transform.rotation);
      playerController.Character.gameObject.SetActive(false);
      AudioManager.Instance.PlaySound(_transformSound);
      yield return new WaitForSeconds(3.0f);

      playerController.Player.Respawn();
      playerController.enabled = true;

      PickNewDesiredItem(playerController, creature.Definition);
      creature.Interactable.enabled = true;
    }

    // Reset the maw 
    ResetMaw();
  }

  private void ResetMaw()
  {
    _skullAltar.ResetAll();
    _mistakeCount = 0;
    _correctCount = 0;
  }

  private void PickNewDesiredItem(PlayerController forPlayer)
  {
    Item.ItemDefinition desiredItem;
    desiredItem.FaceIndex = _faceIndicesThisGame[Random.Range(0, _faceIndicesThisGame.Count)];
    desiredItem.ShapeIndex = _shapeIndicesThisGame[Random.Range(0, _shapeIndicesThisGame.Count)];
    PickNewDesiredItem(forPlayer, desiredItem);
  }

  private void PickNewDesiredItem(PlayerController forPlayer, Item.ItemDefinition desiredItem)
  {
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