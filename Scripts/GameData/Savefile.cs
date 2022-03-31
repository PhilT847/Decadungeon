using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Savefile : MonoBehaviour
{
    // The combined "Save Code"
    public string saveCode;

    // The "Loot Code" contains all loot already grabbed by the player. Ensures that the loot pool isn't reset when reloading a game
    public string lootCode;

    public static Savefile instance;

    // Ensures that the GameController loads new data when starting the game
    public bool playingSavedGame;

    public List<Weapon> allWeapons;
    public List<Badge> allBadges;
    public List<Ring> allRings;
    public List<Ability> allAbilities; // All abilities learned through Mime or Rings
    public GameObject wildMagicPrefab; // Used to create Green Magic from the above list

    // Certain rooms only allowable in certain quantities. Removes encounter/treasure rooms when reloading. Also, you MUST fight the chosen boss
    public int forcedFloorNumber;
    public int allowableEncounters;
    public int allowableTreasureChests;
    public int forcedBossIndex;
    public int allowableBossRooms;

    /*
        The save code is as follows:
        Floor code
        (01 -> 10)

        Hero codes (4 total)
        Level (L + 01-99) [3 characters] 
        Class (C + 00-09) [3]
        Current EXP (E + 0000-9999) [4]
        Stats... HP (H + 000-999), STR (S + 00-99), MAG (M + 00-99), DEX (D + 00-99), FTH (F + 00-99) [16]
        Spell List... Since Mime/Rings can alter the list, it must be recorded. Two-digit code for each spell (see Ability). [40 digits total]

        NOTE: 00 is an empty badge. 50 is a fist weapon (counted as "empty" in inventory). 00-39 are badges. 40-89 are weapons. 90-99 are rings/other.
        NOTE: Find item codes in the itemCode string contained in each Item object. See ITEM DIRECTORY at the bottom of this code.

        Inventory code
        First, pull each item off each character (including non-equipped "empty" badges)
        Then add all non-equipped items (Same as Equipment codes... two digits each. 00 means there's no item in that slot)

        *EXAMPLE CODE*
        01
        L01C02E0360H055S12M08D08F100000000000000000000000000000000000000000
        L01C02E0360H055S12M08D08F100000000000000000000000000000000000000000
        L01C02E0360H055S12M08D08F100000000000000000000000000000000000000000
        L01C02E0360H055S12M08D08F100000000000000000000000000000000000000000
        030003000300030001021230
    */

    // Ensure that there's the same Savefile in each scene
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        if (PlayerPrefs.HasKey("SaveCode"))
        {
            saveCode = PlayerPrefs.GetString("SaveCode");
        }

        if (PlayerPrefs.HasKey("LootCode"))
        {
            lootCode = PlayerPrefs.GetString("LootCode");
        }
    }

    public void LoadSaveData()
    {

    }

    public void CreateSaveData()
    {
        string newSaveData = "";

        // Save floor number
        if(FindObjectOfType<FloorBuilder>().floorNumber > 9)
        {
            newSaveData += FindObjectOfType<FloorBuilder>().floorNumber;
        }
        else
        {
            newSaveData += "0" + FindObjectOfType<FloorBuilder>().floorNumber;
        }

        // Save all characters
        for (int i = 0; i < 4; i++)
        {
            newSaveData += CreateCharacterCode(FindObjectOfType<GameController>().allCharacters[i]);
        }

        // Save all equipment
        newSaveData += CreateInventoryCode();

        // Save the current boss (and if it's alive), amount of enemies killed, and amount of treasure taken so you can't repeat rooms
        newSaveData += FindObjectOfType<FloorBuilder>().CountRoomsWithEncounters();
        newSaveData += FindObjectOfType<FloorBuilder>().CountRoomsWithTreasure();
        newSaveData += FindObjectOfType<FloorBuilder>().BossOnFloor();
        newSaveData += FindObjectOfType<BattleController>(true).bossIndex;

        // Add current Myrrh count
        newSaveData += FindObjectOfType<Myrrh>().myrrhCount;

        saveCode = newSaveData;
        PlayerPrefs.SetString("SaveCode", saveCode);
    }

    string CreateCharacterCode(Character thisCharacter)
    {
        string newCode = "L";

        // Character level

        if (thisCharacter.level > 9)
        {
            newCode += thisCharacter.level;
        }
        else
        {
            newCode += "0" + thisCharacter.level;
        }

        // Character class (see Hero)

        newCode += "C";

        if(thisCharacter.chosenClass.classCode > 9)
        {
            newCode += thisCharacter.chosenClass.classCode;
        }
        else
        {
            newCode += "0" + thisCharacter.chosenClass.classCode;
        }

        // Character EXP

        newCode += "E";

        if (thisCharacter.exp > 999)
        {
            // Ensure that EXP can't go above 9999... this shouldn't happen but is a failsafe for code reading
            if(thisCharacter.exp > 9999)
            {
                thisCharacter.exp = 9999;
            }

            newCode += thisCharacter.exp;
        }
        else if(thisCharacter.exp > 99)
        {
            newCode += "0" + thisCharacter.exp;
        }
        else if(thisCharacter.exp > 9)
        {
            newCode += "00" + thisCharacter.exp;
        }
        else
        {
            newCode += "000" + thisCharacter.exp;
        }

        // Stats

        // HP (Max and Current)
        newCode += "H";

        if (thisCharacter.GetRawMaxHP() > 99)
        {
            newCode += thisCharacter.GetRawMaxHP();
        }
        else
        {
            newCode += "0" + thisCharacter.GetRawMaxHP();
        }

        // Current HP
        if (thisCharacter.currentHP > 99)
        {
            newCode += thisCharacter.currentHP;
        }
        else if (thisCharacter.currentHP > 9)
        {
            newCode += "0" + thisCharacter.currentHP;
        }
        else
        {
            newCode += "00" + thisCharacter.currentHP;
        }

        // Current MP
        if(thisCharacter.currentMP > 9)
        {
            newCode += thisCharacter.currentMP;
        }
        else
        {
            newCode += "0" + thisCharacter.currentMP;
        }

        // STR
        newCode += "S";

        if (thisCharacter.GetRawStrength() > 9)
        {
            newCode += thisCharacter.GetRawStrength();
        }
        else
        {
            newCode += "0" + thisCharacter.GetRawStrength();
        }

        // MAG
        newCode += "M";

        if (thisCharacter.GetRawMagic() > 9)
        {
            newCode += thisCharacter.GetRawMagic();
        }
        else
        {
            newCode += "0" + thisCharacter.GetRawMagic();
        }

        // DEX
        newCode += "D";

        if (thisCharacter.GetRawDexterity() > 9)
        {
            newCode += thisCharacter.GetRawDexterity();
        }
        else
        {
            newCode += "0" + thisCharacter.GetRawDexterity();
        }

        // FTH
        newCode += "F";

        if (thisCharacter.GetRawFaith() > 9)
        {
            newCode += thisCharacter.GetRawFaith();
        }
        else
        {
            newCode += "0" + thisCharacter.GetRawFaith();
        }

        // Spell list. Altered by rings and Mime. Max 20 spells. Empty spells are "00"
        for(int i = 0; i < 20; i++)
        {
            if(i < thisCharacter.spellList.Count)
            {
                if (thisCharacter.spellList[i].abilityCode > 9)
                {
                    newCode += thisCharacter.spellList[i].abilityCode;
                }
                else
                {
                    newCode += "0" + thisCharacter.spellList[i].abilityCode;
                }
            }
            else
            {
                newCode += "00";
            }
        }

        return newCode;
    }

    // Create a full inventory code. First add equipped weapons/badges, then non-equipped items
    string CreateInventoryCode()
    {
        // First, equipment
        string inventoryCode = "E";

        Character[] allCharacters = FindObjectOfType<GameController>().allCharacters;

        for(int i = 0; i < allCharacters.Length; i++)
        {
            // NOTE: Fists are not added to the inventory, but DO count as itemCode = 50 for equipping purposes
            if (allCharacters[i].equippedWeapon.itemCode > 9)
            {
                inventoryCode += allCharacters[i].equippedWeapon.itemCode;
            }
            else
            {
                inventoryCode += "0" + allCharacters[i].equippedWeapon.itemCode;
            }

            // Badge. Note that "00" indicates no badge
            if(allCharacters[i].equippedBadge == null)
            {
                inventoryCode += "00";
            }
            else // Having a badge equipped reduces potential inventory space
            {
                if (allCharacters[i].equippedBadge.itemCode > 9)
                {
                    inventoryCode += allCharacters[i].equippedBadge.itemCode;
                }
                else
                {
                    inventoryCode += "0" + allCharacters[i].equippedBadge.itemCode;
                }
            }
        }

        // Next, all non-equipped items
        inventoryCode += "I";

        // Add all items at the end
        List<Item> inventoryList = FindObjectOfType<BagWindow>(true).allStoredItems;

        // 4 total inventory slots beyond equipment. Note that equipped items are not part of this list and exist as extra "00"'s at the end
        for(int i = 0; i < 12; i++)
        {
            if(i < inventoryList.Count)
            {
                if (inventoryList[i].itemCode > 9)
                {
                    inventoryCode += inventoryList[i].itemCode;
                }
                else
                {
                    inventoryCode += "0" + inventoryList[i].itemCode;
                }
            }
            else
            {
                inventoryCode += "00";
            }
        }

        return inventoryCode;
    }
    
    public Item FindItemWithID(int searchingID)
    {
        // First, check if it's a weapon.
        foreach (Weapon storedWeapon in allWeapons)
        {
            if (storedWeapon.itemCode == searchingID)
            {
                return storedWeapon;
            }
        }

        // If it's not a weapon, look for a badge
        foreach (Badge storedBadge in allBadges)
        {
            if (storedBadge.itemCode == searchingID)
            {
                return storedBadge;
            }
        }

        // If it's not a badge, look for a ring
        foreach (Ring storedRing in allRings)
        {
            if (storedRing.itemCode == searchingID)
            {
                return storedRing;
            }
        }

        // If you can't find the desired item, return null
        return null;
    }

    // Look for an enemy or Ring ability. Does not include spells learned only through normal means
    public Ability FindAbilityWithID(int searchingID)
    {
        foreach (Ability storedAbility in allAbilities)
        {
            if (storedAbility.abilityCode == searchingID)
            {
                return storedAbility;
            }
        }

        return null;
    }

    // Add an item code to the running loot code
    public void AddToLootCode(int itemID)
    {
        if(itemID > 9)
        {
            lootCode += itemID;
        }
        else
        {
            lootCode += "0" + itemID;
        }

        // When you receive loot, save the loot code here. CreateSaveData() is run once the treasure room closes
        PlayerPrefs.SetString("LootCode", lootCode); 
    }

    // When loading from the file, remove all loot that's already been grabbed
    public void AdjustLootFromCode()
    {
        TreasureRoom adjustedLootPool = FindObjectOfType<TreasureRoom>(true);

        for (int i = 0; i < lootCode.Length; i += 2)
        {
            int chosenItemCode = GetComponent<SavedHeroLoader>().GetIntegerValueFrom('N', 'N', lootCode[i], lootCode[i + 1]);

            bool itemFound = false;

            // Run through each loot pool until it finds the chosen item
            while (itemFound == false)
            {
                for(int c = 0; c < adjustedLootPool.commonLootPool.Count; c++)//each (Item commonItem in adjustedLootPool.commonLootPool)
                {
                    if (!itemFound && adjustedLootPool.commonLootPool[c].itemCode == chosenItemCode)
                    {
                        Debug.Log("Removed " + adjustedLootPool.commonLootPool[c].itemName + " from common pool");
                        adjustedLootPool.commonLootPool.Remove(adjustedLootPool.commonLootPool[c]);

                        itemFound = true;
                    }
                }

                for(int r = 0; r < adjustedLootPool.rareLootPool.Count; r++) //each (Item rareItem in adjustedLootPool.rareLootPool)
                {
                    if (!itemFound && adjustedLootPool.rareLootPool[r].itemCode == chosenItemCode)
                    {
                        Debug.Log("Removed " + adjustedLootPool.rareLootPool[r].itemName + " from rare pool");
                        adjustedLootPool.rareLootPool.Remove(adjustedLootPool.rareLootPool[r]);

                        itemFound = true;
                    }
                }

                for(int l = 0; l < adjustedLootPool.legendaryLootPool.Count; l++)  //each (Item legendaryItem in adjustedLootPool.legendaryLootPool)
                {
                    if (!itemFound && adjustedLootPool.legendaryLootPool[l].itemCode == chosenItemCode)
                    {
                        Debug.Log("Removed " + adjustedLootPool.legendaryLootPool[l].itemName + " from legendary pool");
                        adjustedLootPool.legendaryLootPool.Remove(adjustedLootPool.legendaryLootPool[l]);

                        itemFound = true;
                    }
                }

                // If it can't find the item, just break
                if (itemFound == false)
                {
                    Debug.Log("Could not find item " + chosenItemCode);
                    itemFound = true;
                }
            }
        }
    }
}

/*
    ITEM DIRECTORY

        00 Empty
        01 Heartstone
        02 Aegis
        03 Spellshield
        04 Bulwark
        05 Burning Coal
        06 Sunstone
        07 Third Eye
        08 Dragon Fang
        09 Lucky Doubloon
        10

        50 Fists
        51 Bronze Sword
        52 Iron Sword
        53 Golden Sword
        54 Platinum Sword
        55 Red Claymore
        56 Bone Knife
        57 Wind Katana
        58 Blood Katana
        59 Spirit Katana
        60 Training Bow
        61 Scout Bow
        62 Ranger Bow
        63 Boom Bow
        64 Artemis Bow
        65 Eagle Greatshield
        66
        67
        68
        69
        70 Wooden Staff
        71 Sapphire Staff
        72 Emerald Staff
        73 Ruby Staff
        74 Briar Staff
        75 Priest Mallet
        76 Crusader Sledge
        77 Paladin Mallet
        78
        79
        80 Glass Lance
        81 Lightning Rod
        82 Rusty Pitchfork
        83 Fallen King Lance
        84
        85 Leather Whip
        86 Tesla Whip
        87
        88
        89

        90 Fire Ring
        91 Cure Ring
        92 Icetu Ring
        93 Revive Ring

    SPELL DIRECTORY

        00 Empty
        01 Fire
        02 Ice
        03 Lightning
        04 Arcane
        05 Firetu
        06 Icetu
        07 Lightningtu
        08 Comet
        09 
        
        10 Cure
        11 Curetu
        12 Banish
        13 Barrier
        14 BarrierAll
        15 Revive
        16 Miracle
        17 
        18
        19
        20

        21 Life
        22 Lifetu
        23 Empower
        24 Skelebuddy
        25 Fee
        26 Magmaiden
        27 Squidge
        28 Feenix
        29

        30 Drain
        31 Draintu
        
        50 Squiceberg
        51 Clobber
        52 Trick-Treat
        53 Hothead
        54 Stampede
        55 Warp
        56 Zombie Ray
        57 Squeal!
        58 Googoo
        59 Smog
        60 Heartburn
        61 Psycho H
        62 Squightning
        63 Soul Breath
        64 Wyrmblood
        65
        66
        67
        68
        69

*/
