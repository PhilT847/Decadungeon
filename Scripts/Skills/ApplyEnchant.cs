using UnityEngine;

public class ApplyEnchant : Ability
{
    public string nameOfEnchant;
    public Color enchantmentColor;

    public bool areaOfEffect;

    // Particles when this unit attacks with their enchanted weapon
    public ParticleSystem appliedAttackParticles;

    public override void UseAbility(Unit user, Unit target)
    {
        if (areaOfEffect)
        {
            Character[] allCharacters = FindObjectsOfType<Character>();

            for (int i = 0; i < allCharacters.Length; i++)
            {
                allCharacters[i].weaponEnchant.EnchantWeapon(this);

                GenerateEffectParticles(allCharacters[i].transform);
            }
        }
        else
        {
            target.GetComponent<Character>().weaponEnchant.EnchantWeapon(this);

            GenerateEffectParticles(target.transform);
        }
    }

    public override void SelectAbility(Unit caster)
    {
        if (areaOfEffect)
        {
            FindObjectOfType<BattleController>().DisplayAreaAllyButton();
        }
        else
        {
            FindObjectOfType<BattleController>().DisplayUnitButtons();
        }
    }
}
