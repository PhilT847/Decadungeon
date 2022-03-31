using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusWeaponFrame : MonoBehaviour
{
    public Image weaponDexBonusImage;
    public Image weaponFaithBonusImage;
    public Image badgeStrengthBonusImage;

    public TextMeshProUGUI mightText;
    public TextMeshProUGUI dexBonusText;
    public TextMeshProUGUI faithBonusText;

    public void SetWeaponStatus(Character weaponOwner)
    {
        // Check for weapon might, which starts with the weapon and can potentially be increased by the wielder's badge.
        int totalMight = weaponOwner.equippedWeapon.weaponMight;

        // If a badge also grants bonus strength, make the extra bar appear and increase might
        if (weaponOwner.equippedBadge != null && weaponOwner.equippedBadge.strengthBonus != 0)
        {
            totalMight += weaponOwner.equippedBadge.strengthBonus;
            badgeStrengthBonusImage.color = Color.black;
        }
        else
        {
            badgeStrengthBonusImage.color = Color.clear;
        }

        mightText.SetText("+{0}", totalMight);

        if (weaponOwner.equippedWeapon.bonusDexterity > 0)
        {
            weaponDexBonusImage.color = new Color32(100,200,100,255);
            dexBonusText.color = Color.white;
            dexBonusText.SetText("+{0}", weaponOwner.equippedWeapon.bonusDexterity);
        }
        else if (weaponOwner.equippedWeapon.bonusDexterity < 0)
        {
            weaponDexBonusImage.color = new Color32(200, 100, 100, 255);
            dexBonusText.color = Color.white;
            dexBonusText.SetText("-{0}", Mathf.Abs(weaponOwner.equippedWeapon.bonusDexterity));
        }
        else
        {
            weaponDexBonusImage.color = Color.clear;
            dexBonusText.color = Color.clear;
            dexBonusText.SetText("");
        }

        if (weaponOwner.equippedWeapon.bonusFaith > 0)
        {
            weaponFaithBonusImage.color = new Color32(100, 200, 100, 255);
            faithBonusText.color = Color.white;
            faithBonusText.SetText("+{0}", weaponOwner.equippedWeapon.bonusFaith);
        }
        else if (weaponOwner.equippedWeapon.bonusFaith < 0)
        {
            weaponFaithBonusImage.color = new Color32(200, 100, 100, 255);
            faithBonusText.color = Color.white;
            faithBonusText.SetText("-{0}", Mathf.Abs(weaponOwner.equippedWeapon.bonusFaith));
        }
        else
        {
            weaponFaithBonusImage.color = Color.clear;
            faithBonusText.color = Color.clear;
            faithBonusText.SetText("");
        }
    }
}
