public class Attack : Ability
{
    private int damageDealt; // The damage dealt by an attack. Resets each attack

    public override void UseAbility(Unit user, Unit target)
    {
        damageDealt = 0;

        if (user.GetComponent<Character>())
        {
            // Direct attacks hit the given Unit "target". Area weapons (whips) attack all living enemies
            if (!user.GetComponent<Character>().equippedWeapon.areaAttack)
            {
                AttackEnemy(user, target);
            }
            else // AoE attack (whip)
            {
                Enemy[] allEnemies = FindObjectsOfType<Enemy>();

                for (int i = 0; i < allEnemies.Length; i++)
                {
                    AttackEnemy(user, allEnemies[i]);
                }

                // AoE spells also affect the Boss unit
                if (FindObjectsOfType<Boss>().Length > 0)
                {
                    AttackEnemy(user, FindObjectOfType<Boss>());
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
            GenerateEffectParticles(target.transform);

            target.TakeDamage(user, user.strength, associatedElement == "Physical", associatedElement);
        }
    }

    public void AttackEnemy(Unit attacker, Unit target)
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

        target.TakeDamage(attacker, attacker.GetComponent<Character>().CharacterAttackPower() + enchantStrength, attackElement == "Physical", attackElement);

        // Increase the value of damageDealt in the UseAbility function. Used for weapon transformations and enchantment effects
        damageDealt += (targetOriginalHP - target.currentHP);

        // Monks gain 1 Chi when they attack, unless they're in phase mode. See GainChi()
        if(attacker.GetComponent<Character>().chosenClass.className == "Monk")
        {
            attacker.GetComponent<Character>().GainChi(1);
        }
    }

    public override void SelectAbility(Unit caster)
    {
        if(caster.GetComponent<Character>() && caster.GetComponent<Character>().equippedWeapon.areaAttack)
        {
            FindObjectOfType<BattleController>().DisplayAreaEnemyButton();
        }
        else
        {
            FindObjectOfType<BattleController>().DisplayEnemyButtons();
        }
    }
}
