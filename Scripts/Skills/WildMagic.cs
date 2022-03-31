using UnityEngine;

public class WildMagic : Spell
{
    //This class creates and uses wild magic taken from enemy skill lists.

    public int potencyBase; //damage/healing base
    public float potencyGrowth;

    public bool areaOfEffect;

    public bool damageSpell; //either a damage spell (str/mag) or a healing spell (fth, targets allies).
    public bool magicDamage; //magic damage targets the target's magical defense and grows based on the unit's Magic. Otherwise, grow based on Strength.

    public bool leechDamage; //if true, leech 50% of damage dealt.

    public Buff potentialBuff;
    public int buffApplicationChance;

    //Convert an enemy skill into a usable spell.
    public void ConvertToWildMagic(EnemySpecialAttack enemySkill)
    {
        //don't change the ability icon; baseline, it's the green orb. Otherwise, copy the spell and add an MP cost.
        abilityName = enemySkill.abilityName;
        abilityCode = enemySkill.abilityCode;
        abilityDescription = enemySkill.abilityDescription;
        associatedElement = enemySkill.associatedElement;

        potencyBase = enemySkill.potencyBase;
        potencyGrowth = enemySkill.potencyGrowth;

        // Squeal!'s damage is different in the hands of a Mimic. It instead deals 10 + (1.5x Magic) damage so that it's useful at later levels
        if (abilityName == "Squeal!")
        {
            potencyBase = 10;
            potencyGrowth = 1.5f;
        }

        areaOfEffect = enemySkill.areaOfEffect;

        chanceOfMultihit = enemySkill.chanceOfMultihit;
        amountOfHits = enemySkill.amountOfHits;

        damageSpell = !enemySkill.healingSpell; //damage is opposite of healing
        magicDamage = enemySkill.magicDamage;

        leechDamage = enemySkill.leechDamage;

        potentialBuff = enemySkill.potentialBuff;
        buffApplicationChance = enemySkill.buffApplicationChance;

        MP_Cost = enemySkill.ConvertedMPCost;

        castableInMenu = enemySkill.castableInMenu;

        effectParticles = enemySkill.effectParticles;
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
            ApplyEffect(caster, target);
        }
    }

    void ApplyEffect(Unit caster, Unit target)
    {
        GenerateEffectParticles(target.transform);

        //status only applied if the attack actually hits.
        bool hitEnemy = false;

        //store original/final HP in case the enemy leeches damage dealt. Also used to check if hit.
        int targetOriginalHP = target.currentHP;

        if (!damageSpell) //healing
        {
            if (potencyBase > 0 || potencyGrowth > 0) // Must have potency or growth to apply healing
            {
                target.HealUnit(caster, potencyBase + (int)(caster.faith * potencyGrowth));
            }

            hitEnemy = true;
        }
        else if (potencyBase > 0 || potencyGrowth > 0) // Magic or physical attack
        {
            if (magicDamage)
            {
                if (abilityName == "Heartburn") // Heartburn deals damage based on HP
                {
                    potencyBase = caster.currentHP;
                }

                // Psycho Δ's base damage is equal to the target's Magic * 2 (plus 50% of caster's Magic in potencyGrowth)
                if (abilityName == "Psycho H")
                {
                    potencyBase = (int) (target.magic * 2f);
                }

                if (potencyBase > 0 || potencyGrowth > 0)
                {
                    target.TakeDamage(caster, potencyBase + (int)(caster.magic * potencyGrowth), false, associatedElement);
                }

                // Leech 50% of damage dealt if applicable.
                int leechValue = (targetOriginalHP - target.currentHP) / 2;

                // If the enemy's HP is reduced, counts as a hit.
                hitEnemy = targetOriginalHP > target.currentHP;

                if (leechDamage)
                {
                    caster.HealUnit(caster, leechValue);
                }

                if (abilityName == "Squeal!") // Squeal! Kills its user
                {
                    GenerateEffectParticles(caster.transform);
                    caster.TakeDamage(null, 999, false, "Aura");
                }
            }
            else // Physical attack
            {
                target.TakeDamage(caster, potencyBase + (int)((caster.strength + caster.GetComponent<Character>().equippedWeapon.weaponMight) * potencyGrowth), true, "Physical");

                //leech 50% of damage dealt if applicable.
                int leechValue = (targetOriginalHP - target.currentHP) / 2;

                if (leechDamage)
                {
                    caster.HealUnit(caster, leechValue);
                }

                //if the enemy's HP is reduced, counts as a hit.
                hitEnemy = targetOriginalHP > target.currentHP;
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
