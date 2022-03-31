using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateSummon : Spell
{
    // The Summon created by this spell
    public GameObject summonedCreature;

    public override void UseAbility(Unit user, Unit target)
    {
        // The actual "target" is irrelevant here
        CastSpell(user, target);

        //update the battle hp/mp bars to ensure they're affected by the spell
        foreach (HeroStatus heroHP in FindObjectsOfType<HeroStatus>())
        {
            heroHP.UpdateHeroStatus();
        }
    }

    // Summoning spells always display the area ally button
    public override void SelectAbility(Unit caster)
    {
        FindObjectOfType<BattleController>().DisplayAreaAllyButton();
    }

    public override void CastSpell(Unit caster, Unit target)
    {
        caster.currentMP -= MP_Cost;

        CreateNewSummon(caster);
    }

    void CreateNewSummon(Unit caster)
    {
        // If the unit already has a summon, swap them out
        if(caster.GetComponent<Character>().currentSummon != null)
        {
            caster.GetComponent<Character>().currentSummon.KillSummon();
        }

        // Spawn a new summon and set their stats

        GameObject newSummon = Instantiate(summonedCreature, caster.transform);
        newSummon.GetComponent<Summon>().SetupSummon(caster.GetComponent<Character>());

        newSummon.transform.localPosition = new Vector3(caster.transform.localPosition.x + 1.8f, caster.transform.localPosition.y + .6f, 0f);
        newSummon.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        GenerateEffectParticles(newSummon.transform);
    }
}
