using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff : MonoBehaviour
{
    public string buffName; // Prevents stacking.

    public Unit affectedUnit;

    public int attackMod;
    public int defenseMod;
    public int dexMod;
    public float turnSpeedMod; // Directly affects turn speed

    public int buffDuration; // The amount of time that this buff lasts. Usually 15s
    public float timeRemaining;

    public void ApplyBuff(Unit newUnit)
    {
        //Buffs are not applied to dead units.
        if(newUnit.currentHP > 0)
        {
            //Create a copy of the buff instead of using the original; that way, it can be used on multiple targets.
            Buff copiedBuff = Instantiate(this, newUnit.transform);

            copiedBuff.affectedUnit = newUnit;

            // Animate the buff bar to show the new/applied buff(s)
            copiedBuff.affectedUnit.unitBuffs.GetComponent<Animator>().SetTrigger("ShowBuffs");

            bool foundRepeat = false;

            //checks for repeats. If it repeats, just reset the duration. Otherwise, add the new buff.
            foreach (Buff b in newUnit.unitBuffs.allBuffs)
            {
                if (b.buffName == copiedBuff.buffName)
                {
                    b.timeRemaining = buffDuration;

                    foundRepeat = true;
                }
            }

            if (!foundRepeat)
            {
                copiedBuff.timeRemaining = buffDuration;

                copiedBuff.affectedUnit.strength += copiedBuff.attackMod;
                copiedBuff.affectedUnit.magic += copiedBuff.attackMod;

                copiedBuff.affectedUnit.physicalDefense += copiedBuff.defenseMod;
                copiedBuff.affectedUnit.magicalDefense += copiedBuff.defenseMod;

                copiedBuff.affectedUnit.dexterity += copiedBuff.dexMod;

                copiedBuff.affectedUnit.turnSpeedMultiplier += copiedBuff.turnSpeedMod;

                //Add the buff to the unit's buff list.
                copiedBuff.affectedUnit.unitBuffs.allBuffs.Add(copiedBuff);

                copiedBuff.affectedUnit.unitBuffs.UpdateBuffBar(affectedUnit);
            }
            else //if there's a repeat, just delete this object.
            {
                copiedBuff.affectedUnit.unitBuffs.UpdateBuffBar(affectedUnit);

                Destroy(copiedBuff.gameObject);
            }
        }
    }

    //Since this function is run by the copy, it doesn't require an instantiated buff copy... just destroy the object.
    public void RemoveBuff()
    {
        affectedUnit.strength -= attackMod;
        affectedUnit.magic -= attackMod;

        affectedUnit.physicalDefense -= defenseMod;
        affectedUnit.magicalDefense -= defenseMod;

        affectedUnit.dexterity -= dexMod;

        affectedUnit.turnSpeedMultiplier -= turnSpeedMod;

        Destroy(gameObject);
    }
}
