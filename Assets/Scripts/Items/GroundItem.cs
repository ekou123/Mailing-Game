using UnityEngine;

public class GroundItem : MonoBehaviour, IInteractable
{
    public ItemData itemData;
    public int quantity = 1;

    public string InteractionPrompt => itemData != null ? $"Pick up {itemData.itemName}" : "Pick up item";

    public bool Interact(Interactor interactor)
    {
        Debug.Log("Interacting with ground object");
        if (itemData == null)
        {
            Debug.LogWarning("GroundItem has no ItemData assigned.");
            return false;
        }

        Inventory inventory = interactor.GetComponent<Inventory>();
        if (inventory != null)
        {
            Debug.Log("Adding " + itemData.name + " to your inventory");
            bool added = inventory.AddItem(itemData);
            if (added)
            {
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