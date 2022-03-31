using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public string itemName;
    public string itemDescription;
    public int itemCode; // Two-digit code used when saving/loading a file. See Savefile class
    public Sprite itemImage;
    public Sprite itemDisplayIcon; // A different image used for display in the inventory

    // Used to give ownership in the Bag menu, allowing characters to swap items
    public Character itemOwner;

    public abstract void EquipItem(Character equipper);

    public abstract void UnequipItem(Character equipper);
}
