using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    public string unitName;

    public int level;

    public int maxHP;
    [HideInInspector] public int currentHP;

    public int maxMP;
    [HideInInspector] public int currentMP;

    public int strength;
    public int magic;
    public int dexterity;
    public int faith;

    public int physicalDefense;
    public int magicalDefense;

    // Bonuses to critical chance
    public int criticalHitBonus;

    // Bonus attack power. Multiplies damage dealt by an amount in the TakeDamage() function
    public float bonusAttackPower;

    // Turn Speed Multiplier is affected by haste skills (ex. Rogue's Evade) and is also randomized in enemies
    public float turnSpeedMultiplier;

    // Damage of criticals; 3 (Rogue), 2 (other classes), 1.25 (enemies)
    public float criticalHitCoefficient;

    public List<Ability> passiveList; // List of passive abilities
    public List<Ability> skillList;
    public List<Ability> spellList;
    public List<string> resistancesList; // Damage from a "resistances" element is reduced to 1
    public List<string> absorbancesList; // Elements that heal the unit instead of harming them
    public List<string> weaknessesList; // Damage from a "weakness" element is amplified 50%

    public UnitBuffs unitBuffs;

    public float ATB_TimeUntilAttack;
    public float ATB_CurrentCharge;

    //text that appears when taking damage or being healed. Contains the DamageText object.
    public GameObject damageText;

    public abstract void TakeDamage(Unit source, int value, bool physicalDamage, string element);

    public abstract void HealUnit(Unit source, int value);

    public abstract void CheckStatusEffects();

    public abstract void CheckBuffs();

    public void PrintDamageText(int value, bool crit, bool healing, bool mpRestore)
    {
        if (GetComponent<Character>())
        {
            var newDamageText = Instantiate(damageText, new Vector3(transform.position.x + 2f, transform.position.y + 0.8f, 0f), Quaternion.identity);
            newDamageText.GetComponent<DamageText>().SetText(value, crit, healing, mpRestore);
        }
        else
        {
            var newDamageText = Instantiate(damageText, new Vector3(transform.position.x + 2f, transform.position.y, 0f), Quaternion.identity);
            newDamageText.GetComponent<DamageText>().SetText(value, crit, healing, mpRestore);
        }
    }

    //print a special string like damage text when a unit gains a status effect "+Regen", dodges/blocks an attack, etc.
    public void PrintDamageText(string specialString)
    {
        if (GetComponent<Character>())
        {
            var newDamageText = Instantiate(damageText, new Vector3(transform.position.x + 2f, transform.position.y + 0.8f, 0f), Quaternion.identity);
            newDamageText.GetComponent<DamageText>().SetText(specialString);
        }
        else
        {
            var newDamageText = Instantiate(damageText, new Vector3(transform.position.x + 2f, transform.position.y, 0f), Quaternion.identity);
            newDamageText.GetComponent<DamageText>().SetText(specialString);
        }
    }
}
