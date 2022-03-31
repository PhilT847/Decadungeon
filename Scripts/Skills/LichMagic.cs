using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LichMagic : Spell
{
    public int potencyBase; //base damage/healing for a spell.
    public float potencyGrowth; //percentage of Magic that is added to spell potency.

    public bool areaOfEffect;

    public float drainPercentage; //percentage of damage dealt that is healed back to the unit.

    public Buff potentialBuff;
    public int buffApplicationChance; //chance from 1-100

    public override void SelectAbility(Unit caster)
    {
        //if it's area of effect, the prompt covers all enemies. If single-target, prompt the user to select a target.
        if (areaOfEffect)
        {
            FindObjectOfType<BattleController>().DisplayAreaEnemyButton();
        }
        else
        {
            FindObjectOfType<BattleController>().DisplayEnemyButtons();
        }
    }

    public override void UseAbility(Unit user, Unit target)
    {
        CastSpell(user, target);

        //update the battle hp/mp bars to ensure they're affected by the spell
        foreach (HeroStatus heroHP in FindObjectsOfType<HeroStatus>())
        {
            heroHP.UpdateHeroStatus();
        }
    }

    public override void CastSpell(Unit caster, Unit target)
    {
        caster.currentMP -= MP_Cost;

        if (areaOfEffect)
        {
            if (caster.GetComponent<Character>())
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
            ApplyEffect(caster, target);
        }
    }

    //creating ApplyEffect allows for easier reading when applying magic's various effects.
    void ApplyEffect(Unit caster, Unit target)
    {
        GenerateEffectParticles(target.transform);

        //store original HP of the target in case the spell leeches for damage dealt.
        int targetOriginalHP = target.currentHP;

        target.TakeDamage(caster, potencyBase + (int)(caster.magic * potencyGrowth), false, associatedElement);

        int amountDrained = targetOriginalHP - target.currentHP;

        bool hitEnemy = targetOriginalHP > target.currentHP;

        //Drain and Draintu heal the caster.
        if (drainPercentage > 0f && hitEnemy)
        {
            caster.HealUnit(caster, (int)(amountDrained * drainPercentage));
        }

        //If there's a buff, roll to see if it's applied.
        if (potentialBuff != null)
        {
            int buffRoll = Random.Range(1, 101);

            if (buffRoll <= buffApplicationChance && hitEnemy)
            {
                potentialBuff.ApplyBuff(target);
            }
        }
    }
}
