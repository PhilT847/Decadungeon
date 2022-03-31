using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item
{
    public int weaponMight;
    public string weaponElement;
    public ParticleSystem weaponParticles;

    // Used for the Rusty Pitchfork. Once this weapon deals 200 damage, it transforms.
    public Weapon upgradedForm;
    public int damageUntilUpgrade;

    public int bonusHP;
    public int bonusMP;
    public int weaponHitMod; //% loss or gain to hit rate
    public int weaponBonusCritical;

    public int bonusFaith;
    public int bonusDexterity;
    public int bonusPhysicalDefense;
    public int bonusMagicalDefense;

    public bool rangedWeapon; // Bows deal full damage from the back row.
    public bool mixedRangeWeapon; // Whips and the Lightning Rod can attack from all ranges

    public bool areaAttack; // Whips deal damage to all enemies

    public override void EquipItem(Character equipper)
    {
        // If there's already a weapon equipped, remove bonus stats
        if(equipper.equippedWeapon != null)
        {
            equipper.maxHP -= equipper.equippedWeapon.bonusHP;
            equipper.maxMP -= equipper.equippedWeapon.bonusMP;
            equipper.faith -= equipper.equippedWeapon.bonusFaith;
            equipper.dexterity -= equipper.equippedWeapon.bonusDexterity;
            equipper.physicalDefense -= equipper.equippedWeapon.bonusPhysicalDefense;
            equipper.magicalDefense -= equipper.equippedWeapon.bonusMagicalDefense;
            equipper.criticalHitBonus -= equipper.equippedWeapon.weaponBonusCritical;
        }

        itemOwner = equipper;

        if (equipper.equippedWeapon != null)
        {
            //If the unit goes from using a sword/staff/whip to a bow (or vice versa), rotate the weapon sprite as needed to make the unit hold it correctly. Also, if the equipped weapon is null (meaning the game started), also apply bow rotation.
            if (!equipper.equippedWeapon.rangedWeapon && rangedWeapon)
            {
                equipper.chosenClass.weaponSprite.transform.Rotate(new Vector3(0f, 0f, equipper.chosenClass.rotateForBow)); //= Quaternion.Euler(0f, 0f, chosenClass.weaponSprite.transform.rotation.z - chosenClass.rotateForBow);
            }
            else if (equipper.equippedWeapon.rangedWeapon && !rangedWeapon) //ranged to melee/mixed
            {
                equipper.chosenClass.weaponSprite.transform.Rotate(new Vector3(0f, 0f, -equipper.chosenClass.rotateForBow));
            }
        }
        else if (rangedWeapon) // If equipping a ranged weapon after having no weapon, rotate for the bow
        {
            equipper.chosenClass.weaponSprite.transform.Rotate(new Vector3(0f, 0f, equipper.chosenClass.rotateForBow));
        }

        equipper.equippedWeapon = this;

        equipper.maxHP += equipper.equippedWeapon.bonusHP;
        equipper.maxMP += equipper.equippedWeapon.bonusMP;
        equipper.faith += equipper.equippedWeapon.bonusFaith;
        equipper.dexterity += equipper.equippedWeapon.bonusDexterity;
        equipper.physicalDefense += equipper.equippedWeapon.bonusPhysicalDefense;
        equipper.magicalDefense += equipper.equippedWeapon.bonusMagicalDefense;
        equipper.criticalHitBonus += equipper.equippedWeapon.weaponBonusCritical;

        // Wait until after the weapon is equipped to check maximum MP/HP
        if (equipper.currentHP > equipper.maxHP)
        {
            equipper.currentHP = equipper.maxHP;
        }

        if (equipper.currentMP > equipper.maxMP)
        {
            equipper.currentMP = equipper.maxMP;
        }

        // Change the sprite to equip the new weapon, unless the unit is a monk.
        if (equipper.chosenClass.className != "Monk")
        {
            equipper.chosenClass.weaponSprite.sprite = equipper.equippedWeapon.itemImage;
        }
    }

    // Remove a weapon from a unit
    public override void UnequipItem(Character equipper)
    {
        itemOwner = null; // Remove ownership

        equipper.maxHP -= equipper.equippedWeapon.bonusHP;
        equipper.maxMP -= equipper.equippedWeapon.bonusMP;
        equipper.faith -= equipper.equippedWeapon.bonusFaith;
        equipper.dexterity -= equipper.equippedWeapon.bonusDexterity;
        equipper.physicalDefense -= equipper.equippedWeapon.bonusPhysicalDefense;
        equipper.magicalDefense -= equipper.equippedWeapon.bonusMagicalDefense;
        equipper.criticalHitBonus -= equipper.equippedWeapon.weaponBonusCritical;

        if (equipper.currentHP > equipper.maxHP)
        {
            equipper.currentHP = equipper.maxHP;
        }

        if (equipper.currentMP > equipper.maxMP)
        {
            equipper.currentMP = equipper.maxMP;
        }

        // After removing stats, remove the weapon itself from the character
        equipper.equippedWeapon = null;
        equipper.chosenClass.weaponSprite.sprite = null;
    }

    // Used when the Rusty Pitchfork transforms during combat
    public IEnumerator TransformWeapon(Character equipper)
    {
        // Wait for the unit to finish their attack; then, transform!
        yield return new WaitForSeconds(1f);

        Time.timeScale = 0f;
        FindObjectOfType<BattleController>().ActivateEnemyActionText("Pitchfork..?!");

        yield return new WaitForSecondsRealtime(1f);

        // Play a noise and equip
        upgradedForm.EquipItem(equipper);

        yield return new WaitForSecondsRealtime(1f);

        Time.timeScale = 1f;
        FindObjectOfType<BattleController>().RemoveEnemyActionText();

        // Change upgradedForm to null to ensure it's not seeking another transformation
        upgradedForm = null;

        yield return null;
    }
}
