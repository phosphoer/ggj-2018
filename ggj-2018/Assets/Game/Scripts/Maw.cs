using UnityEngine;
using System.Collections.Generic;

public class Maw : MonoBehaviour
{
  [SerializeField]
  private Item _itemPrefab = null;

  [SerializeField]
  private int _losingMistakeCount = 5;

  [SerializeField]
  private int _winningCorrectCount = 5;

  private int _mistakeCount;
  private int _correctCount;
  private Dictionary<PlayerController, Item.ItemDefinition> _desiredItems;

  private void Awake()
  {
    _desiredItems = new Dictionary<PlayerController, Item.ItemDefinition>();
    PlayerController.Spawned += OnPlayerSpawned;
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