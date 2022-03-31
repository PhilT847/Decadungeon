using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMechanic : MonoBehaviour
{
    [HideInInspector] public Boss mechanicOwner;

    public string mechanicName;
    public string activationType; // OnLowHealth (<50% HP), OnDamaged
    public bool activated; // Prevents certain skills from activating multiple times in a fight (ex. berserk)
    private int mechanicHP; // Some mechanics (ex. Witch's Mirror) are removed by taking enough damage
    public int absorbedDamage; // Damage absorbed by mechanics like the Mirror

    //Particles/animations that appear when a unit is hit by the ability.
    public GameObject effectParticles;

    public Unit chosenTarget;

    public IEnumerator ActivateMechanic(Unit attackSource, int damageInflicted, string element)
    {
        // By this point (after taking damage), the BattleController will attempt to restart the ATB gauges. Stop the SelectedUnitUse* Coroutines to prevent this change
        FindObjectOfType<BattleController>().StopAllCoroutines();

        // Freeze the ATB gauge while at work
        FindObjectOfType<BattleController>().ATB_Active = false;

        switch (mechanicName)
        {
            case "Berserk": // Frost Giant's Berseker mode, reducing ATB time and increasing damage

                yield return new WaitForSeconds(1f);

                if (!activated)
                {
                    activated = true;

                    mechanicOwner.bossAnim.SetTrigger("Berserk");
                    mechanicOwner.ATB_TimeUntilAttack = 8f; // ATB gauge now moves faster
                    mechanicOwner.strength = (int)(mechanicOwner.strength * 1.2f);
                    mechanicOwner.magic = (int)(mechanicOwner.magic * 1.2f);

                    FindObjectOfType<BattleController>().ActivateEnemyActionText("Berserk");

                    yield return new WaitForSeconds(1f);

                    GenerateEffectParticles(mechanicOwner.transform);

                    yield return new WaitForSeconds(0.75f);

                    FindObjectOfType<BattleController>().RemoveEnemyActionText();

                    yield return new WaitForSeconds(0.25f);
                }

                break;

            case "MagicMirror": // Sea Witch reflects all incoming magic damage when her Mirror is active

                yield return new WaitForSeconds(1f);

                // The mirror takes physical damage and can shatter
                if ((activated && element == "Physical") || mechanicOwner.currentHP < 1)
                {
                    mechanicHP -= damageInflicted;

                    if(mechanicHP < 1 || mechanicOwner.currentHP < 1)
                    {
                        // Wait before activating
                        yield return new WaitForSeconds(0.25f);

                        mechanicHP = 0;
                        activated = false;
                        mechanicOwner.extraAnim.SetTrigger("MirrorClosed");

                        // Wait before selecting next unit
                        yield return new WaitForSeconds(0.75f);
                    }
                }

                // If still activated, reflect magical damage or begin mirroring if appropriate
                if (mechanicOwner.currentHP > 0 && element != "Physical")
                {
                    // The mirror activates when taking magical damage. Chance to activate = 50%
                    if (!activated)
                    {
                        int diceRoll = Random.Range(0, 2);

                        if(diceRoll == 0)
                        {
                            FindObjectOfType<BattleController>().ActivateEnemyActionText("Magic Mirror");
                            mechanicOwner.bossAnim.SetTrigger("BossAttack");
                            yield return new WaitForSeconds(1.125f);

                            activated = true;
                            mechanicHP = mechanicOwner.maxHP / 8; // Mirror has 12.5% of the Hag's max HP.
                            mechanicOwner.extraAnim.SetTrigger("MirrorOpen");

                            //time after the attack connects before the enemy action text goes away
                            yield return new WaitForSeconds(0.125f);

                            FindObjectOfType<BattleController>().RemoveEnemyActionText();

                            //time before selecting the next unit
                            yield return new WaitForSeconds(0.75f);
                        }
                    }
                    else
                    {
                        FindObjectOfType<BattleController>().ActivateEnemyActionText("Reflect");

                        //mechanicOwner.bossAnim.SetTrigger("BossAttack");

                        yield return new WaitForSeconds(1.25f);

                        attackSource.TakeDamage(mechanicOwner, absorbedDamage, false, element);

                        GenerateEffectParticles(attackSource.transform);

                        // Return absorbed damage to zero
                        absorbedDamage = 0;

                        //time after the attack connects before the enemy action text goes away
                        yield return new WaitForSeconds(0.125f);

                        FindObjectOfType<BattleController>().RemoveEnemyActionText();

                        //time before selecting the next unit
                        yield return new WaitForSeconds(0.75f);
                    }
                }

                // The barrier goes down 34% its maximum per turn, so it's never up for more than 3 turns.
                mechanicHP -= (mechanicOwner.maxHP / 24);

                // If this final damage destroys it while active, wait a moment and destroy it
                if (activated && mechanicHP < 1)
                {
                    // Wait before activating
                    yield return new WaitForSeconds(0.25f);

                    mechanicHP = 0;
                    activated = false;
                    mechanicOwner.extraAnim.SetTrigger("MirrorClosed");

                    // Wait before selecting next unit
                    yield return new WaitForSeconds(0.75f);
                }

                break;

            case "TimeStop": // Vampire puts one unit in time stop

                // If there's only one Hero alive, don't freeze them! Return the clock so that they're able to act
                if (FindObjectOfType<GameController>().FindLivingCharacters().Count == 1 && chosenTarget == mechanicOwner.PickDirectTarget())
                {
                    mechanicOwner.bossAnim.SetTrigger("BossAttack");

                    FindObjectOfType<BattleController>().ActivateEnemyActionText("Return Time");

                    yield return new WaitForSeconds(1.25f);

                    // PickDirectTarget() is always the one living hero, which is why it can be used to denote the last person alive
                    mechanicOwner.PickDirectTarget().turnSpeedMultiplier = 1f;
                    mechanicOwner.extraAnim.SetTrigger("ClockReturn");

                    yield return new WaitForSeconds(0.75f);

                    FindObjectOfType<BattleController>().RemoveEnemyActionText();

                    yield return new WaitForSeconds(0.25f);

                    yield return null;
                }

                // 33% chance to cast Time Stop... always cast on the first turn (it's not "activated" until the first use, which forces it to rely on chance moving forwards)
                int stopRoll = Random.Range(0, 3);

                // Activate if: 33% OR not activated yet OR the chosen target died
                if(FindObjectOfType<GameController>().FindLivingCharacters().Count > 1 && (stopRoll == 0 || !activated || (chosenTarget != null && chosenTarget.currentHP == 0)))
                {
                    activated = true;

                    // If the vampire already stunned someone, remove their stun before picking a new target
                    if (chosenTarget != null)
                    {
                        chosenTarget.turnSpeedMultiplier = 1f;
                        mechanicOwner.extraAnim.SetTrigger("ClockReturn");
                    }

                    // Pick a random living hero to freeze
                    chosenTarget = FindObjectOfType<GameController>().FindLivingCharacters()[Random.Range(0, FindObjectOfType<GameController>().FindLivingCharacters().Count)];

                    mechanicOwner.bossAnim.SetTrigger("BossAttack");

                    FindObjectOfType<BattleController>().ActivateEnemyActionText("Time Stop");

                    yield return new WaitForSeconds(1.25f);

                    // Remove status effects and freeze the target in place. Move the clock to the new target
                    GenerateEffectParticles(chosenTarget.transform);
                    chosenTarget.turnSpeedMultiplier = 0f;
                    mechanicOwner.extraAnim.transform.position = new Vector3(chosenTarget.transform.position.x, chosenTarget.transform.position.y + 1f, 0f);

                    mechanicOwner.extraAnim.SetTrigger("ClockEntry");

                    yield return new WaitForSeconds(0.75f);

                    FindObjectOfType<BattleController>().RemoveEnemyActionText();

                    yield return new WaitForSeconds(0.25f);
                }

                // Make their ATB appear black
                foreach (HeroStatus h in FindObjectsOfType<HeroStatus>())
                {
                    h.VisualizeATB();
                }

                break;
        }

        // Perform the ending functions of the SelectedUnitUse* Coroutines, as they have been stopped
        if (!FindObjectOfType<BattleController>().CheckForWonBattle())
        {
            // Restart the ATB gauge
            FindObjectOfType<BattleController>().ATB_Active = true;

            if(attackSource != null)
            {
                attackSource.ATB_CurrentCharge = 0f;
            }
        }

        yield return null;
    }

    //Generate particles and destroy them after 1s.
    public void GenerateEffectParticles(Transform target)
    {
        //Particles only appear on living targets.
        if (effectParticles != null && target.GetComponent<Unit>().currentHP > 0)
        {
            var particles = Instantiate(effectParticles, target);

            //move the particles upwards if affecting a character, as their pivot point is at the base (meaning particles will be too low)
            if (target.GetComponent<Character>())
            {
                particles.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            }

            Destroy(particles.gameObject, 1.5f);
        }
    }
}
