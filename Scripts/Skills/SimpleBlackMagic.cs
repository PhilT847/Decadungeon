using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBlackMagic : Spell
{
    public int potencyBase; //base damage/healing for a spell.
    public float potencyGrowth; //percentage of Magic that is added to spell potency.

    public bool areaOfEffect;

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

    //creating ApplyEffect allows for easier reading when applying white magic's various effects.
    void ApplyEffect(Unit caster, Unit target)
    {
        GenerateEffectParticles(target.transform);

        bool hitEnemy = false;

        //store original HP of the target in case the spell leeches for damage dealt.
        int targetOriginalHP = target.currentHP;

        if (potencyBase > 0)
        {
            //Most spells use unit's Magic stat... Arcane instead deals damage based on the enemy's Magic stat. Max 5 MP restored.
            if(abilityName != "Arcane")
            {
                target.TakeDamage(caster, potencyBase + (int)(caster.magic * potencyGrowth), false, associatedElement);

                hitEnemy = targetOriginalHP > target.currentHP;
            }
            else
            {
                // Arcane also generates particles on the caster
                GenerateEffectParticles(caster.transform);

                target.TakeDamage(caster, target.magic / 2, false, "Aura");

                hitEnemy = targetOriginalHP > target.currentHP;

                if (hitEnemy)
                {
                    if(target.magic <= 20) // Up to Magic = 20, restore 25% of their Magic
                    {
                        caster.GetComponent<Character>().RestoreMP(target.magic / 4);
                    }
                    else // MP restore is capped at 5
                    {
                        caster.GetComponent<Character>().RestoreMP(5);
                    }
                }
            }
        }

        //If there's a buff, roll to see if it's applied.
        if(potentialBuff != null)
        {
            int buffRoll = Random.Range(1, 101);

            if(buffRoll <= buffApplicationChance && hitEnemy)
            {
                potentialBuff.ApplyBuff(target);
            }
        }
    }
}
