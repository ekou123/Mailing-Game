using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData containedItem;
    public string InteractionPrompt => "Open Chest";

    public bool Interact(Interactor interactor)
    {
        var inventory = interactor.GetComponentInParent<Inventory>();
        if (inventory != null && containedItem != null)
            inventory.AddItem(new ItemInstance(containedItem));
        return true;
    }
}

