using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteMagic : Spell
{
    public int potencyBase; //base damage/healing for a spell.
    public float potencyGrowth; //percentage of Magic that is added to spell potency.

    public bool damageSpell;

    public bool appliesRegen;
    public bool appliesBarrier;
    public bool revivesAllies;
    public bool areaOfEffect;

    public override void UseAbility(Unit user, Unit target)
    {
        CastSpell(user, target);

        //update the battle hp/mp bars to ensure they're affected by the spell
        foreach (HeroStatus heroHP in FindObjectsOfType<HeroStatus>())
        {
            heroHP.UpdateHeroStatus();
        }
    }

    public override void SelectAbility(Unit caster)
    {
        //if it's area of effect, the prompt covers all units. If single-target, prompt the user to select a target.
        if (areaOfEffect)
        {
            if (!damageSpell)
            {
                FindObjectOfType<BattleController>().DisplayAreaAllyButton();
            }
            else
            {
                FindObjectOfType<BattleController>().DisplayAreaEnemyButton();
            }
        }
        else
        {
            if (!damageSpell)
            {
                FindObjectOfType<BattleController>().DisplayUnitButtons();
            }
            else
            {
                FindObjectOfType<BattleController>().DisplayEnemyButtons();
            }
        }
    }

    public override void CastSpell(Unit caster, Unit target)
    {
        caster.currentMP -= MP_Cost;

        if (areaOfEffect)
        {
            if (caster.GetComponent<Character>())
            {
                if (damageSpell)
                {
                    Enemy[] allEnemies = FindObjectsOfType<Enemy>();

                    for (int i = 0; i < allEnemies.Length; i++)
                    {
                        ApplyEffect(caster, allEnemies[i]);
                    }

                    // AoE spells also affect the Boss unit
                    if (FindObjectsOfType<Boss>().Length > 0)
                    {
                        ApplyEffect(caster, FindObjectOfType<Boss>());
                    }
                }
                else
                {
                    Character[] allCharacters = FindObjectsOfType<Character>();

                    for (int i = 0; i < allCharacters.Length; i++)
                    {
                        ApplyEffect(caster, allCharacters[i]);
                    }
                }
            }
            else
            {
                if (damageSpell)
                {
                    Character[] allCharacters = FindObjectsOfType<Character>();

                    for (int i = 0; i < allCharacters.Length; i++)
                    {
                        ApplyEffect(caster, allCharacters[i]);
                    }
                }
                else
                {
                    Enemy[] allEnemies = FindObjectsOfType<Enemy>();

                    for (int i = 0; i < allEnemies.Length; i++)
                    {
                        ApplyEffect(caster, allEnemies[i]);
                    }

                    // AoE spells also affect the Boss unit
                    if (FindObjectsOfType<Boss>().Length > 0)
                    {
                        ApplyEffect(caster, FindObjectOfType<Boss>());
                    }
                }
            }

        }
        else
        {
            ApplyEffect(caster, target);
        }
    }

    //creating ApplyEffect allows for easier reading when applying white magic's various effects.
    void ApplyEffect(Unit caster, Unit target)
    {
        GenerateEffectParticles(target.transform);

        if (damageSpell)
        {
            // Undead units with Holy weakness take tons of damage from Banish; other enemies take just 1.
            if(target.weaknessesList.Contains("Holy"))
            {
                // Counted as Aura damage so it's not resisted in any way; note that it removes the double-damage due to Holy weakness
                target.TakeDamage(caster, potencyBase + (int)(caster.faith * potencyGrowth), false, "Aura");
            }
            else
            {
                target.TakeDamage(caster, 1, false, associatedElement);
            }
        }
        else
        {
            if(potencyBase > 0 && !revivesAllies)
            {
                target.HealUnit(caster, potencyBase + (int)(caster.faith * potencyGrowth));
            }

            if (appliesBarrier)
            {
                if (target.currentHP > 0)
                {
                    target.GetComponent<Character>().barrierTimeRemaining = 15f;

                    //clerics grant barriers that prevent (20% + 0.5% per Faith, max 40%) damage for 3 turns... maxed at 40 Faith
                    target.GetComponent<Character>().barrierGuardMult = 1f - (0.2f + (caster.faith * 0.005f));

                    //if the guard is better than 40%, reduce it to 40%.
                    if (target.GetComponent<Character>().barrierGuardMult < 0.6f)
                    {
                        target.GetComponent<Character>().barrierGuardMult = 0.6f;
                    }

                    //Display the unit's barrier that also shows the percentage of damage blocked.
                    target.GetComponent<Character>().chosenClass.barrierObject.GetComponent<Animator>().SetTrigger("BarrierUp");
                    target.GetComponent<Character>().chosenClass.barrierText.SetText("{0:0}%", 100f - (target.GetComponent<Character>().barrierGuardMult * 100f));
                }
                else
                {
                    target.PrintDamageText("MISS");
                }
            }

            //Apply a regen amount based on spell potency for 20s.
            if (appliesRegen && target.currentHP > 0)
            {
                //If the target is already affected by a stronger Regen, it just reapplies a Regen of that power. Note that it must be currently active
                int intendedRegenValue = 3 + (int)(caster.faith * 0.25f);

                if (target.GetComponent<Character>().regenTimeRemaining > 0f 
                    && target.GetComponent<Character>().regenHealValue > intendedRegenValue)
                {
                    // Swap to their current regen value to reapply it
                    intendedRegenValue = target.GetComponent<Character>().regenHealValue;
                }

                // Apply regen for a guaranteed five ticks by granting 20 (four ticks) plus the amount needed to reach a fifth tick
                target.GetComponent<Character>().regenTimeRemaining = 20f + target.GetComponent<Character>().timeUntilRegenTick;
                //target.GetComponent<Character>().timeUntilRegenTick = 2f;

                target.GetComponent<Character>().regenHealValue = intendedRegenValue;

                target.GetComponent<Character>().unitBuffs.UpdateBuffBar(target);

                // Animate the buff bar to show this new buff
                target.GetComponent<Character>().unitBuffs.GetComponent<Animator>().SetTrigger("ShowBuffs");
            }

            //Reviving allies increases their HP to the spell potency value.
            if (revivesAllies)
            {
                if(abilityName!= "Miracle")
                {
                    target.GetComponent<Character>().ReviveUnit(potencyBase + (int)(caster.faith * potencyGrowth));
                }
                else //The "Prayer" spell can either heal or revive based on allied HP.
                {
                    if (target.currentHP == 0) //dead allies are revived. living allies are healed.
                    {
                        target.GetComponent<Character>().ReviveUnit(potencyBase + (int)(caster.faith * potencyGrowth));
                        //revive healing adds to the healer's lifetime healing dealt.
                        caster.GetComponent<Character>().lifetimeHealingDealt += potencyBase + (int)(caster.faith * potencyGrowth);
                    }
                    else
                    {
                        target.HealUnit(caster, potencyBase + (int)(caster.faith * potencyGrowth));
                    }
                }
            }
        }
    }
}
