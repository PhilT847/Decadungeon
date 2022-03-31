using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Summon : Unit
{
    public Character summonOwner;

    public Slider summonHP_Bar;

    public float maxHealthPerMagic; // The amount of max HP this summon has based on the summoner's Magic

    public int timeUntilHealthLoss; // Summons lose 1 HP every 2 seconds

    public Transform summonGraphics; // The body increased in size by Empower

    public override void TakeDamage(Unit source, int value, bool physicalDamage, string element)
    {
        // Add bonus attack power from the source, if applicable
        if (source != null)
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

        //Aura damage ignores defenses and cannot crit (it acts as "true damage"). Otherwise, reduce incoming damage by physical or magical defenses. Also roll for crit.
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

        //every 4 points of dexterity grants 1% dodge (essentially 0.25% per point)
        if (dodgeRoll <= dexterity / 4)
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
                GetComponentInChildren<Animator>().SetTrigger("Hurt");

                currentHP -= value;

                //characterAnim.SetTrigger("Hurt");

                PrintDamageText(value, hitCritical, false, false);
            }
        }

        UpdateSummonHP_Bar();

        //When dead, reduce HP to 0 and remove all status effects.
        if (currentHP < 1)
        {
            KillSummon();
        }
    }

    // Summons can't technically be healed, so this function doesn't get much use unless the Magma absorbs fire damage
    public override void HealUnit(Unit source, int value)
    {
        currentHP += value;

        if(currentHP > maxHP)
        {
            currentHP = maxHP;
        }

        PrintDamageText(value, false, true, false);

        UpdateSummonHP_Bar();
    }

    public override void CheckStatusEffects() // Summons lose HP every tick so that you can't have them out forever
    {
        timeUntilHealthLoss--;

        if (timeUntilHealthLoss == 0)
        {
            timeUntilHealthLoss = 2;

            currentHP -= 1;

            if (currentHP < 1)
            {
                KillSummon();
            }

            UpdateSummonHP_Bar();
        }
    }

    // When summons are buffed, they are increased in size (see SummonerSpell). When the buff is gone, they shrink again
    public override void CheckBuffs()
    {
        if (transform.localScale.x > 1f && unitBuffs.currentAtkMod < 1)
        {
            StartCoroutine(ChangeSize(false));
        }
    }

    public IEnumerator ChangeSize(bool isLarger)
    {
        if (isLarger)
        {
            float timeToChange = 0f;

            while (timeToChange < 0.5f)
            {
                timeToChange += Time.deltaTime;

                summonGraphics.localScale = new Vector3(1f + (0.4f * timeToChange), 1f + (0.4f * timeToChange), 1f);

                yield return new WaitForEndOfFrame();
            }

            summonGraphics.localScale = new Vector3(1.2f, 1.2f, 1f);
        }
        else
        {
            float timeToChange = 0f;

            while (timeToChange < 0.5f)
            {
                timeToChange += Time.deltaTime;

                summonGraphics.localScale = new Vector3(1.2f - (0.4f * timeToChange), 1.2f - (0.4f * timeToChange), 1f);

                yield return new WaitForEndOfFrame();
            }

            summonGraphics.localScale = new Vector3(1f, 1f, 1f);
        }

        yield return null;
    }

    //A summon's HP is based on unit health, and their damage is based on unit Magic
    public void SetupSummon(Character newOwner)
    {
        summonOwner = newOwner;

        level = newOwner.level;

        maxHP = (int) (newOwner.magic * maxHealthPerMagic);
        currentHP = maxHP;

        // All non-HP stats are based on the caster's Magic stat
        strength = newOwner.magic;
        magic = newOwner.magic;
        dexterity = newOwner.magic;
        faith = newOwner.magic;

        UpdateSummonHP_Bar();

        ATB_CurrentCharge = 10f;
        ATB_TimeUntilAttack = 10f;
        timeUntilHealthLoss = 2;

        criticalHitCoefficient = 2f;
        bonusAttackPower = 1f;
        turnSpeedMultiplier = 1f;

        // Set the owner's current summon to this Summon
        summonOwner.currentSummon = this;

        // Add this summon to the turn order
        FindObjectOfType<BattleController>().turnOrder.Add(this);
    }

    public void KillSummon()
    {
        //Ensure that this summon doesn't begin attacking before dying
        ATB_TimeUntilAttack = 99f;

        FindObjectOfType<BattleController>().turnOrder.Remove(this);

        summonOwner.currentSummon = null;

        RemoveAllStatusEffects();

        summonHP_Bar.gameObject.SetActive(false);

        GetComponentInChildren<Animator>().SetTrigger("Dispel");

        Destroy(gameObject, 0.55f);
    }

    public void UseAbility()
    {
        StartCoroutine(SummonAction());
    }

    public IEnumerator SummonAction()
    {
        GetComponentInChildren<Animator>().SetTrigger("Attack");

        //freeze the ATB gauges of all other enemies while attacking.
        FindObjectOfType<BattleController>().ATB_Active = false;
        ATB_CurrentCharge = 0f;

        Ability chosenAttack = skillList[Random.Range(0, skillList.Count)];

        //time to read the enemy attack text and for the enemy to animate 0.75s in
        yield return new WaitForSeconds(0.75f);

        //attack... NOTE THAT AREA ATTACKS ARE USED IN THIS CALC AS WELL... direct target only is used in the function if it needs one...
        chosenAttack.UseAbility(this, PickDirectTarget());

        //time before selecting the next unit
        yield return new WaitForSeconds(1f);

        if (!FindObjectOfType<BattleController>().CheckForWonBattle())
        {
            //Activate the ATB gauges once more.
            FindObjectOfType<BattleController>().ATB_Active = true;
        }

        yield return null;
    }

    // Choose an enemy to attack. During boss battles, always select the boss
    Unit PickDirectTarget()
    {
        if (FindObjectOfType<BattleController>().isBossBattle)
        {
            return FindObjectOfType<Boss>();
        }
        else
        {
            Enemy[] allEnemies = FindObjectsOfType<Enemy>();

            return allEnemies[Random.Range(0, allEnemies.Length)];
        }
    }

    // Removes buffs from this summon
    void RemoveAllStatusEffects()
    {
        for (int i = 0; i < unitBuffs.allBuffs.Count; i++)
        {
            unitBuffs.allBuffs[i].RemoveBuff();
            unitBuffs.allBuffs.Remove(unitBuffs.allBuffs[i]);
            i--;
        }

        unitBuffs.UpdateBuffBar(this);
    }

    void UpdateSummonHP_Bar()
    {
        summonHP_Bar.value = (float)currentHP / maxHP;
    }
}
