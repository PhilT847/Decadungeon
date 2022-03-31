using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DruidInspect : Ability
{
    public GameObject wildMagicPrefab;

    //The Druid's ability to learn skills from enemies. Also reveals their HP bar.
    public override void SelectAbility(Unit caster)
    {
        FindObjectOfType<BattleController>().DisplayEnemyButtons();
    }

    public override void UseAbility(Unit user, Unit target)
    {
        ScanEnemy(user, target);
    }

    void ScanEnemy(Unit user, Unit target)
    {
        //Go through the whole enemy's skill list and look for skills that the Mimic can learn. Not usable on bosses
        List<Ability> potentialLearnedSkills = new List<Ability>();

        //Start by scanning the enemy
        if (target.GetComponent<Boss>())
        {
            // Bosses are not affected by Mime.
            return;
            //target.GetComponent<Boss>().ScanEnemy();
        }
        else
        {
            //target.GetComponent<Enemy>().ScanEnemy();

            potentialLearnedSkills = target.GetComponent<Enemy>().skillList;
        }

        GenerateEffectParticles(target.transform);

        //list of spells actually learnable by the Druid.
        List<Ability> learnableSkills = new List<Ability>();

        for (int i = 0; i < potentialLearnedSkills.Count; i++)
        {
            //Many enemies possess "Attack", but druids can't learn it as a spell. Skip the "Attack"s within enemy skill lists.
            if (potentialLearnedSkills[i].abilityName != "Attack")
            {
                bool AlreadyLearnedThisSkill = false;

                //If it's a unique skill, ensure that the druid doesn't already know it. Check their whole spell list for duplicates.
                for (int j = 0; j < user.spellList.Count; j++)
                {
                    //if the name of this skill matches the name of any spells the druid knows, it's unable to be learned again.
                    if (potentialLearnedSkills[i].abilityName == user.spellList[j].abilityName)
                    {
                        AlreadyLearnedThisSkill = true;
                    }
                }

                if (!AlreadyLearnedThisSkill)
                {
                    learnableSkills.Add(potentialLearnedSkills[i]);
                }
            }
        }

        if (learnableSkills.Count > 0) //if there are skills possible to learn, pick a random skill that the enemy knows.
        {
            user.PrintDamageText("NEW!");
            AddSpellToList(user, learnableSkills[Random.Range(0, learnableSkills.Count)]);
        }
    }

    void AddSpellToList(Unit learningUnit, Ability newAbility)
    {
        var newSpell = Instantiate(wildMagicPrefab, learningUnit.transform);

        //convert the ability into Wild Magic, then teach it to the unit.
        newSpell.GetComponent<WildMagic>().ConvertToWildMagic(newAbility.GetComponent<EnemySpecialAttack>());

        learningUnit.spellList.Add(newSpell.GetComponent<WildMagic>());

        learningUnit.GetComponent<Character>().SortSpells(); // Sort the character's spell list
    }
}
