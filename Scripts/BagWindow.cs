using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BagWindow : MonoBehaviour
{
    // Displays hero buttons when an item is selected
    public GameObject giveItemTo;

    public List<Item> allHeldItems; // Items currently equipped by units
    public List<Item> allStoredItems; // Items stored in the Bag
    public List<Item> allItems; // The combination of the above two lists, used to display the inventory

    public InventoryItem[] allInventorySpaces;
    public Item selectedItem;

    // Related to the item description at the bottom
    public Image itemIcon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDescription;

    public Button equipButton;
    public Button dropButton;
    public Button cancelEquipButton;

    public Image goldenBorder; //the gold border that goes around the selected item

    public GameObject[] DEBUG_ITEMS_LIST;

    public void OpenInventory()
    {
        // Refresh the list of held unit items
        allHeldItems = AddItemsFromUnits();

        allItems.Clear();
        allItems.AddRange(allHeldItems);
        allItems.AddRange(allStoredItems);

        selectedItem = null;
        giveItemTo.SetActive(false);

        equipButton.gameObject.SetActive(false);
        dropButton.gameObject.SetActive(false);
        cancelEquipButton.gameObject.SetActive(false);

        //Set the item desc area to empty
        itemIcon.color = Color.clear;
        itemName.SetText("");
        itemDescription.SetText("    Select an item.");

        // Go through each held/stored item and update each space accordingly
        for (int i = 0; i < 12; i++)
        {
            if (i < allItems.Count)
            {
                allInventorySpaces[i].UpdateInventorySpace(allItems[i]);
            }
            else
            {
                allInventorySpaces[i].UpdateInventorySpace(null);
            }
        }

        // Ensure that the "E"'s are all transparent or colored to their owner
        for (int i = 0; i < allInventorySpaces.Length; i++)
        {
            allInventorySpaces[i].SetEquippedStatus();
        }

        // Nothing's selected, so the border becomes clear
        goldenBorder.color = Color.clear;
    }

    List<Item> AddItemsFromUnits()
    {
        List<Item> allUnitItems = new List<Item>();

        Character[] allCharacters = FindObjectOfType<GameController>().allCharacters;

        // Add each item equipped by characters
        for (int i = 0; i < allCharacters.Length; i++)
        {
            if(allCharacters[i].chosenClass.className != "Monk")
            {
                allUnitItems.Add(allCharacters[i].equippedWeapon);
            }

            if(allCharacters[i].equippedBadge != null)
            {
                allUnitItems.Add(allCharacters[i].equippedBadge);
            }
        }

        return allUnitItems;
    }

    public void SelectItem(int index)
    {
        FindObjectOfType<AudioManager>().Play("select");

        goldenBorder.color = Color.white;
        goldenBorder.transform.localPosition = allInventorySpaces[index].transform.localPosition;

        selectedItem = allInventorySpaces[index].heldItem;

        itemIcon.color = Color.white;
        itemIcon.sprite = selectedItem.itemDisplayIcon;
        itemName.SetText(selectedItem.itemName);
        itemDescription.SetText(selectedItem.itemDescription);

        // Items cannot be equipped/dropped during combat
        equipButton.gameObject.SetActive(!FindObjectOfType<GameController>().battleScreen.gameObject.activeSelf);
        // You can't drop an equipped item
        dropButton.gameObject.SetActive(!FindObjectOfType<GameController>().battleScreen.gameObject.activeSelf && selectedItem.itemOwner == null);

        // If it's a ring, then it's a one-time use and changes the equip button to "Teach"
        if (selectedItem.GetComponent<Ring>())
        {
            equipButton.GetComponentInChildren<Image>().color = new Color32(160, 50, 160, 255);
            equipButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Teach");
        }
        else // Otherwise, it's just "Equip"
        {
            equipButton.GetComponentInChildren<Image>().color = new Color32(80,125,80,255);
            equipButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Equip");
        }
    }

    public void ChooseEquipButton()
    {
        giveItemTo.SetActive(true);
        cancelEquipButton.gameObject.SetActive(true);

        equipButton.gameObject.SetActive(false);
        dropButton.gameObject.SetActive(false);
    }

    public void CancelEquip()
    {
        giveItemTo.SetActive(false);
        cancelEquipButton.gameObject.SetActive(false);

        equipButton.gameObject.SetActive(true);
        dropButton.gameObject.SetActive(true);
    }

    public void GiveItemTo(int heroIndex)
    {
        FindObjectOfType<AudioManager>().Play("select");

        Character equipper = FindObjectOfType<GameController>().allCharacters[heroIndex];
        Item replacedItem = null;

        if (((selectedItem.GetComponent<Weapon>() || selectedItem.GetComponent<Ring>()) && equipper.chosenClass.className == "Monk")
            || (selectedItem.GetComponent<Ring>() && equipper.spellList.Contains(selectedItem.GetComponent<Ring>().taughtAbility)))
        {
            // Two errors; 1) Monks can't equip weapons/rings, and 2) units that already know a spell can't learn it again through a Ring (though they can learn it early)
            return;
        }

        // Check whether the unit is replacing a weapon/badge to equip this item
        if (selectedItem.GetComponent<Weapon>())
        {
            replacedItem = equipper.equippedWeapon;
        }
        else if (selectedItem.GetComponent<Badge>() && equipper.equippedBadge != null)
        {
            replacedItem = equipper.equippedBadge;
        }

        // If the item already had an owner, then swap what you currently have with them. Note that weapons are ALWAYS swapped bt non-Monk heroes as everyone has one
        if(selectedItem.itemOwner != null)
        {
            if(replacedItem != null)
            {
                replacedItem.EquipItem(selectedItem.itemOwner);
            }
            else // If there's no item to swap, then it must be a badge as heroes don't start with or require one. Unequip the owner's badge
            {
                selectedItem.GetComponent<Badge>().UnequipItem(selectedItem.itemOwner);
            }
        }
        else if (replacedItem != null) // If the replacement item isn't being given to a new owner, then it's added to storage
        {
            allStoredItems.Add(replacedItem);
            replacedItem.itemOwner = null; // Remove ownership of the removed weapon
        }

        //PROBLEM: IT'S UNEQUIPPING THE CURRENT WEAPON FROM THE OLD USER
        selectedItem.EquipItem(equipper);

        // Remove this item from storage if it's equipped from storage
        if (allStoredItems.Contains(selectedItem))
        {
            allStoredItems.Remove(selectedItem);
        }

        // Reset the item menu, also updating each item in the list
        OpenInventory();
    }

    public void DropItem()
    {
        FindObjectOfType<AudioManager>().Play("trash");

        // Remove the object and destroy it... since equipped items can't be dropped, it's always a stored item
        allStoredItems.Remove(selectedItem);

        Destroy(selectedItem.gameObject);

        // Refresh the menu
        OpenInventory();
    }

    public void AddItemToStorage(Item newItem)
    {
        allStoredItems.Add(newItem);
    }
}
