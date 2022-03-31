using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    public Item heldItem;

    public Image itemBorder;
    public Image itemImage;
    public Image equipped_E; // The "E" that appears if an item is equipped

    public void UpdateInventorySpace(Item newItem)
    {
        // If there's a held item, make the button interactable and set it to that item. Otherwise, it's empty
        if (newItem != null)
        {
            itemBorder.GetComponent<Button>().interactable = true;
            SetToItem(newItem);
        }
        else
        {
            itemBorder.GetComponent<Button>().interactable = false;
            EmptySpace();
        }
    }

    // Turns this part into an empty space if there's no item here
    public void EmptySpace()
    {
        heldItem = null;

        itemBorder.color = new Color32(200, 200, 200, 200);
        itemImage.color = Color.clear;
    }

    public void SetToItem(Item thisItem)
    {
        heldItem = thisItem;

        itemImage.color = Color.white;
        itemImage.sprite = heldItem.itemDisplayIcon;

        itemBorder.color = new Color32(200, 200, 200, 255);
    }

    // Sets the "E" based on who- if anyone- equipped this item
    public void SetEquippedStatus()
    {
        // If this slot has both an item AND it's owned by a character, the item's owner and the "E" are set to match the character
        if(heldItem != null && heldItem.itemOwner != null)
        {
            equipped_E.color = heldItem.itemOwner.primaryColor;
        }
        else
        {
            equipped_E.color = Color.clear;
        }
    }
}
