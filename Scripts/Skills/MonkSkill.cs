using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonkSkill : Ability
{
    public int baseAttackDamage;
    public int attackPotency;

    public bool affectsAllies;
    public bool areaOfEffect;

    // Phase Shift buffs ATB time based on Chi spent
    public Buff phaseBuff1;
    public Buff phaseBuff2;
    public Buff phaseBuff3;

    public override void SelectAbility(Unit caster)
    {
        //if it's area of effect, the prompt covers all units. If single-target, prompt the user to select a target.
        if (areaOfEffect)
        {
            if (affectsAllies)
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
            if (affectsAllies)
            {
                FindObjectOfType<BattleController>().DisplayUnitButtons();
            }
            else
            {
                FindObjectOfType<BattleController>().DisplayEnemyButtons();
            }
        }
    }

    public override void UseAbility(Unit user, Unit target)
    {
        // Spend chi
        int chiSpent = user.GetComponent<Character>().currentChi;
        user.GetComponent<Character>().currentChi = 0;

        // Chi Heal restores a % of max HP. Also removes debuffs
        if (abilityName == "Chi Heal")
        {
            GenerateEffectParticles(user.transform);

            // Heal 25/50/100% max HP based on Chi level
            switch (chiSpent)
            {
                case 1:
                    user.HealUnit(user, (int)(user.maxHP * 0.25f));
                    break;
                case 2:
                    user.HealUnit(user, (int)(user.maxHP * 0.5f));
                    break;
                case 3:
                    user.HealUnit(user, (int)(user.maxHP * 1f));
                    break;
            }

            // Cleanse debuffs
            user.GetComponent<Character>().CleanseDebuffs();
        }
        else if(abilityName == "Takedown") // Drains a percentage of the enemy's current HP
        {
            GenerateEffectParticles(target.transform);

            float hpPercentage = 0f;

            // Deals 20/40/60% current HP to an enemy; halved against bosses
            switch (chiSpent)
            {
                case 1:
                    hpPercentage = 0.2f;
                    break;
                case 2:
                    hpPercentage = 0.4f;
                    break;
                case 3:
                    hpPercentage = 0.6f;
                    break;
            }

            // Effect is halved on bosses (10/20/40)
            if (target.GetComponent<Boss>())
            {
                hpPercentage /= 2f;
            }

            target.TakeDamage(user, (int) (target.currentHP * hpPercentage), false, "Aura");
        }
        else if(abilityName == "Phase Shift") // Phase Shift grants a buff
        {
            GenerateEffectParticles(user.transform);

            switch (chiSpent)
            {
                case 1:
                    phaseBuff1.ApplyBuff(user);
                    break;
                case 2:
                    phaseBuff2.ApplyBuff(user);
                    break;
                case 3:
                    phaseBuff3.ApplyBuff(user);
                    break;
            }
        }
        else if(abilityName == "Spirit Wave") // Spirit Wave deals Aura damage to all enemies. A simple finisher
        {
            int chiPotency = 0;

            // Deal 20/30/40(+Faith) Aura damage based on chi level
            switch (chiSpent)
            {
                case 1:
                    chiPotency = 20 + user.faith;
                    break;
                case 2:
                    chiPotency = 30 + (int)(user.faith * 1.5f);
                    break;
                case 3:
                    chiPotency = 40 + user.faith * 2;
                    break;
            }

            Enemy[] allEnemies = FindObjectsOfType<Enemy>();

            foreach(Enemy e in allEnemies)
            {
                GenerateEffectParticles(e.transform);
                e.TakeDamage(user, chiPotency, true, "Aura");
            }

            if (FindObjectOfType<Boss>())
            {
                FindObjectOfType<Boss>().TakeDamage(user, chiPotency, true, "Aura");
            }
        }

        // Update the battle hp/mp/atb bars.
        foreach (HeroStatus heroHP in FindObjectsOfType<HeroStatus>())
        {
            heroHP.UpdateHeroStatus();
        }
    }
}
