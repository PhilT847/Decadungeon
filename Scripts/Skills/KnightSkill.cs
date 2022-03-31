public class KnightSkill : Ability
{
    //percentage of Strength applied to a skill's damage.
    public int baseAttackDamage;
    public float attackPotency;

    //For organization purposes, this class holds all skills associated with the Knight and related classes
    public bool areaOfEffect;
    public bool affectsAllies;

    public bool protectAllies;

    public bool appliesRally;

    public bool hexBlade; //Lich skill that deals bonus magic damage but costs 20% of the user's max HP.

    public Buff rallyBuff;

    // Records damage dealt each cast. Used for Whirlwind with enchantments
    private int damageDealt;

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
        damageDealt = 0;

        if (protectAllies)
        {
            user.GetComponent<Character>().protectingAllies = true;
            user.GetComponent<Character>().timeProtecting = 10f;

            //Animate the shield on the Knight class.
            user.GetComponent<Character>().extraAnim.ResetTrigger("EndBlock");
            user.GetComponent<Character>().extraAnim.SetTrigger("BeginBlock");

            GenerateEffectParticles(user.transform);
        }
        else if(attackPotency > 0)
        {
            if (!hexBlade) // Whirlwind
            {
                // Direct attacks hit the given Unit "target". Area weapons (whips) attack all living enemies
                if (!user.GetComponent<Character>().equippedWeapon.areaAttack)
                {
                    WhirlwindAttack(user, target);
                }
                else // AoE attack (whip)
                {
                    Enemy[] allEnemies = FindObjectsOfType<Enemy>();

                    for (int i = 0; i < allEnemies.Length; i++)
                    {
                        WhirlwindAttack(user, allEnemies[i]);
                    }

                    // AoE spells also affect the Boss unit
                    if (FindObjectsOfType<Boss>().Length > 0)
                    {
                        WhirlwindAttack(user, FindObjectOfType<Boss>());
                    }
                }

                // Heal or MP restore after attacking
                if (user.GetComponent<Character>().weaponEnchant.enchantmentName == "Holy")
                {
                    user.HealUnit(user, damageDealt / 2);
                }
                else if (user.GetComponent<Character>().weaponEnchant.enchantmentName == "Aura")
                {
                    user.GetComponent<Character>().RestoreMP(damageDealt / 4);
                }

                // If the weapon has an upgraded form, reduce the time needed for it to upgrade based on damage dealt
                if (user.GetComponent<Character>().equippedWeapon.upgradedForm != null)
                {
                    user.GetComponent<Character>().equippedWeapon.damageUntilUpgrade -= damageDealt;

                    if (user.GetComponent<Character>().equippedWeapon.damageUntilUpgrade < 1)
                    {
                        user.GetComponent<Character>().equippedWeapon.StartCoroutine(user.GetComponent<Character>().equippedWeapon.TransformWeapon(user.GetComponent<Character>()));
                    }
                }
            }
            else
            {
                bool hitTarget = false;
                int targetOriginalHP = target.currentHP;

                GenerateEffectParticles(target.transform);

                target.TakeDamage(user, baseAttackDamage + (int)(user.GetComponent<Character>().CharacterAttackPower() * attackPotency) , false, "Aura");

                hitTarget = targetOriginalHP > target.currentHP;

                //On hit, Hexblade deals 25% max HP to the user. Cannot reduce HP below 1.
                if (hitTarget)
                {
                    if (user.currentHP > user.maxHP / 4)
                    {
                        user.TakeDamage(null, user.maxHP / 4, false, "Aura");
                    }
                    else if (user.currentHP > 1)
                    {
                        user.TakeDamage(null, user.currentHP - 1, false, "Aura");
                    }
                }
            }
        }
        else if (appliesRally) //Rally applies the Rally (+atk, +def) buff to all allies.
        {
            Character[] allCharacters = FindObjectsOfType<Character>();

            for (int i = 0; i < allCharacters.Length; i++)
            {
                GenerateEffectParticles(allCharacters[i].transform);

                rallyBuff.ApplyBuff(allCharacters[i]);
            }
        }
    }

    void WhirlwindAttack(Unit attacker, Unit target)
    {
        // If enchanted, use the particles from the enchant. Otherwise, use the particles associated with the weapon
        if (attacker.GetComponent<Character>().weaponEnchant.overrideEnchantParticles != null)
        {
            GenerateEffectParticles(target.transform, attacker.GetComponent<Character>().weaponEnchant.overrideEnchantParticles);
        }
        else
        {
            GenerateEffectParticles(target.transform, attacker.GetComponent<Character>().equippedWeapon.weaponParticles);
        }

        //The element can be changed by enchanting.
        string attackElement = attacker.GetComponent<Character>().equippedWeapon.weaponElement;

        int enchantStrength = 0;

        //Before attacking, check the unit's enchantment.
        switch (attacker.GetComponent<Character>().weaponEnchant.enchantmentName)
        {
            case "Fire": //Fire increases power by 50% of strength or magic (whichever is higher).

                if (attacker.strength > attacker.magic)
                {
                    enchantStrength = attacker.strength / 2;
                }
                else
                {
                    enchantStrength = attacker.magic / 2;
                }

                attackElement = "Fire";
                break;
            case "Holy":
                enchantStrength = 0;
                attackElement = "Holy";
                break;
            case "Aura":
                enchantStrength = 0;
                attackElement = "Aura";
                break;
        }

        int targetOriginalHP = target.currentHP;

        //non-Monk classes calculate attack power through strength + weapon power. Monks use strength and their level * 2. See CharacterAttackPower().
        //If the element is physical, it's a physical attack. Otherwise, it's magical and uses the associated element.

        target.TakeDamage(attacker, (int) ((attacker.GetComponent<Character>().CharacterAttackPower() + enchantStrength) * attackPotency), attackElement == "Physical", attackElement);

        // Increase the value of damageDealt in the UseAbility function. Used for weapon transformations and enchantment effects
        damageDealt += (targetOriginalHP - target.currentHP);
    }
}
