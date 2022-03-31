using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : Unit
{
    public Sprite bossHead; // Image that appears on the boss room

    public List<Ability> desperationAbilities; // Skill list when this boss is below 50% HP
    public BossMechanic bossMechanic; // Actions specific to this boss

    public GameObject bossBody;
    public Animator bossAnim;
    public Animator extraAnim;

    public Slider healthBarSlider;

    public int baseExpGranted;

    public override void TakeDamage(Unit source, int value, bool physicalDamage, string element)
    {
        if (element != "Physical" && bossMechanic.mechanicName == "MagicMirror" && bossMechanic.activated)
        {
            PrintDamageText("MIRROR");
            extraAnim.SetTrigger("MirrorReflect");

            // Add to damage absorbed for reflection. Returned to zero after reflected
            bossMechanic.absorbedDamage += value;
        }
        else
        {
            StartCoroutine(EnemyTakeDamage(source, value, physicalDamage, element));
        }

        healthBarSlider.value = currentHP;
    }

    // Since multi-hit attacks don't check mechanics until the last hit, this function can't be checked every time the enemy takes damage. Thus, check within BattleController
    public void CheckOnDamagedMechanics(Unit source, int value, bool physicalDamage, string element)
    {
        // Activate OnDamaged and OnLowHealth mechanics as necessary
        if (bossMechanic.activationType == "OnDamaged" || (bossMechanic.activationType == "OnLowHealth" && currentHP <= maxHP / 2))
        {
            bossMechanic.StartCoroutine(bossMechanic.ActivateMechanic(source, value, element));
        }
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

    // Reveal an enemy's health bar. This function does not currently do anything to bosses
    public void ScanEnemy()
    {
        /*
        if (healthBarSlider)
        {
            healthBarSlider.gameObject.SetActive(true);
            healthBarSlider.maxValue = maxHP;
            healthBarSlider.value = currentHP;
        }
        */
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

        // Boss stats scale 8% per level past 1, and 3% after level 20 to slow growth. HP always grows 15% per level (until level 20)
        for(int i = 2; i <= level; i++)
        {
            // HP grows until level 20 to prevent overinflation
            if(i <= 20)
            {
                maxHP = maxHP + (int)(maxHP * 0.15f);
            }

            // statGrowth controls the percentage that non-HP stats grow per level
            float statGrowth = 0.08f;

            // At level 20, stats start to grow slower
            if(i > 20)
            {
                statGrowth = 0.03f;
            }

            strength = strength + (int)(strength * statGrowth);
            magic = magic + (int)(magic * statGrowth);

            dexterity = dexterity + (int)(dexterity * statGrowth);
            faith = faith + (int)(faith * statGrowth);

            baseExpGranted = baseExpGranted + (int)(baseExpGranted * statGrowth);
        }

        // Since defenses are so low, they have to be increased manually (10% per level).
        physicalDefense = physicalDefense + (int)(0.1f * physicalDefense * level);
        magicalDefense = magicalDefense + (int)(0.1f * magicalDefense * level);

        // Ensure that HP reaches its max after the increase.
        currentHP = maxHP;
    }

    public void SetBoss(Boss newBoss, int chosenLevel)
    {
        bossBody = newBoss.bossBody;

        var newBossBody = Instantiate(bossBody, transform);
        newBossBody.transform.localScale = new Vector3(25f, 25f, 1f);
        newBossBody.transform.localPosition = Vector3.zero;
        bossAnim = newBossBody.GetComponent<Animator>();
        bossAnim.SetTrigger("BossEntry");

        //Look for extra animators within the boss. Should one exist, assign the boss's extraAnim.
        foreach (Animator anim in GetComponentsInChildren<Animator>())
        {
            if (anim.CompareTag("ExtraAnimator"))
            {
                extraAnim = anim;
            }
        }

        maxHP = newBoss.maxHP;
        strength = newBoss.strength;
        dexterity = newBoss.dexterity;
        magic = newBoss.magic;
        faith = newBoss.faith;

        physicalDefense = newBoss.physicalDefense;
        magicalDefense = newBoss.magicalDefense;

        baseExpGranted = newBoss.baseExpGranted;

        currentHP = maxHP;

        skillList = newBoss.skillList;
        desperationAbilities = newBoss.desperationAbilities;

        var copiedMechanic = Instantiate(newBoss.bossMechanic.gameObject, transform);
        bossMechanic = copiedMechanic.GetComponent<BossMechanic>();
        bossMechanic.mechanicOwner = this;

        unitName = newBoss.unitName;

        damageText = newBoss.damageText;

        resistancesList = newBoss.resistancesList;
        absorbancesList = newBoss.absorbancesList;
        weaknessesList = newBoss.weaknessesList;

        criticalHitCoefficient = newBoss.criticalHitCoefficient;
        bonusAttackPower = newBoss.bonusAttackPower;
        turnSpeedMultiplier = newBoss.turnSpeedMultiplier;

        //also find the health bar slider in the Enemy prefab
        healthBarSlider = GetComponentInChildren<Slider>(true);

        unitBuffs = GetComponentInChildren<UnitBuffs>(true);

        SetLevelAndStats(chosenLevel);

        ATB_TimeUntilAttack = 10f;
    }

    public IEnumerator EnemyAction()
    {
        //If living, act!
        if (currentHP > 0)
        {
            //freeze the ATB gauges of all other enemies while attacking.
            FindObjectOfType<BattleController>().ATB_Active = false;
            ATB_CurrentCharge = Random.Range(0.1f, ATB_TimeUntilAttack * 0.4f); // ATB fills up to 1/3 randomly to mix up turn order

            // First, if a boss's mechanic is activated each turn, then activate it
            if (bossMechanic.activationType == "OnTurn")
            {
                yield return StartCoroutine(bossMechanic.ActivateMechanic(null, 0, null));

                FindObjectOfType<BattleController>().ATB_Active = false;
            }

            Ability chosenAttack = null;

            if (currentHP > (maxHP / 2) || desperationAbilities.Count == 0) // Normal abilities
            {
                chosenAttack = skillList[Random.Range(0, skillList.Count)];
            }
            else // HP below 50%; desperation abilities
            {
                chosenAttack = desperationAbilities[Random.Range(0, desperationAbilities.Count)];
            }

            //time for unit to reach attack position
            yield return new WaitForSeconds(0.125f);

            FindObjectOfType<BattleController>().ActivateEnemyActionText(chosenAttack);

            bossAnim.SetTrigger("BossAttack");

            //time to read the enemy attack text and for the enemy to animate 1s in
            yield return new WaitForSeconds(1.25f);

            // if it's a heal or buff, always choose the boss themself
            if (chosenAttack.GetComponent<EnemySpecialAttack>() && chosenAttack.GetComponent<EnemySpecialAttack>().healingSpell)
            {
                chosenAttack.UseAbility(this, this);
            }
            else
            {
                //attack... NOTE THAT AREA ATTACKS ARE USED IN THIS CALC AS WELL... direct target only is used in the function if it needs one...
                chosenAttack.UseAbility(this, PickDirectTarget());

                //If it's a multi-hit attack, cast it as many times as necessary.
                if (chosenAttack.amountOfHits > 1)
                {
                    yield return new WaitForSeconds(0.25f); //wait a moment before casting again

                    for (int i = 1; i < chosenAttack.amountOfHits; i++)
                    {
                        bossAnim.SetTrigger("BossAttack");

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
        if(protectingList.Count > 0)
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

    public IEnumerator KillBoss()
    {
        bossAnim.SetTrigger("BossDead");

        //Remove the boss from all lists
        FindObjectOfType<BattleController>().RemoveBoss(this);
        
        //Wait 1.5f before destroying the object
        yield return new WaitForSeconds(2f);

        FindObjectOfType<BattleController>().StartCoroutine(FindObjectOfType<BattleController>().WinBattle());

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

            //crit rate is (50% of dex) and doubles damage (after reductions from defense/resistance)
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
        else if (weaknessesList.Contains(element)) // Unlike other units, bosses take 1.25x damage from weaknesses (instead of 2x)
        {
            value = (int)(value * 1.25f);
        }

        // Always take at least 1 damage
        if (value < 1)
        {
            value = 1;
        }

        // By default (with no source), the unit does not dodge. Allows for self-inflicted damage
        int dodgeRoll = 100;

        if (source != null && source.GetComponent<Character>())
        {
            //dodge takes the unit weapon's hit roll into account
            dodgeRoll = Random.Range(1, 101) + source.GetComponent<Character>().equippedWeapon.weaponHitMod;
        }

        //every 4 points of dexterity grants 1% dodge (essentially 0.25% per point). Note that Aura attacks cannot be dodged
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
                bossAnim.SetTrigger("BossHurt");

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
                else if (source != null && source.GetComponent<Summon>())
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

        // Update the battle hp/mp bars
        foreach (HeroStatus heroHP in FindObjectsOfType<HeroStatus>())
        {
            heroHP.UpdateHeroStatus();
        }

        yield return new WaitForSeconds(1f);

        if (currentHP == 0)
        {
            StartCoroutine(KillBoss());
        }

        yield return null;
    }
}
