using UnityEngine;

public class GroundItem : MonoBehaviour, IInteractable
{
    public ItemData itemData;
    public int quantity = 1;

    [SerializeField] private WorldInteractionPrompt worldPrompt;

    public string InteractionPrompt
    {
        get
        {
            if (itemData == null)
                return "Pick up item";

            return $"Pick up {itemData.itemName}";
        }
    }

    private void Awake()
    {
        if (worldPrompt == null)
            worldPrompt = GetComponentInChildren<WorldInteractionPrompt>(true);
    }

    public void ShowPrompt()
    {
        if (worldPrompt != null)
            worldPrompt.Show($"[E] {InteractionPrompt}");
    }

    public void HidePrompt()
    {
        if (worldPrompt != null)
            worldPrompt.Hide();
    }

    public bool Interact(Interactor interactor)
    {
        Inventory inventory = interactor.GetComponentInParent<Inventory>();

        if (inventory == null)
        {
            Debug.LogWarning("No Inventory found on player.");
            return false;
        }

        ItemInstance item = new ItemInstance(itemData, quantity);

        bool added = inventory.AddItem(item);

        if (added)
        {
            Destroy(gameObject);
            return true;
        }

        Debug.Log("Inventory full.");
        return false;
    }
}