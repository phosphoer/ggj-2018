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

  private int _mistakeCount;
  private int _correctCount;
  private Dictionary<PlayerController, Item.ItemDefinition> _desiredItems;
  private float _eyeBlinkTimer;
  private Vector3 _eyeOriginalScale;
  private float _eyeBlinkScale = 1.0f;
  private float _eyeOpenScale = 1.0f;
  private bool _isOpen;

  private void Awake()
  {
    _desiredItems = new Dictionary<PlayerController, Item.ItemDefinition>();
    _eyeOriginalScale = _eyesTransform.localScale;
    _interactable.PromptShown += OnPromptShown;
    _interactable.PromptHidden += OnPromptHidden;
    PlayerController.Spawned += OnPlayerSpawned;

    foreach (GameObject victoryTorch in _victoryTorches)
    {
      victoryTorch.transform.localScale = Vector3.zero;
    }
  }

  private void Update()
  {
    _eyeBlinkTimer -= Time.deltaTime;
    if (_eyeBlinkTimer < 0)
    {
      _eyeBlinkTimer = Random.Range(_eyeBlinkIntervalMin, _eyeBlinkIntervalMax);
      StartCoroutine(EyeBlinkRoutine());
    }

    _eyeOpenScale = Mathfx.Damp(_eyeOpenScale, IsOpen ? 1.5f : 1.0f, 0.5f, Time.deltaTime * 5.0f);

    Vector3 scale = _eyesTransform.localScale;
    scale.x = _eyeOriginalScale.x * _eyeBlinkScale * _eyeOpenScale;
    scale.z = _eyeOriginalScale.z * _eyeOpenScale;
    _eyesTransform.localScale = scale;

    for (int i = 0; i < _victoryTorches.Length; ++i)
    {
      GameObject victoryTorch = _victoryTorches[i];
      bool isLit = _correctCount > i;
      victoryTorch.transform.localScale = Mathfx.Damp(victoryTorch.transform.localScale, isLit ? Vector3.one : Vector3.zero, 0.5f, Time.deltaTime);
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
    }

    CheckWinLose();
  }

  private void OnPromptShown()
  {
    IsOpen = true;
  }

  private void OnPromptHidden()
  {
    IsOpen = false;
  }

  private void OnPlayerSpawned(PlayerController playerController)
  {
    PickNewDesiredItem(playerController);
  }

  private void CorrectChoiceMade(PlayerController byPlayer)
  {
    PickNewDesiredItem(byPlayer);
    ++_correctCount;
  }

  private void IncorrectChoiceMade(PlayerController byPlayer)
  {
    ++_mistakeCount;

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
  }
}