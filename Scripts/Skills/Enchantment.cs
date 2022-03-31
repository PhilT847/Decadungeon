using UnityEngine;

public class Enchantment : MonoBehaviour
{
    public string enchantmentName;
    public ParticleSystem enchantParticles;
    public ParticleSystem enchantRingParticles;

    // Particles played when attacking with an enchanted weapon
    public ParticleSystem overrideEnchantParticles;

    public void ClearEnchantments()
    {
        enchantParticles.Stop();
        enchantmentName = "";

        // Return to the usual attack particles
        overrideEnchantParticles = null;
    }

    public void EnchantWeapon(ApplyEnchant newEnchantment)
    {
        ClearEnchantments();

        //If you're not disenchanting, add a new enchantment.
        if(newEnchantment.abilityName!= "Disenchant")
        {
            enchantParticles.Clear();
            enchantParticles.Play();

            enchantmentName = newEnchantment.nameOfEnchant;
            enchantParticles.startColor = newEnchantment.enchantmentColor;
            enchantRingParticles.startColor = newEnchantment.enchantmentColor;

            // Change attack effect
            overrideEnchantParticles = newEnchantment.appliedAttackParticles;
        }
    }
}
