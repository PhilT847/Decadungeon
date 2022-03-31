using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Unit
{
    public Hero chosenClass;

    public Sprite characterPortrait;

    public Weapon equippedWeapon;
    public Badge equippedBadge;

    //while in the back row, become less likely to be targeted. However, melee weapons deal only 50% damage.
    public bool inBackRow;
    public bool protectingAllies; //when using Taunt/Protect, tank all direct hits for allies.
    public float timeProtecting; //units spend 20s protecting.
    public bool evadingAttacks; //Automatic 75% dodge rate as well as faster ATB gauge speed
    public float timeEvading; //units spend 10s evading.

    public float barrierTimeRemaining; //barrier reduces incoming damage by 20-60% based on allied Faith. Lasts 20s
    public float barrierGuardMult; //percentage damage blocked by barrier, if active

    public float timeUntilRegenTick; //regen ticks every 5s. Having Regen cast on you resets this timer.
    public float regenTimeRemaining; //regen heals a certain amount of HP every 5s.
    public int regenHealValue; //regen value is how much is healed each turn. Most recent value applied.

    public Sprite[] faceAndHair; //0-3 are faces (neutral, blink, hurt, happy), 4 is frontHair, 5 is backHair, 6 is ear

    //the first and second colors for whichever outfit this unit has
    public Color primaryColor;
    public Color secondaryColor;
    public Color skinTone;
    public Color hairColor;

    public Animator characterAnim;
    public Face unitFace; // The face of the unit. Taken from Hero during HeroSetup()
    public Animator extraAnim; //the "extra animator"... includes special class animations (Ex. the Knight's blocking shield)

    public Enchantment weaponEnchant; //the Enchanter can add various enchantments to weapons. Removing status effects also removes enchantments.
    public Summon currentSummon; // Used by Druids. If the character has an active summon, then the summon takes damage for them.

    // Monks use Chi instead of Mana
    public int currentChi;
    public int maxChi; // 3 baseline

    //Modifiers to class growths.
    public int hpGrowth;
    public int strGrowth;
    public int magGrowth;
    public int dexGrowth;
    public int fthGrowth;

    public int exp;
    public int expToNextLevel;

    public int lifetimeDamageDealt;
    public int lifetimeHealingDealt;
    public int lifetimeKills;
    public int lifetimeDeaths;

    public override void TakeDamage(Unit source, int value, bool physicalDamage, string element)
    {
        //dead heroes do not take damage. First ensure that the hero is alive; otherwise, area-of-effect damage may incorrectly deal damage to dead heroes.
        if(currentHP > 0)
        {
            // If the unit has an active summon, then the summon will take the hit for them
            if(currentSummon != null)
            {
                currentSummon.TakeDamage(source, value, physicalDamage, element);
                return;
            }

            // Add bonus attack power from the source, if applicable
            if(source != null)
            {
                value = (int)(value * source.bonusAttackPower);
            }

            bool hitCritical = false;

            //10% randomizer on damage.
            value = (int)(value * Random.Range(0.9f, 1.1f));

            //damage capped at 999.
            if (value > 999)
            {
                value = 999;
            }

            // Aura damage ignores defenses and cannot crit (it acts as "true damage"). Otherwise, reduce incoming damage by physical or magical defenses. Also roll for crit.
            if (element != "Aura")
            {
                if (physicalDamage)
                {
                    value -= physicalDefense;
                }
                else //25% of faith is also applied as magic defense.
                {
                    value -= magicalDefense + (faith / 4);
                }

                //crit rate is (50% of dex)... enemy crits are only 1.25x strength to prevent random wipes.
                int critRoll = Random.Range(1, 101);
                if (critRoll < (source.dexterity / 2) + source.criticalHitBonus)
                {
                    value = (int)(value * source.criticalHitCoefficient);
                    hitCritical = true;
                }

                // Liches reduce incoming non-Aura damage based on HP value
                if (chosenClass.className == "Lich")
                {
                    // 100% damage at max HP. 50% damage at minimal HP
                    float lichGuardMult = 0.5f + (((float)currentHP / maxHP) / 2f);
                    value = (int)(value * lichGuardMult);
                }

                //If a barrier exists, reduce incoming non-Aura damage.
                if (barrierTimeRemaining > 0f)
                {
                    value = (int)(value * barrierGuardMult);
                }
            }

            // Resistant elements deal 1/3 damage
            if (resistancesList.Contains(element))
            {
                value /= 3;
            }
            else if (weaknessesList.Contains(element)) // If the damage's element is a weakness, double incoming damage
            {
                value = (int)(value * 2f);
            }

            // Always take at least 1 damage
            if (value < 1)
            {
                value = 1;
            }

            int dodgeRoll = Random.Range(1, 101);

            //every 4 points of dexterity grants 1% dodge (essentially 0.25% per point). Note that Aura attacks are not dodged
            if (element != "Aura" && ((evadingAttacks && dodgeRoll <= 75) || (!evadingAttacks && dodgeRoll <= dexterity / 4)))
            {
                value = 0;
                PrintDamageText("DODGE");
                //Dodge text/sound
            }

            if (value > 0)
            {
                if (absorbancesList.Contains(element)) //If the unit absorbs this damage type, get healed instead of taking damage
                {
                    HealUnit(null, value);
                }
                else
                {
                    currentHP -= value;

                    characterAnim.SetTrigger("Hurt");

                    unitFace.SetFace("Damaged", 0.75f);

                    PrintDamageText(value, hitCritical, false, false);

                    // If this hero uses Chi, gain 1 Chi for taking the hit
                    if(maxChi > 0)
                    {
                        GainChi(1);
                    }
                }
            }

            //When dead, reduce HP to 0 and remove all status effects.
            if (currentHP < 1)
            {
                KillUnit();
            }

            //update the battle hp/mp bars to ensure they're set up
            foreach (HeroStatus heroHP in FindObjectsOfType<HeroStatus>())
            {
                heroHP.UpdateHeroStatus();
            }
        }
    }

    public override void HealUnit(Unit source, int value)
    {
        //when healed by an ally, add to their lifetime healing count.
        if(source != null)
        {
            if (source.GetComponent<Character>())
            {
                source.GetComponent<Character>().lifetimeHealingDealt += value;
            }
            else if (source.GetComponent<Summon>())
            {
                source.GetComponent<Summon>().summonOwner.lifetimeHealingDealt += value;
            }
        }

        //healing only affects living targets.
        if (currentHP > 0)
        {
            //damage/healing capped at 999.
            if (value > 999)
            {
                value = 999;
            }

            currentHP += value;

            if (currentHP > maxHP)
            {
                currentHP = maxHP;
            }

            PrintDamageText(value, false, true, false);

            // Heroes smile when healed
            unitFace.SetFace("Happy", 1.25f);

            //update the battle hp/mp bars to ensure they're set up
            foreach (HeroStatus heroHP in FindObjectsOfType<HeroStatus>())
            {
                heroHP.UpdateHeroStatus();
            }
        }
        else //when trying to heal a dead unit, print "MISS" instead of healing.
        {
            PrintDamageText("MISS");
        }
    }

    //heals a unit from 0 HP.
    public void ReviveUnit(int healValue)
    {
        GhostifyUnit(false);

        if (currentHP == 0)
        {
            PrintDamageText("REVIVE");

            // Ensure that the hero is revived with at least 1 HP.
            if(healValue < 1)
            {
                healValue = 1;
            }

            //Reviving doesn't heal like a normal spell; it prints "REVIVE" and increases HP as needed.
            currentHP = healValue;

            if (currentHP > maxHP)
            {
                currentHP = maxHP;
            }

            //update the battle hp/mp bars to ensure they're set up
            foreach (HeroStatus heroHP in FindObjectsOfType<HeroStatus>())
            {
                heroHP.UpdateHeroStatus();
            }
        }
        else
        {
            PrintDamageText("MISS");
        }
    }

    //Restores a unit to full HP/MP. Used by Fountains and when going upstairs.
    public void FullRestore()
    {
        GhostifyUnit(false);

        currentHP = maxHP;
        currentMP = maxMP;
    }

    //reduces a unit's HP to zero, removes status effects, and checks to see if the whole party is dead.
    public void KillUnit()
    {
        //add to the amount of deaths over their lifetime.
        lifetimeDeaths++;

        currentHP = 0;
        RemoveAllStatusEffects();

        GhostifyUnit(true);

        // Monks lose all Chi when dying
        if(maxChi > 0)
        {
            currentChi = 0;
            GainChi(0);
        }

        //If there are no living units, end the game.
        if (FindObjectOfType<GameController>().FindLivingCharacters().Count == 0)
        {
            FindObjectOfType<GameController>().StartCoroutine(FindObjectOfType<GameController>().LoseGame());
        }
    }
    
    public void GhostifyUnit(bool isDead)
    {
        if (isDead)
        {
            foreach (SpriteRenderer characterSprite in chosenClass.GetComponentsInChildren<SpriteRenderer>(true))
            {
                // Note that status effects (ex. Protect's shield and Acrobatics's boots) must be excluded
                if (characterSprite.gameObject.tag != "ExtraAnimator" && characterSprite.color.a != 0.33f)
                {
                    //Ghosts are semi-transparent and have less red
                    Color ghostColor = characterSprite.color;
                    ghostColor.a = 0.33f;
                    ghostColor.r /= 2f;
                    characterSprite.color = ghostColor;
                }
            }
        }
        else
        {
            // Return to physical status
            foreach (SpriteRenderer characterSprite in chosenClass.GetComponentsInChildren<SpriteRenderer>(true))
            {
                // Note that status effects (ex. Protect's shield and Acrobatics's boots) must be excluded
                if (characterSprite.gameObject.tag != "ExtraAnimator" && characterSprite.color.a != 1f)
                {
                    Color livingColor = characterSprite.color;
                    livingColor.a = 1f;
                    livingColor.r *= 2f;
                    characterSprite.color = livingColor;
                }
            }
        }
    }

    public void RestoreMP(int value)
    {
        // Monks do not regenerate MP in any way
        if(chosenClass.className == "Monk")
        {
            return;
        }

        //damage/healing capped at 999.
        if (value > 999)
        {
            value = 999;
        }

        currentMP += value;

        if(currentMP > maxMP)
        {
            currentMP = maxMP;
        }
        else if(currentMP < 1)
        {
            currentMP = 0;
        }

        PrintDamageText(value, false, false, true);

        //update the battle hp/mp bars to ensure they're set up
        foreach (HeroStatus heroHP in FindObjectsOfType<HeroStatus>())
        {
            heroHP.UpdateHeroStatus();
        }
    }

    public void GainChi(int value)
    {
        // While in Phase Shift, you do not gain Chi
        if(unitBuffs.currentTimeMod < 5f)
        {
            currentChi += value;
        }

        if (currentChi > maxChi)
        {
            currentChi = maxChi;
        }

        // Update the battle hp/mp bars
        foreach (HeroStatus heroHP in FindObjectsOfType<HeroStatus>())
        {
            heroHP.UpdateHeroStatus();
        }
    }

    //remove/reduce status effects every 1s. See BattleController.
    public override void CheckStatusEffects()
    {
        if(timeProtecting > 0f)
        {
            timeProtecting -= 1f;
        }
        else if (protectingAllies)
        {
            protectingAllies = false;

            extraAnim.SetTrigger("EndBlock");
        }

        if (timeEvading > 0f)
        {
            timeEvading -= 1f;
        }
        else if (evadingAttacks)
        {
            evadingAttacks = false;

            extraAnim.SetTrigger("EndEvasion");
        }

        if(regenTimeRemaining > 0f)
        {
            regenTimeRemaining -= 1f;
        }

        if(timeUntilRegenTick > 0f)
        {
            timeUntilRegenTick -= 1f;
        }
        else
        {
            timeUntilRegenTick = 5f;
            RegenHPandMP();
        }

        if (barrierTimeRemaining > 0f)
        {
            barrierTimeRemaining -= 1f;
        }
        else if(chosenClass.barrierObject.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "BarrierIdle") // If the barrier is currently up, put it down
        {
            chosenClass.barrierObject.GetComponent<Animator>().SetTrigger("BarrierDown");
        }

        CheckBuffs();
    }

    public void RegenHPandMP()
    {
        //since the Sunstone and regen both regenerate HP, add both values together if necessary so that only one healing value needs to appear.
        int totalRegenValue = 0;

        if (regenTimeRemaining > 0f)
        {
            totalRegenValue += regenHealValue;
        }

        //Sunstone regenerates 10% max HP per tick.
        if (equippedBadge != null && equippedBadge.regenHP)
        {
            totalRegenValue += maxHP / 10;
        }

        if (totalRegenValue > 0)
        {
            HealUnit(null, totalRegenValue);
        }

        //Third Eye regenerates 2 MP every 5s
        if (equippedBadge != null && equippedBadge.regenMP)
        {
            // If regenerating HP, do not display the MP number. Otherwise, show it as they won't overlap
            if (totalRegenValue > 0)
            {
                currentMP += 2;

                if (currentMP > maxMP)
                {
                    currentMP = maxMP;
                }
            }
            else
            {
                RestoreMP(2);
            }
        }

        // Update the battle hp/mp bars
        foreach (HeroStatus heroHP in FindObjectsOfType<HeroStatus>())
        {
            heroHP.UpdateHeroStatus();
        }
    }

    public override void CheckBuffs()
    {
        for (int i = 0; i < unitBuffs.allBuffs.Count; i++)
        {
            unitBuffs.allBuffs[i].timeRemaining -= 1f;

            if (unitBuffs.allBuffs[i].timeRemaining < 1f)
            {
                unitBuffs.allBuffs[i].RemoveBuff();
                unitBuffs.allBuffs.Remove(unitBuffs.allBuffs[i]);
                i--;
            }
        }

        unitBuffs.UpdateBuffBar(this);
    }

    public void RemoveAllBuffs()
    {
        for(int i = 0; i < unitBuffs.allBuffs.Count; i++)
        {
            unitBuffs.allBuffs[i].RemoveBuff();
            unitBuffs.allBuffs.Remove(unitBuffs.allBuffs[i]);
            i--;
        }

        unitBuffs.UpdateBuffBar(this);
    }

    // Purify any negative debuffs
    public void CleanseDebuffs()
    {
        List<Buff> allUnitBuffs = unitBuffs.allBuffs;

        for (int i = 0; i < allUnitBuffs.Count; i++)
        {
            if (allUnitBuffs[i].attackMod < 0 || allUnitBuffs[i].defenseMod < 0 || allUnitBuffs[i].dexMod < 0 || allUnitBuffs[i].turnSpeedMod < 0)
            {
                allUnitBuffs[i].RemoveBuff();
                allUnitBuffs.Remove(allUnitBuffs[i]);
                i--;
            }
        }

        unitBuffs.UpdateBuffBar(this);
    }

    //Remove all status effects when a battle ends.
    public void RemoveAllStatusEffects()
    {
        if (protectingAllies)
        {
            protectingAllies = false;
            extraAnim.SetTrigger("EndBlock");
        }

        if (evadingAttacks)
        {
            evadingAttacks = false;
            extraAnim.SetTrigger("EndEvasion");
        }

        timeUntilRegenTick = 5f;
        regenTimeRemaining = 0;

        if(barrierTimeRemaining > 0)
        {
            chosenClass.barrierObject.GetComponent<Animator>().SetTrigger("BarrierDown");
        }

        barrierTimeRemaining = 0;

        weaponEnchant.ClearEnchantments();

        // Kill the unit's summon if they have any
        if(currentSummon != null)
        {
            currentSummon.KillSummon();
        }

        RemoveAllBuffs();
    }

    public void SwapPosition()
    {
        inBackRow = !inBackRow;

        if (inBackRow)
        {
            chosenClass.transform.localPosition = new Vector3(-270f, chosenClass.transform.localPosition.y, 0f);
        }
        else
        {
            chosenClass.transform.localPosition = new Vector3(-200f, chosenClass.transform.localPosition.y, 0f);
        }
    }

    // The combined physical attack power of a unit and their weapon
    public int CharacterAttackPower()
    {
        // Note that "strength" also includes bonuses from buffs and badges
        int rawPower = strength;

        rawPower += equippedWeapon.weaponMight;

        // Being in the incorrect row for a weapon halves attack power. Mixed range weapons deal full damage anywhere
        if (!equippedWeapon.mixedRangeWeapon && ((inBackRow && !equippedWeapon.rangedWeapon) || (!inBackRow && equippedWeapon.rangedWeapon)))
        {
            rawPower /= 2;
        }

        return rawPower;
    }

    // Getters for raw stats (not counting buffs and weapon bonuses). Used to read character stats

    public int GetRawMaxHP()
    {
        int rawHP = maxHP - equippedWeapon.bonusHP;

        if(equippedBadge != null)
        {
            rawHP -= equippedBadge.hpBonus;
        }

        return rawHP;
    }

    // Raw MP is just the Magic stat
    public int GetRawMaxMP()
    {
        // Monks do not use MP
        if(chosenClass.className == "Monk")
        {
            return 0;
        }

        return GetRawMagic();
    }

    public int GetRawStrength()
    {
        int rawStr = strength - unitBuffs.currentAtkMod;

        if (equippedBadge != null)
        {
            rawStr -= equippedBadge.strengthBonus;
        }

        return rawStr;
    }

    public int GetRawMagic()
    {
        return magic - unitBuffs.currentAtkMod;
    }

    public int GetRawDexterity()
    {
        return dexterity - equippedWeapon.bonusDexterity - unitBuffs.currentDexMod;
    }

    public int GetRawFaith()
    {
        return faith - equippedWeapon.bonusFaith;
    }

    public int GetRawPhysicalDefense()
    {
        int rawDef = physicalDefense - equippedWeapon.bonusPhysicalDefense - unitBuffs.currentDefMod;

        if (equippedBadge != null)
        {
            rawDef -= equippedBadge.physicalDefenseBonus;
        }

        return rawDef;
    }

    // Magical defense is faith / 4
    public int GetRawMagicalDefense()
    {
        return GetRawFaith() / 4;
    }

    // Retrieves a stat for the detailed menu. Includes equipment bonuses, but not buff modifiers
    public int GetStatWithEquipment(string whichStat)
    {
        switch (whichStat)
        {
            case "HP":
                int equippedHP = maxHP + equippedWeapon.bonusHP;

                if (equippedBadge != null)
                {
                    equippedHP += equippedBadge.hpBonus;
                }

                return equippedHP;

            case "MP":

                // Monks do not use MP and receive no bonuses from equipment
                if(chosenClass.className == "Monk")
                {
                    return 0;
                }

                int equippedMP = magic + equippedWeapon.bonusMP;

                if (equippedBadge != null)
                {
                    equippedMP += equippedBadge.mpBonus;
                }

                return equippedMP;

            case "Strength":
                int equippedSTR = GetRawStrength();

                if (equippedBadge != null)
                {
                    equippedSTR += equippedBadge.strengthBonus;
                }

                return equippedSTR;

            case "Magic":
                return GetRawMagic();

            case "Dexterity":
                return GetRawDexterity() + equippedWeapon.bonusDexterity;

            case "Faith":
                return GetRawFaith() + equippedWeapon.bonusFaith;

            case "Physical Defense":
                int equippedDef = GetRawPhysicalDefense() + equippedWeapon.bonusPhysicalDefense;

                if (equippedBadge != null)
                {
                    equippedDef += equippedBadge.physicalDefenseBonus;
                }

                return equippedDef;

            case "Magical Defense":
                int equippedMDef = GetRawMagicalDefense() + (equippedWeapon.bonusFaith / 4) + equippedWeapon.bonusMagicalDefense;

                if (equippedBadge != null)
                {
                    equippedMDef += equippedBadge.magicalDefenseBonus;
                }

                return equippedMDef;
        }

        return 0;
    }

    public void SortSpells()
    {
        /*Sorts spells like so:
         * Black
         * White
         * Summons
         * Green
         * Each is internally sorted by time learned
        */

        List<Spell> allBlackMagic = new List<Spell>();
        List<Spell> allWhiteMagic = new List<Spell>();
        List<Spell> allSummonMagic = new List<Spell>();
        List<Spell> allGreenMagic = new List<Spell>();

        foreach (Spell checkedSpell in spellList)
        {
            if (checkedSpell.GetComponent<SimpleBlackMagic>() || checkedSpell.GetComponent<LichMagic>()) // Black
            {
                allBlackMagic.Add(checkedSpell);
            }
            else if (checkedSpell.GetComponent<WhiteMagic>()) // White
            {
                allWhiteMagic.Add(checkedSpell);
            }
            else if (checkedSpell.GetComponent<CreateSummon>()) //Summon
            {
                allSummonMagic.Add(checkedSpell);
            }
            else // Otherwise, it must be green magic (WildMagic)
            {
                allGreenMagic.Add(checkedSpell);
            }
        }

        // Now clear the old list and add each sorted spell

        spellList = new List<Ability>();

        spellList.AddRange(allBlackMagic);
        spellList.AddRange(allWhiteMagic);
        spellList.AddRange(allSummonMagic);
        spellList.AddRange(allGreenMagic);
    }

    // Updates the Blood Rush mechanic for liches. Bonus Attack power (and the guard in TakeDamage()) goes from 1-1.5
    public void UpdateBloodRush()
    {
        float hpPercentage = currentHP / maxHP;
        bonusAttackPower = 1.5f - (((float)currentHP / maxHP) / 2f);

        // The rusher is invisible until <=75% HP. The character must also be alive
        if (bonusAttackPower >= 1.125f && currentHP > 0)
        {
            extraAnim.gameObject.SetActive(true);

            // The spinner spins faster at lower HP values. Goes from 0.5 to 1
            extraAnim.SetFloat("BloodRushSpeed", 0.5f + (bonusAttackPower - 1f));
            extraAnim.transform.localScale = new Vector3(0.6f + ((bonusAttackPower - 1f) * 0.8f), 0.6f + ((bonusAttackPower - 1f) * 0.8f), 1f);

            // Color goes from a = 25 to a = 250 based on rush value
            int alphaValue = 25 + (int)((bonusAttackPower - 1f) * 450f);

            extraAnim.GetComponentInChildren<SpriteRenderer>().color = new Color32(255, 60, 60, (byte) alphaValue);
        }
        else
        {
            extraAnim.gameObject.SetActive(false);
        }
    }
}
