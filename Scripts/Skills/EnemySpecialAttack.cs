using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpecialAttack : Ability
{
    //For organization purposes, this class holds all skills associated with enemies.
    public int potencyBase; //damage/healing base
    public float potencyGrowth;

    public bool areaOfEffect;

    public bool magicDamage; //magic damage targets the target's magical defense and grows based on the unit's Magic. Otherwise, grow based on Strength.

    public bool healingSpell; //if it targets allies, then it's a healing spell that uses the unit's Faith as growth.

    public bool leechDamage; //if true, leech 50% of damage dealt.

    public int ConvertedMPCost; //the MP cost of this ability if it's converted to Wild Magic by a Druid.

    public Buff potentialBuff;
    public int buffApplicationChance;

    public override void SelectAbility(Unit caster)
    {
        //only enemies use this kind of ability, so leave this function empty.
    }

    public override void UseAbility(Unit user, Unit target)
    {
        if (areaOfEffect)
        {
            if (healingSpell)
            {
                Enemy[] allEnemies = FindObjectsOfType<Enemy>();

                for (int i = 0; i < allEnemies.Length; i++)
                {
                    ApplyEffect(user, allEnemies[i]);
                }

                // AoE spells also affect the Boss unit
                if (FindObjectsOfType<Boss>().Length > 0)
                {
                    ApplyEffect(user, FindObjectOfType<Boss>());
                }
            }
            else
            {
                Character[] allCharacters = FindObjectsOfType<Character>();

                for (int i = 0; i < allCharacters.Length; i++)
                {
                    ApplyEffect(user, allCharacters[i]);
                }
            }
        }
        else
        {
            ApplyEffect(user, target);
        }
    }
    
    void ApplyEffect(Unit caster, Unit target)
    {
        GenerateEffectParticles(target.transform);

        //status only applied if the attack actually hits.
        bool hitEnemy = false;

        //store original/final HP in case the enemy leeches damage dealt. Also used to check if hit.
        int targetOriginalHP = target.currentHP;

        if (healingSpell) //healing or buffing
        {
            if(potencyBase > 0)
            {
                target.HealUnit(caster, potencyBase + (int)(caster.faith * potencyGrowth));
            }

            hitEnemy = true;
        }
        else if (potencyBase > 0 || potencyGrowth > 0) //magic or physical attack... only deal damage if the potency/growth is greater than 0
        {
            if (magicDamage)
            {
                if (abilityName == "Heartburn") //Heartburn deals damage based on 50% of HP (100% when learned as Wild Magic).
                {
                    potencyBase = caster.currentHP / 2;
                }

                // Psycho Δ's base damage is equal to the target's Magic * 2
                if (abilityName == "Psycho H")
                {
                    potencyBase = (int)(target.magic * 2f);
                }

                target.TakeDamage(caster, potencyBase + (int)(caster.magic * potencyGrowth), false, associatedElement);

                //leech 50% of damage dealt if applicable.
                int leechValue = (targetOriginalHP - target.currentHP) / 2;

                //if the enemy's HP is reduced, counts as a hit.
                hitEnemy = targetOriginalHP > target.currentHP;

                if (leechDamage && hitEnemy)
                {
                    caster.HealUnit(caster, leechValue);
                }

                if (abilityName == "Squeal!") // Squeal! Also kills the user
                {
                    caster.GetComponent<Enemy>().StartCoroutine(caster.GetComponent<Enemy>().EnemyTakeDamage(null, 99, false, "Aura"));
                }
            }
            else // Physical damage
            {
                target.TakeDamage(caster, potencyBase + (int)(caster.strength * potencyGrowth), true, associatedElement);

                //leech 50% of damage dealt if applicable.
                int leechValue = (targetOriginalHP - target.currentHP) / 2;

                //if the enemy's HP is reduced, counts as a hit.
                hitEnemy = targetOriginalHP > target.currentHP;

                if (leechDamage && hitEnemy)
                {
                    caster.HealUnit(caster, leechValue);
                }
            }
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
        else if (abilityName== "Smog") //Smog removes all positive buffs from the target.
        {
            for (int i = 0; i < target.unitBuffs.allBuffs.Count; i++)
            {
                Buff b = target.unitBuffs.allBuffs[i];

                if (b.attackMod > 0 || b.defenseMod > 0 || b.dexMod > 0 || b.turnSpeedMod > 0)
                {
                    b.RemoveBuff();
                    target.unitBuffs.allBuffs.Remove(b);
                    i--;
                }
            }
        }
    }
}
