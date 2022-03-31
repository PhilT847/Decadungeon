using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TreasureRoom : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescText;
    public Image itemImage;

    public Ring grantedRing;
    public Weapon grantedWeapon;
    public Badge grantedBadge;

    public List<Item> commonLootPool;
    public List<Item> rareLootPool;
    public List<Item> legendaryLootPool;

    public List<Item> DEBUG_ITEMS;

    public GameObject inventoryFullPanel;
    public List<Item> storedItems;
    public Button[] storedItemButtons;
    public Item itemSelectedForDrop;
    public Image selectedBorder;
    public Button dropItemButton;

    public void OpenTreasureRoom(int floor)
    {
        // Reset the items so that only one is granted
        grantedWeapon = null;
        grantedBadge = null;
        grantedRing = null;

        FindObjectOfType<GameController>().DisableJoystick(true);
        gameObject.SetActive(true);
        inventoryFullPanel.SetActive(false); //close the "Full Inventory" panel and set no items for dropping
        itemSelectedForDrop = null;
        // Nothing's selected, so the selected drop item border becomes clear
        selectedBorder.color = Color.clear;

        // Update the inventory so the Treasure Room "knows" how large the inventory is... allows for checking if it's full
        FindObjectOfType<BagWindow>(true).OpenInventory();

        SetTreasureRoom(floor);

        // Animate
        GetComponent<Animator>().SetTrigger("TreasureOpen");

        // Animate Myrrh
        FindObjectOfType<Myrrh>().GetComponent<Animator>().SetTrigger("MyrrhExit");
    }

    void SetTreasureRoom(int floor)
    {
        // If there are any items in need of debugging, give one automatically and return
        if (DEBUG_ITEMS.Count > 0)
        {
            DEBUG_GrantItem();
            return;
        }

        if(floor < 3) //floors 1-2 only have low-tier items/spells. 80/20/0
        {
            int itemRoll = Random.Range(0, 5);

            if(itemRoll != 4)
            {
                GrantCommonItem();
            }
            else
            {
                GrantRareItem();
            }
        }
        else if(floor < 7) //floors 3-6 carry medium-tier items/spells. 10/80/10
        {
            int itemRoll = Random.Range(0, 10);

            if(itemRoll == 0)
            {
                GrantCommonItem();
            }
            else if (itemRoll < 9)
            {
                GrantRareItem();
            }
            else
            {
                GrantLegendaryItem();
            }
        }
        else //floors 7-10 carry high-tier items. 0/33/67
        {
            int itemRoll = Random.Range(0, 3);

            if (itemRoll == 2)
            {
                GrantRareItem();
            }
            else
            {
                GrantLegendaryItem();
            }
        }

        // Set the image/description

        if(grantedWeapon != null)
        {
            itemNameText.SetText(grantedWeapon.itemName);
            itemDescText.SetText(grantedWeapon.itemDescription);
            itemImage.sprite = grantedWeapon.itemDisplayIcon;
        }
        else if(grantedBadge != null)
        {
            itemNameText.SetText(grantedBadge.itemName);
            itemDescText.SetText(grantedBadge.itemDescription);
            itemImage.sprite = grantedBadge.itemDisplayIcon;
        }
        else
        {
            itemNameText.SetText(grantedRing.itemName);
            itemDescText.SetText(grantedRing.itemDescription);
            itemImage.sprite = grantedRing.itemDisplayIcon;
        }
    }

    void DEBUG_GrantItem()
    {
        Item chosenItem = DEBUG_ITEMS[Random.Range(0, DEBUG_ITEMS.Count)];

        if (chosenItem.GetComponent<Weapon>())
        {
            grantedWeapon = chosenItem.GetComponent<Weapon>();
        }
        else if (chosenItem.GetComponent<Badge>())
        {
            grantedBadge = chosenItem.GetComponent<Badge>();
        }
        else
        {
            grantedRing = chosenItem.GetComponent<Ring>();
        }

        if (grantedWeapon != null)
        {
            itemNameText.SetText(grantedWeapon.itemName);
            itemDescText.SetText(grantedWeapon.itemDescription);
            itemImage.sprite = grantedWeapon.itemDisplayIcon;
        }
        else if (grantedBadge != null)
        {
            itemNameText.SetText(grantedBadge.itemName);
            itemDescText.SetText(grantedBadge.itemDescription);
            itemImage.sprite = grantedBadge.itemDisplayIcon;
        }
        else
        {
            itemNameText.SetText(grantedRing.itemName);
            itemDescText.SetText(grantedRing.itemDescription);
            itemImage.sprite = grantedRing.itemDisplayIcon;
        }
    }

    void GrantCommonItem()
    {
        if(commonLootPool.Count > 0)
        {
            Item chosenItem = commonLootPool[Random.Range(0, commonLootPool.Count)];

            if (chosenItem.GetComponent<Weapon>())
            {
                grantedWeapon = chosenItem.GetComponent<Weapon>();
            }
            else if (chosenItem.GetComponent<Badge>())
            {
                grantedBadge = chosenItem.GetComponent<Badge>();
            }
            else
            {
                grantedRing = chosenItem.GetComponent<Ring>();
            }

            commonLootPool.Remove(chosenItem);
        }
        else
        {
            GrantRareItem();
        }
    }

    void GrantRareItem()
    {
        if(rareLootPool.Count > 0)
        {
            Item chosenItem = rareLootPool[Random.Range(0, rareLootPool.Count)];

            if (chosenItem.GetComponent<Weapon>())
            {
                grantedWeapon = chosenItem.GetComponent<Weapon>();
            }
            else if (chosenItem.GetComponent<Badge>())
            {
                grantedBadge = chosenItem.GetComponent<Badge>();
            }
            else
            {
                grantedRing = chosenItem.GetComponent<Ring>();
            }

            rareLootPool.Remove(chosenItem);
        }
        else
        {
            GrantLegendaryItem();
        }
    }

    void GrantLegendaryItem()
    {
        if(legendaryLootPool.Count > 0)
        {
            Item chosenItem = legendaryLootPool[Random.Range(0, legendaryLootPool.Count)];

            if (chosenItem.GetComponent<Weapon>())
            {
                grantedWeapon = chosenItem.GetComponent<Weapon>();
            }
            else if (chosenItem.GetComponent<Badge>())
            {
                grantedBadge = chosenItem.GetComponent<Badge>();
            }
            else
            {
                grantedRing = chosenItem.GetComponent<Ring>();
            }

            legendaryLootPool.Remove(chosenItem);
        }
        else
        {
            CloseTreasureMenu();
        }
    }

    public void SetDescription(Item thisItem)
    {
        itemNameText.SetText(thisItem.itemName);
        itemDescText.SetText(thisItem.itemDescription);

        itemImage.sprite = thisItem.itemDisplayIcon;
    }

    public void OpenFullInventoryPanel()
    {
        inventoryFullPanel.SetActive(true);
        itemSelectedForDrop = null;

        dropItemButton.interactable = false;

        UpdateStoredInventory();
    }

    public void UpdateStoredInventory()
    {
        // Add all stored items to the list, as they can be dropped
        List<Item> droppableItems = new List<Item>();
        droppableItems.AddRange(FindObjectOfType<BagWindow>(true).allStoredItems);

        // Do NOT include equipped weapons, but include equipped badges
        foreach(Item heldItem in FindObjectOfType<BagWindow>(true).allHeldItems)
        {
            if (!heldItem.GetComponent<Weapon>())
            {
                droppableItems.Add(heldItem);
            }
        }

        storedItems = droppableItems;

        // Clear the inventory buttons, then add items as needed
        for(int i = 0; i < storedItemButtons.Length; i++)
        {
            storedItemButtons[i].interactable = false;
            storedItemButtons[i].GetComponent<Image>().color = Color.clear;
            storedItemButtons[i].transform.GetChild(0).GetComponent<Image>().color = Color.clear;
        }

        for(int i = 0; i < storedItems.Count; i++)
        {
            storedItemButtons[i].interactable = true;
            storedItemButtons[i].GetComponent<Image>().color = Color.white;
            storedItemButtons[i].transform.GetChild(0).GetComponent<Image>().color = Color.white;
            storedItemButtons[i].transform.GetChild(0).GetComponent<Image>().sprite = storedItems[i].itemDisplayIcon;
        }
    }

    public void SelectItemForDrop(int itemIndex)
    {
        itemSelectedForDrop = storedItems[itemIndex];
        selectedBorder.color = Color.white;
        selectedBorder.transform.localPosition = storedItemButtons[itemIndex].transform.localPosition;

        // You can now drop an item
        dropItemButton.interactable = true;
    }

    public void CancelDroppingItem()
    {
        inventoryFullPanel.SetActive(false); //close the "Full Inventory" panel and set no items for dropping
        itemSelectedForDrop = null;
        // Nothing's selected, so the selected drop item border becomes clear
        selectedBorder.color = Color.clear;
    }

    public void TakeTreasure()
    {
        //If there was an item selected for dropping, then remove it from the inventory (and unequip if the dropped item was a held badge)
        if(itemSelectedForDrop != null)
        {
            // If the dropped item is a held badge, unequip the badge and remove from the list of held items
            if(itemSelectedForDrop.GetComponent<Badge>() && itemSelectedForDrop.itemOwner != null)
            {
                itemSelectedForDrop.GetComponent<Badge>().UnequipItem(itemSelectedForDrop.itemOwner);

                FindObjectOfType<BagWindow>(true).allHeldItems.Remove(itemSelectedForDrop);
            }
            else // Otherwise, it's a stored item
            {
                FindObjectOfType<BagWindow>(true).allStoredItems.Remove(itemSelectedForDrop);
            }

            Destroy(itemSelectedForDrop.gameObject);
        }

        // If storage is at maximum, ask to swap an item
        if (FindObjectOfType<BagWindow>(true).allHeldItems.Count + FindObjectOfType<BagWindow>(true).allStoredItems.Count < 12)
        {
            if (grantedRing != null) // Give a ring.
            {
                var newRing = Instantiate(grantedRing, GameObject.FindGameObjectWithTag("ItemContainer").transform);

                FindObjectOfType<BagWindow>(true).AddItemToStorage(newRing.GetComponent<Item>());

                // Add the code to the running lootCode for removal when loading the game
                FindObjectOfType<Savefile>().AddToLootCode(grantedRing.itemCode);
            }
            if (grantedWeapon != null) // Give a weapon.
            {
                var newWeapon = Instantiate(grantedWeapon, GameObject.FindGameObjectWithTag("ItemContainer").transform);

                FindObjectOfType<BagWindow>(true).AddItemToStorage(newWeapon.GetComponent<Item>());

                // Add the code to the running lootCode for removal when loading the game
                FindObjectOfType<Savefile>().AddToLootCode(grantedWeapon.itemCode);
            }
            else if (grantedBadge != null) // Give a badge.
            {
                var newBadge = Instantiate(grantedBadge, GameObject.FindGameObjectWithTag("ItemContainer").transform);

                FindObjectOfType<BagWindow>(true).AddItemToStorage(newBadge.GetComponent<Item>());

                // Add the code to the running lootCode for removal when loading the game
                FindObjectOfType<Savefile>().AddToLootCode(grantedBadge.itemCode);
            }

            CloseTreasureMenu();
        }
        else // If inventory is full, prompt the player to swap an item out
        {
            OpenFullInventoryPanel();
        }
    }

    public void CloseTreasureMenu()
    {
        // Animate Myrrh
        FindObjectOfType<Myrrh>().GetComponent<Animator>().SetTrigger("MyrrhEnter");

        //clears out the treasure room, turning it into a normal room.
        FindObjectOfType<FloorBuilder>().ClearRoom(FindObjectOfType<FieldCharacter>().currentRoom);

        // Now that the treasure room is removed and the item's taken/dropped, save the game
        FindObjectOfType<Savefile>().CreateSaveData();

        FindObjectOfType<GameController>().EnableJoystick(true);
        gameObject.SetActive(false);
    }
}
