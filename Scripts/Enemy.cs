using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Unit
{
    public Sprite[] enemyImages; // Potential appearances

    public Slider healthBarSlider;

    public int baseExpGranted;

    public override void TakeDamage(Unit source, int value, bool physicalDamage, string element)
    {
        StartCoroutine(EnemyTakeDamage(source, value, physicalDamage, element));

        healthBarSlider.value = currentHP;
    }

    public override void HealUnit(Unit source, int value)
    {
        currentHP += value;

        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }

        healthBarSlider.value = currentHP;

        PrintDamageText(value, false, true, false);
    }

    //reveal an enemy's health bar.
    public void ScanEnemy()
    {
        if (healthBarSlider)
        {
            healthBarSlider.gameObject.SetActive(true);
            healthBarSlider.maxValue = maxHP;
            healthBarSlider.value = currentHP;
        }
    }

    public override void CheckStatusEffects()
    {
        CheckBuffs();
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

    public void SetLevelAndStats(int setLevel)
    {
        level = setLevel;

        //Enemy stats scale 10% per level.
        float statIncreaseMult = 1f + (0.1f * level);
        float hpIncreaseMult = 1f + (0.1f * level);

        if (level < 6) //enemies of levels 1-5 are cut down by 20%.
        {
            statIncreaseMult *= 0.8f;
            hpIncreaseMult *= 0.8f;
        }
        else if(level > 20) //enemies above level 15 scale only 7.5% per additional level. Note that they receive all previous bonuses. HP scales only 2.5% per level.
        {
            statIncreaseMult = 3f + (0.075f * level);
            hpIncreaseMult = 3f + (0.025f * level);
        }

        maxHP = (int)(maxHP * hpIncreaseMult);
        strength = (int)(strength * statIncreaseMult);
        dexterity = (int)(dexterity * statIncreaseMult);
        magic = (int)(magic * statIncreaseMult);
        faith = (int)(faith * statIncreaseMult);
        physicalDefense = (int)(physicalDefense * statIncreaseMult);
        magicalDefense = (int)(magicalDefense * statIncreaseMult);

        //EXP only scales 5% so that heroes have to win more battles to level up as their level increases.
        baseExpGranted = (int)(baseExpGranted * (1f + (0.05f * level)));

        //10x EXP in debug mode as well as 1 HP for easy kills
        if (FindObjectOfType<GameController>().DEBUG_MODE == 1)
        {
            baseExpGranted *= 5;

            maxHP = 1;
        }

        //ensure that HP reaches its max after the increase.
        currentHP = maxHP;
    }

    public void SetBattleEnemy(Enemy newEnemy, int chosenLevel)
    {
        GetComponentInChildren<Image>().sprite = newEnemy.enemyImages[Random.Range(0, newEnemy.enemyImages.Length)];

        maxHP = newEnemy.maxHP;
        strength = newEnemy.strength;
        dexterity = newEnemy.dexterity;
        magic = newEnemy.magic;
        faith = newEnemy.faith;

        physicalDefense = newEnemy.physicalDefense;
        magicalDefense = newEnemy.magicalDefense;

        baseExpGranted = newEnemy.baseExpGranted;

        currentHP = maxHP;

        skillList = newEnemy.skillList;

        unitName = newEnemy.unitName;

        damageText = newEnemy.damageText;

        resistancesList = newEnemy.resistancesList;
        absorbancesList = newEnemy.absorbancesList;
        weaknessesList = newEnemy.weaknessesList;

        criticalHitCoefficient = newEnemy.criticalHitCoefficient;
        bonusAttackPower = newEnemy.bonusAttackPower;
        turnSpeedMultiplier = newEnemy.turnSpeedMultiplier;

        //also find the health bar slider and weakness indicator in the Enemy prefab
        healthBarSlider = GetComponentInChildren<Slider>(true);
        healthBarSlider.GetComponentInChildren<WeaknessBar>().SetToEnemy(this);

        unitBuffs = GetComponentInChildren<UnitBuffs>(true);

        SetLevelAndStats(chosenLevel);

        ATB_TimeUntilAttack = 10f;
    }

    public IEnumerator EnemyAction()
    {
        //If living and not frozen, act!
        if (currentHP > 0)
        {
            //freeze the ATB gauges of all other enemies while attacking.
            FindObjectOfType<BattleController>().ATB_Active = false;
            ATB_CurrentCharge = Random.Range(0f, ATB_TimeUntilAttack * 0.33f); // ATB fills up to 1/3 randomly to mix up turn order

            Ability chosenAttack = skillList[Random.Range(0, skillList.Count)];

            //time for unit to reach attack position
            yield return new WaitForSeconds(0.125f);

            FindObjectOfType<BattleController>().ActivateEnemyActionText(chosenAttack);

            GetComponentInChildren<Animator>().SetTrigger("EnemyAction");

            //time to read the enemy attack text and for the enemy to animate 1s in
            yield return new WaitForSeconds(1.25f);

            //if it's a heal or buff, pick an ally. Note that area of effect support spells are used correctly even when "picking" a direct target.
            if(chosenAttack.GetComponent<EnemySpecialAttack>() && chosenAttack.GetComponent<EnemySpecialAttack>().healingSpell)
            {
                chosenAttack.UseAbility(this, PickOtherEnemy());
            }
            else
            {
                //attack... NOTE THAT AREA ATTACKS ARE USED IN THIS CALC AS WELL... direct target only is used in the function if it needs one...
                chosenAttack.UseAbility(this, PickDirectTarget());

                //If it's a multi-hit attack, cast it as many times as necessary.
                if(chosenAttack.amountOfHits > 1)
                {
                    yield return new WaitForSeconds(0.25f); //wait a moment before casting again

                    for (int i = 1; i < chosenAttack.amountOfHits; i++)
                    {
                        GetComponentInChildren<Animator>().SetTrigger("EnemyAction");

                        //time to read the enemy attack text and for the enemy to animate 1s in
                        yield return new WaitForSeconds(1.25f);

                        chosenAttack.UseAbility(this, PickDirectTarget());

                        yield return new WaitForSeconds(0.25f);
                    }
                }
            }

            //time after the attack connects before the enemy action text goes away
            yield return new WaitForSeconds(0.125f);

            FindObjectOfType<BattleController>().RemoveEnemyActionText();

            //time before selecting the next unit
            yield return new WaitForSeconds(0.75f);
        }

        //Activate the ATB gauges once more.
        FindObjectOfType<BattleController>().ATB_Active = true;

        yield return null;
    }

    //Check all living units and select one at random. Certain characters are counted more to increase their odds of being selected.
    public Unit PickDirectTarget()
    {
        List<Character> targetsList = new List<Character>();
        List<Character> protectingList = new List<Character>();

        //Adds each living character to the list of potential targets.
        foreach (Character c in FindObjectOfType<GameController>().FindLivingCharacters())
        {
            targetsList.Add(c);

            //units in the front row are counted three times, making them more likely to be targeted directly.
            if (!c.inBackRow)
            {
                targetsList.Add(c);
                targetsList.Add(c);
            }

            //units using the Protect command are prioritized
            if (c.protectingAllies)
            {
                protectingList.Add(c);
            }
        }

        // ALWAYS prefer a Protecting character
        if (protectingList.Count > 0)
        {
            return protectingList[Random.Range(0, protectingList.Count)];
        }

        return targetsList[Random.Range(0, targetsList.Count)];
    }

    public Enemy PickOtherEnemy()
    {
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();

        return allEnemies[Random.Range(0, allEnemies.Length)];
    }

    public IEnumerator KillEnemy()
    {
        GetComponentInChildren<Animator>().SetTrigger("EnemyDeath");

        //Remove the enemy from all lists
        FindObjectOfType<BattleController>().RemoveEnemy(this);

        //Wait 0.75f before destroying the object
        yield return new WaitForSeconds(0.75f);

        //Destroys the unit along with its image and HP bar slider so that they're not detected by the Spy command, attacks, or other actions
        Destroy(gameObject);

        yield return null;
    }

    //since the round doesn't end 
    public IEnumerator EnemyTakeDamage(Unit source, int value, bool physicalDamage, string element)
    {
        // Add bonus attack power from the source, if applicable
        if (source != null)
        {
            value = (int)(value * source.bonusAttackPower);
        }

        bool hitCritical = false;

        //10% randomizer on damage.
        value = (int)(value * Random.Range(0.9f, 1.1f));

        //damage/healing capped at 999.
        if (value > 999)
        {
            value = 999;
        }

        //Aura damage ignores defenses and cannot crit (it acts as "true damage"). Otherwise, reduce incoming damage by physical or magical defenses. Also roll for crit.
        if (element != "Aura")
        {
            if (physicalDamage)
            {
                value -= physicalDefense;
            }
            else //magicDefense is increased by 25% of Faith.
            {
                value -= magicalDefense + (faith / 4);
            }

            //crit rate is (50% of dex) and doubles damage (after reductions from defense/resistance)... triple for Rogues
            int critRoll = Random.Range(1, 101);
            if (critRoll < (source.dexterity / 2) + source.criticalHitBonus)
            {
                value = (int)(value * source.criticalHitCoefficient);

                hitCritical = true;
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

        // By default (with no source), the unit does not dodge. Allows for self-inflicted damage
        int dodgeRoll = 100;

        if(source != null && source.GetComponent<Character>())
        {
            //dodge takes the unit weapon's hit roll into account
            dodgeRoll = Random.Range(1, 101) + source.GetComponent<Character>().equippedWeapon.weaponHitMod;
        }

        //every 4 points of dexterity grants 1% dodge (essentially 0.25% per point)
        if (element != "Aura" && dodgeRoll <= dexterity / 4)
        {
            value = 0;
            PrintDamageText("DODGE");
            //Dodge text/sound. Also, the caster makes an annoyed face
            if (source.GetComponent<Character>())
            {
                source.GetComponent<Character>().unitFace.SetFace("Damaged", 0.75f);
            }
        }

        if (value > 0)
        {
            if (absorbancesList.Contains(element)) //If the unit absorbs this damage type, get healed instead of taking damage
            {
                HealUnit(null, value);
            }
            else //Deal damage
            {
                GetComponentInChildren<Animator>().SetTrigger("EnemyHurt");

                currentHP -= value;

                PrintDamageText(value, hitCritical, false, false);

                //when damaged by a hero, add to their lifetime damage dealt AND kills if the unit dies.
                if (source != null && source.GetComponent<Character>())
                {
                    source.GetComponent<Character>().lifetimeDamageDealt += value;

                    if (currentHP < 1)
                    {
                        source.GetComponent<Character>().lifetimeKills++;
                    }
                }
                else if(source != null && source.GetComponent<Summon>())
                {
                    source.GetComponent<Summon>().summonOwner.lifetimeDamageDealt += value;

                    if (currentHP < 1)
                    {
                        source.GetComponent<Summon>().summonOwner.lifetimeKills++;
                    }
                }
            }
        }

        if (currentHP < 1)
        {
            currentHP = 0;
        }

        // If dead, emove the select button before the ATB gauge can start moving again
        if(currentHP == 0)
        {
            GetComponentInChildren<Button>(true).gameObject.SetActive(false);
        }

        // Update the battle hp/mp bars
        foreach (HeroStatus heroHP in FindObjectsOfType<HeroStatus>())
        {
            heroHP.UpdateHeroStatus();
        }

        yield return new WaitForSeconds(1f);

        if (currentHP == 0)
        {
            StartCoroutine(KillEnemy());
        }

        //If you win, wait a moment before activating the win screen
        if (FindObjectOfType<BattleController>().CheckForWonBattle() && !FindObjectOfType<BattleController>().wonBattle)
        {
            yield return new WaitForSeconds(0.5f);

            FindObjectOfType<BattleController>().StartCoroutine(FindObjectOfType<BattleController>().WinBattle());
        }

        yield return null;
    }
}
