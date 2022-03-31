public class SummonerSpell : Spell
{
    public int potencyBase; //base damage/healing for a spell.
    public float potencyGrowth; //percentage of Magic that is added to spell potency.

    public bool targetsAllies;
    public bool areaOfEffect;
    public Buff appliedBuff;

    public Summon targetedSummon; // The character's current Summon used for the spell

    public override void UseAbility(Unit user, Unit target)
    {
        CastSpell(user, target);

        //update the battle hp/mp bars to ensure they're affected by the spell
        foreach (HeroStatus heroHP in FindObjectsOfType<HeroStatus>())
        {
            heroHP.UpdateHeroStatus();
        }
    }

    public override void SelectAbility(Unit caster)
    {
        //if it's area of effect, the prompt covers all units. If single-target, prompt the user to select a target.
        if (areaOfEffect)
        {
            if (targetsAllies)
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
            if (targetsAllies)
            {
                FindObjectOfType<BattleController>().DisplayUnitButtons();
            }
            else
            {
                FindObjectOfType<BattleController>().DisplayEnemyButtons();
            }
        }
    }

    public override void CastSpell(Unit caster, Unit target)
    {
        caster.currentMP -= MP_Cost;

        targetedSummon = null;

        // For summoner spells to work, there must be a current summon. Otherwise, the spell misses
        if (caster.GetComponent<Character>().currentSummon != null)
        {
            targetedSummon = caster.GetComponent<Character>().currentSummon;
        }
        else
        {
            caster.PrintDamageText("MISS");
            return;
        }

        ApplyEffect(caster, target);
    }

    // Do something with the current summon
    void ApplyEffect(Unit caster, Unit target)
    {
        GenerateEffectParticles(targetedSummon.transform);

        // Heal/empower your summon
        if (targetsAllies)
        {
            targetedSummon.HealUnit(caster, potencyBase + (int)(caster.faith * potencyGrowth));
            appliedBuff.ApplyBuff(targetedSummon);

            // Grow the summon if they're not already large
            if(targetedSummon.summonGraphics.localScale.x < 1.2f)
            {
                targetedSummon.StartCoroutine(targetedSummon.ChangeSize(true));
            }
        }
        else
        {

        }
    }
}
