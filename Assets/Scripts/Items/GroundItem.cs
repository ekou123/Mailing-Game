using UnityEngine;

public class GroundItem : MonoBehaviour, IInteractable
{
    public ItemData itemData;
    public int quantity = 1;

    private bool _consumed;

    public string InteractionPrompt => itemData != null ? $"Pick up {itemData.itemName}" : "Pick up item";

    public bool Interact(Interactor interactor)
    {
        if (_consumed) return false;
        Debug.Log("Interacting with ground object");
        if (itemData == null)
        {
            Debug.LogWarning("GroundItem has no ItemData assigned.");
            return false;
        }

        Inventory inventory = interactor.GetComponentInParent<Inventory>();
        if (inventory != null)
        {
            Debug.Log("Adding " + itemData.name + " to your inventory");
            bool added = inventory.AddItem(new ItemInstance(itemData, quantity));
            if (added)
            {
                _consumed = true;
                Destroy(gameObject);
                return true;
            }
            else
            {
                Debug.Log("Inventory full, cannot pick up item");
            }
        }

        return false;
    }
}