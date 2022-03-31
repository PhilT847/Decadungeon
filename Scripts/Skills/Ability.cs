using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    public string abilityName;
    public int abilityCode;
    public Sprite abilityIcon;
    public string abilityDescription;

    public string associatedElement;

    public bool castableInMenu; //able to be casted from the pause menu.
    public bool spendsChi; // Monk abilities spend all Chi and require at least 1

    //certain abilities target multiple random targets
    public int amountOfHits;
    //% chance that the ability hits multiple times. Reduces by 20% each time (ex. 100 -> 80 -> 60...)... keep in mind that a value greater than 100 (ex. 200) will have multiple guaranteed hits.
    public int chanceOfMultihit;

    public abstract void UseAbility(Unit user, Unit target);
    public abstract void SelectAbility(Unit caster);

    //Particles/animations that appear when a unit is hit by the ability.
    public GameObject effectParticles;

    //Generate particles and destroy them after 1s.
    public void GenerateEffectParticles(Transform target)
    {
        //Particles only appear on living targets.
        if(effectParticles != null && target.GetComponent<Unit>().currentHP > 0)
        {
            var particles = Instantiate(effectParticles, target);

            //move the particles upwards if affecting a character, as their pivot point is at the base (meaning particles will be too low)
            if (target.GetComponent<Character>())
            {
                particles.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            }

            Destroy(particles.gameObject, 1f);
        }
    }

    // Generate specific particles
    public void GenerateEffectParticles(Transform target, ParticleSystem chosenParticles)
    {
        //Particles only appear on living targets.
        if (effectParticles != null && target.GetComponent<Unit>().currentHP > 0)
        {
            var particles = Instantiate(chosenParticles, target);

            //move the particles upwards if affecting a character, as their pivot point is at the base (meaning particles will be too low)
            if (target.GetComponent<Character>())
            {
                particles.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            }

            Destroy(particles.gameObject, 1f);
        }
    }
}
