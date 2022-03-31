using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : Item
{
    public Ability taughtAbility;

    public override void EquipItem(Character equipper)
    {
        equipper.spellList.Add(taughtAbility);
        equipper.SortSpells(); // Sort their spell list

        // Consume the object upon use
        FindObjectOfType<BagWindow>(true).allStoredItems.Remove(this);

        Destroy(gameObject);

        // Refresh inventory
        FindObjectOfType<BagWindow>(true).OpenInventory();
    }

    // Not used by Rings, as they're consumed upon use
    public override void UnequipItem(Character equipper)
    {

    }
}
