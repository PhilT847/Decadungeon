using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonSpell : Ability
{
    public int potencyBase; // Base damage/healing
    public float potencyGrowth; // How much this spell "grows" in potency based on the casting stat (Magic, Strength, Faith...)

    public bool areaOfEffect;
    public bool supportSpell; //affects allies (healing them) or granting buffs
    public Buff phoenixBuff; //revives allies and cleanses debuffs

    public override void SelectAbility(Unit caster)
    {
        // Summons do not have to select any abilities; they cast automatically. This function never runs.
    }

    public override void UseAbility(Unit user, Unit target)
    {
        // If it's an area of effect spell, apply to all enemies/characters as necessary
        if (areaOfEffect)
        {
            if (supportSpell)
            {
                Character[] allCharacters = FindObjectsOfType<Character>();

                for (int i = 0; i < allCharacters.Length; i++)
                {
                    ApplyEffect(user, allCharacters[i]);
                }
            }
            else
            {
                Enemy[] allEnemies = FindObjectsOfType<Enemy>();

                for (int i = 0; i < allEnemies.Length; i++)
                {
                    ApplyEffect(user, allEnemies[i]);
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

        if (supportSpell)
        {
            // Support spells heal if they have any potency and the target is alive (So they're not randomly revived)
            if ((potencyBase > 0 || potencyGrowth > 0) && target.currentHP > 0)
            {
                target.HealUnit(caster, potencyBase + (int)(caster.faith * potencyGrowth));
            }

            if (phoenixBuff != null) // Phoenix's spell revives allies, cleanses debuffs, and adds an ATK buff
            {
                if(target.currentHP == 0) // Dead; revive!
                {
                    target.GetComponent<Character>().ReviveUnit(1);
                }

                // Cleanse debuffs and apply Phoenix Fire buff
                target.GetComponent<Character>().CleanseDebuffs();
                phoenixBuff.ApplyBuff(target);
            }
        }
        else
        {
            // Split between physical (strength-based) and magical (magic-based) skills
            if (associatedElement == "Physical")
            {
                target.TakeDamage(caster, potencyBase + (int)(caster.strength * potencyGrowth), true, associatedElement);
            }
            else
            {
                target.TakeDamage(caster, potencyBase + (int)(caster.magic * potencyGrowth), false, associatedElement);
            }
        }
    }
}
