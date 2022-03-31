using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavedHeroLoader : MonoBehaviour
{
    public void LoadStats(Character c, int heroIndex)
    {
        string loadedCode = PlayerPrefs.GetString("SaveCode");

        // Begin reading through the code at an index that matches with the hero's "piece" of the save code
        int startIndex = 2 + (72 * heroIndex);

        c.level = GetIntegerValueFrom('N', 'N', loadedCode[startIndex + 1], loadedCode[startIndex + 2]);
        c.exp = GetIntegerValueFrom(loadedCode[startIndex + 7], loadedCode[startIndex + 8], loadedCode[startIndex + 9], loadedCode[startIndex + 10]);

        // Calculate exp needed for next level
        int expNeeded = 100;

        for (int i = 1; i < c.level; i++)
        {
            expNeeded = (int)(expNeeded * 1.1f);
        }

        c.expToNextLevel = expNeeded;

        c.maxHP = GetIntegerValueFrom('N', loadedCode[startIndex + 12], loadedCode[startIndex + 13], loadedCode[startIndex + 14]);
        c.currentHP = GetIntegerValueFrom('N', loadedCode[startIndex + 15], loadedCode[startIndex + 16], loadedCode[startIndex + 17]);
        c.currentMP = GetIntegerValueFrom('N', 'N', loadedCode[startIndex + 18], loadedCode[startIndex + 19]);

        c.strength = GetIntegerValueFrom('N', 'N', loadedCode[startIndex + 21], loadedCode[startIndex + 22]); 
        c.magic = GetIntegerValueFrom('N', 'N', loadedCode[startIndex + 24], loadedCode[startIndex + 25]);
        c.dexterity = GetIntegerValueFrom('N', 'N', loadedCode[startIndex + 27], loadedCode[startIndex + 28]); 
        c.faith = GetIntegerValueFrom('N', 'N', loadedCode[startIndex + 30], loadedCode[startIndex + 31]); 

        // Give Knights and Liches their passive armor before adding equipment
        if (c.chosenClass.className == "Knight")
        {
            c.physicalDefense = 1 + (c.level / 2);
        }

        // Learn skills, then add additional skills from the save code (from rings or Mime)
        c.chosenClass.CheckForLearnedSkills(c.level);

        for (int i = startIndex + 32; i < startIndex + 72; i += 2)
        {
            int chosenAbilityInt = GetIntegerValueFrom('N', 'N', loadedCode[i], loadedCode[i + 1]);

            // Attempt to add any ability that isn't ID = "00"
            if(chosenAbilityInt > 0)
            {
                Ability attemptToAddAbility = FindObjectOfType<Savefile>().FindAbilityWithID(chosenAbilityInt);

                // Create a new spell if adding Wild Magic (from enemies)
                if (attemptToAddAbility != null && attemptToAddAbility.GetComponent<EnemySpecialAttack>())
                {
                    var newSpell = Instantiate(FindObjectOfType<Savefile>().wildMagicPrefab, c.transform);

                    //convert the ability into Wild Magic, then teach it to the unit.
                    newSpell.GetComponent<WildMagic>().ConvertToWildMagic(attemptToAddAbility.GetComponent<EnemySpecialAttack>());

                    attemptToAddAbility = newSpell.GetComponent<WildMagic>();
                }

                // If they don't already know the spell, add it to their list
                if (attemptToAddAbility != null && !c.spellList.Contains(attemptToAddAbility))
                {
                    c.spellList.Add(attemptToAddAbility.GetComponent<Spell>());
                }
            }
        }

        // End by sorting their spell list
        c.SortSpells();
    }

    // Get a value for EXP, HP, a stat, etc... with the necessary amount of digits. Use 'N' for null values when certain places aren't needed
    public int GetIntegerValueFrom(char thousandsPlace, char hundredsPlace, char tensPlace, char onesPlace)
    {
        if(thousandsPlace != 'N')
        {
            return ((thousandsPlace - '0') * 1000) + ((hundredsPlace - '0') * 100) + ((tensPlace - '0') * 10) + (onesPlace - '0');
        }
        else if(hundredsPlace != 'N')
        {
            return ((hundredsPlace - '0') * 100) + ((tensPlace - '0') * 10) + (onesPlace - '0');
        }
        else if(tensPlace != 'N')
        {
            return ((tensPlace - '0') * 10) + (onesPlace - '0');
        }
        else
        {
            return onesPlace - '0';
        }
    }

    public void LoadInventory()
    {
        string loadedCode = PlayerPrefs.GetString("SaveCode");

        // Go through each character's inventory and equip their weapon/badge (if applicable)
        for (int i = 291; i < 307; i += 4)
        {
            Character chosenCharacter = FindObjectOfType<GameController>().allCharacters[(i - 291) / 4];

            int chosenWeaponInt = GetIntegerValueFrom('N','N', loadedCode[i], loadedCode[i + 1]);
            int chosenBadgeInt = GetIntegerValueFrom('N', 'N', loadedCode[i + 2], loadedCode[i + 3]);

            // Unequip/destroy their starting weapon and add the new one. Note that MP/HP may decrease upon unequipping, so save these values until re-equipping
            int hpBeforeSwap = chosenCharacter.currentHP;
            int mpBeforeSwap = chosenCharacter.currentMP;

            Weapon originalStartingWeapon = chosenCharacter.equippedWeapon;
            originalStartingWeapon.UnequipItem(chosenCharacter);
            Destroy(originalStartingWeapon.gameObject);

            var newStartingWeapon = Instantiate(FindObjectOfType<Savefile>().FindItemWithID(chosenWeaponInt), GameObject.FindGameObjectWithTag("ItemContainer").transform);

            newStartingWeapon.GetComponent<Weapon>().EquipItem(chosenCharacter);

            // Forcibly return HP/MP values as needed
            chosenCharacter.currentHP = hpBeforeSwap;

            if(chosenCharacter.currentHP > chosenCharacter.maxHP)
            {
                chosenCharacter.currentHP = chosenCharacter.maxHP;
            }

            chosenCharacter.currentMP = mpBeforeSwap;

            if (chosenCharacter.currentMP > chosenCharacter.maxMP)
            {
                chosenCharacter.currentMP = chosenCharacter.maxMP;
            }

            // Since this process involved plenty of weapon swapping, hard code max MP based on what they have equipped. The badge will do so automatically below
            if (chosenCharacter.chosenClass.className != "Monk")
            {
                chosenCharacter.maxMP = chosenCharacter.GetRawMagic() + chosenCharacter.equippedWeapon.bonusMP;
            }
            else
            {
                chosenCharacter.maxMP = 0;
            }

            // If a badge exists, create and equip it
            if (chosenBadgeInt != 0)
            {
                var newStartingBadge = Instantiate(FindObjectOfType<Savefile>().FindItemWithID(chosenBadgeInt), GameObject.FindGameObjectWithTag("ItemContainer").transform);

                newStartingBadge.GetComponent<Badge>().EquipItem(chosenCharacter);
            }
        }

        // Next, add all stored items to the inventory
        for (int i = 308; i < 332; i += 2)
        {
            int chosenItemInt = GetIntegerValueFrom('N', 'N', loadedCode[i], loadedCode[i + 1]);

            if (chosenItemInt != 0 && FindObjectOfType<Savefile>().FindItemWithID(chosenItemInt) != null)
            {
                var itemCopy = Instantiate(FindObjectOfType<Savefile>().FindItemWithID(chosenItemInt), GameObject.FindGameObjectWithTag("ItemContainer").transform);

                FindObjectOfType<BagWindow>(true).allStoredItems.Add(itemCopy.GetComponent<Item>());
            }
        }
    }

    // Set floor, boss, and myrrh count
    public void SetFloorAndBoss()
    {
        FindObjectOfType<Savefile>().forcedFloorNumber = GetIntegerValueFrom('N', 'N', FindObjectOfType<Savefile>().saveCode[0], FindObjectOfType<Savefile>().saveCode[1]);
        FindObjectOfType<Savefile>().allowableEncounters = GetIntegerValueFrom('N', 'N', 'N', FindObjectOfType<Savefile>().saveCode[332]);
        FindObjectOfType<Savefile>().allowableTreasureChests = GetIntegerValueFrom('N', 'N', 'N', FindObjectOfType<Savefile>().saveCode[333]);
        FindObjectOfType<Savefile>().allowableBossRooms = GetIntegerValueFrom('N', 'N', 'N', FindObjectOfType<Savefile>().saveCode[334]);
        FindObjectOfType<Savefile>().forcedBossIndex = GetIntegerValueFrom('N', 'N', 'N', FindObjectOfType<Savefile>().saveCode[335]);

        // Set myrrh
        FindObjectOfType<Myrrh>().ForcedSetMyrrh(GetIntegerValueFrom('N', 'N', 'N', FindObjectOfType<Savefile>().saveCode[336]));
    }
}
