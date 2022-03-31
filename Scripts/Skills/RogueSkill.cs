using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RogueSkill : Ability
{
    //For organization purposes, this class holds all skills associated with the Knight and related classes
    public bool areaOfEffect;
    public bool affectsAllies;

    public int baseAttackDamage;
    public float attackPotency; //growth based on attack stat.

    public Buff addedDebuff;
    public Buff hasteBuff;

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
        bool hitEnemy = false;
        int targetOriginalHP = 0;

        switch (abilityName)
        {
            case "Acrobatics": // Increase ATB timer speed and boost dodge to 75% for a time. Includes direct and area of effect damage.
                user.GetComponent<Character>().evadingAttacks = true;
                user.GetComponent<Character>().timeEvading = 10f;
                hasteBuff.ApplyBuff(user); // Apply the haste buff
                GenerateEffectParticles(user.transform);

                user.GetComponent<Character>().extraAnim.ResetTrigger("EndEvasion");
                user.GetComponent<Character>().extraAnim.SetTrigger("BeginEvasion");

                break;

            case "Spy": // Spy command reveals all enemy health bars

                Enemy[] allEnemies = FindObjectsOfType<Enemy>();

                for (int i = 0; i < allEnemies.Length; i++)
                {
                    allEnemies[i].ScanEnemy();
                    GenerateEffectParticles(allEnemies[i].transform);
                }

                // AoE spells also affect the Boss unit
                if (FindObjectsOfType<Boss>().Length > 0)
                {
                    FindObjectOfType<Boss>().ScanEnemy();
                    GenerateEffectParticles(FindObjectOfType<Boss>().transform);
                }

                break;

            case "Caltrops": // Caltrops damages all enemies and reduces their speed

                Enemy[] caltropEnemies = FindObjectsOfType<Enemy>();

                for (int i = 0; i < caltropEnemies.Length; i++)
                {
                    GenerateEffectParticles(caltropEnemies[i].transform);

                    targetOriginalHP = caltropEnemies[i].currentHP;

                    caltropEnemies[i].TakeDamage(user, baseAttackDamage + (int)(user.GetComponent<Character>().CharacterAttackPower() * attackPotency), true, "Physical");

                    hitEnemy = targetOriginalHP > caltropEnemies[i].currentHP;

                    if (hitEnemy)
                    {
                        // If it hit, add the debuff
                        addedDebuff.ApplyBuff(caltropEnemies[i]);
                    }
                }

                // AoE spells also affect the Boss unit
                if (FindObjectsOfType<Boss>().Length > 0)
                {
                    GenerateEffectParticles(FindObjectOfType<Boss>().transform);

                    targetOriginalHP = FindObjectOfType<Boss>().currentHP;

                    FindObjectOfType<Boss>().TakeDamage(user, baseAttackDamage + (int)(user.GetComponent<Character>().CharacterAttackPower() * attackPotency), true, "Physical");

                    hitEnemy = targetOriginalHP > FindObjectOfType<Boss>().currentHP;

                    if (hitEnemy)
                    {
                        // If it hit, add the debuff
                        addedDebuff.ApplyBuff(FindObjectOfType<Boss>());
                    }
                }

                break;
            case "Poison Dart": // Poison Dart reduces enemy defenses. It also deals 1 damage

                GenerateEffectParticles(target.transform);

                targetOriginalHP = target.currentHP;

                target.TakeDamage(user, 1, true, "Physical");

                hitEnemy = targetOriginalHP > target.currentHP;

                if (hitEnemy)
                {
                    // If it hit, add the debuff
                    addedDebuff.ApplyBuff(target);
                }

                break;
        }
    }
}
