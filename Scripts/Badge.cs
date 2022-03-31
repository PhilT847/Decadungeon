using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Badge : Item
{
    public int hpBonus;
    public int mpBonus;
    public int strengthBonus;
    public int physicalDefenseBonus;
    public int magicalDefenseBonus;

    public bool regenHP;
    public bool regenMP;

    public string addedResistance;

    // The Hero's Medal boosts all stat growths by 20%.
    public bool increasesGrowths;

    public override void EquipItem(Character equipper)
    {
        //if the unit already has a badge equipped, remove the equipped badge's stats before adding new ones.
        if (equipper.equippedBadge != null)
        {
            equipper.maxHP -= equipper.equippedBadge.hpBonus;

            if (equipper.chosenClass.className != "Monk")
            {
                equipper.maxMP -= equipper.equippedBadge.mpBonus;
            }

            equipper.strength -= equipper.equippedBadge.strengthBonus;

            equipper.physicalDefense -= equipper.equippedBadge.physicalDefenseBonus;

            equipper.magicalDefense -= equipper.equippedBadge.magicalDefenseBonus;

            if (equipper.resistancesList.Contains(equipper.equippedBadge.addedResistance))
            {
                equipper.resistancesList.Remove(equipper.equippedBadge.addedResistance);
            }

            if (equipper.equippedBadge.increasesGrowths)
            {
                equipper.hpGrowth -= 20;
                equipper.strGrowth -= 20;
                equipper.magGrowth -= 20;
                equipper.dexGrowth -= 20;
                equipper.fthGrowth -= 20;
            }
        }

        itemOwner = equipper;

        //Add stats.
        equipper.maxHP += hpBonus;

        if (equipper.currentHP > equipper.maxHP)
        {
            equipper.currentHP = equipper.maxHP;
        }

        if (equipper.chosenClass.className != "Monk")
        {
            equipper.maxMP += mpBonus;
        }

        if (equipper.currentMP > equipper.maxMP)
        {
            equipper.currentMP = equipper.maxMP;
        }

        equipper.strength += strengthBonus;

        equipper.physicalDefense += physicalDefenseBonus;

        equipper.magicalDefense += magicalDefenseBonus;

        equipper.resistancesList.Add(addedResistance);

        // The Hero Medal increases growth rates
        if (increasesGrowths)
        {
            equipper.hpGrowth += 20;
            equipper.strGrowth += 20;
            equipper.magGrowth += 20;
            equipper.dexGrowth += 20;
            equipper.fthGrowth += 20;
        }

        equipper.chosenClass.badgeSprite.sprite = itemDisplayIcon;

        equipper.equippedBadge = this;
    }

    // Remove a badge from a unit. Used when a hero gives their badge to another hero, leaving them badge-less
    public override void UnequipItem(Character equipper)
    {
        itemOwner = null; // Remove ownership

        equipper.maxHP -= equipper.equippedBadge.hpBonus;

        if(equipper.currentHP > equipper.maxHP)
        {
            equipper.currentHP = equipper.maxHP;
        }

        if (equipper.chosenClass.className != "Monk")
        {
            equipper.maxMP -= equipper.equippedBadge.mpBonus;

            if (equipper.currentMP > equipper.maxMP)
            {
                equipper.currentMP = equipper.maxMP;
            }
        }

        equipper.strength -= equipper.equippedBadge.strengthBonus;

        equipper.physicalDefense -= equipper.equippedBadge.physicalDefenseBonus;

        equipper.magicalDefense -= equipper.equippedBadge.magicalDefenseBonus;

        if (equipper.resistancesList.Contains(equipper.equippedBadge.addedResistance))
        {
            equipper.resistancesList.Remove(equipper.equippedBadge.addedResistance);
        }

        if (equipper.equippedBadge.increasesGrowths)
        {
            equipper.hpGrowth -= 20;
            equipper.strGrowth -= 20;
            equipper.magGrowth -= 20;
            equipper.dexGrowth -= 20;
            equipper.fthGrowth -= 20;
        }

        // After removing stats, remove the badge itself from the character
        equipper.equippedBadge = null;
        equipper.chosenClass.badgeSprite.sprite = null;
    }
}
