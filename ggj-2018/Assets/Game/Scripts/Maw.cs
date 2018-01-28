using UnityEngine;
using System.Collections.Generic;

public class Maw : MonoBehaviour
{
  [SerializeField]
  private Item _itemPrefab = null;

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
        PickNewDesiredItem(item.OwnedByPlayer);
      }
      // Otherwise we should get mad 
      else
      {

      }
    }
  }

  private void OnPlayerSpawned(PlayerController playerController)
  {
    PickNewDesiredItem(playerController);
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

  private void PickNewDesiredItem(PlayerController forPlayer)
  {
    Item.ItemDefinition desiredItem;
    desiredItem.FaceIndex = Random.Range(0, _itemPrefab.TypeCount);
    desiredItem.ShapeIndex = Random.Range(0, _itemPrefab.TypeCount);

    _desiredItems[forPlayer] = desiredItem;
  }
}