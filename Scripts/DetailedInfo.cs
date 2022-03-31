using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DetailedInfo : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;

    public Slider hpSlider;
    public Slider strSlider;
    public Slider magSlider;
    public Slider dexSlider;
    public Slider fthSlider;

    public TextMeshProUGUI currentHP_Text;
    public TextMeshProUGUI maxHP_Text;
    public TextMeshProUGUI currentMP_Text;
    public TextMeshProUGUI maxMP_Text;

    public TextMeshProUGUI strText;
    public TextMeshProUGUI magText;
    public TextMeshProUGUI dexText;
    public TextMeshProUGUI fthText;

    public TextMeshProUGUI physicalDefText;
    public TextMeshProUGUI magicalDefText;
    public TextMeshProUGUI hitText;
    public TextMeshProUGUI critText;
    public TextMeshProUGUI dodgeText;

    public Color increasedColor;
    public Color decreasedColor;

    public void SetToCharacter(Character thisCharacter)
    {
        // Name/level/class
        nameText.SetText(thisCharacter.unitName);

        string levelString = "Level " + thisCharacter.level + " " + thisCharacter.chosenClass.className;
        levelText.SetText(levelString);

        // HP (w/ growth)
        currentHP_Text.SetText("{0}", thisCharacter.currentHP);
        maxHP_Text.SetText("/ {0}", thisCharacter.maxHP);

        if (thisCharacter.equippedWeapon.bonusHP > 0)
        {
            maxHP_Text.color = increasedColor;
        }
        else if (thisCharacter.equippedWeapon.bonusHP < 0)
        {
            maxHP_Text.color = decreasedColor;
        }
        else
        {
            maxHP_Text.color = Color.black;
        }

        // HP slider goes up to 120% (despite growths going up to 150%) and always has at least 10% filled
        float hpValue = thisCharacter.hpGrowth / 120f;

        if(hpValue < 0.1f)
        {
            hpValue = 0.1f;
        }
        else if (hpValue > 1f) // Some growths go above 120%... in this case, lock at 100%
        {
            hpValue = 1f;
        }

        hpSlider.value = hpValue;

        StatBasedTextColor(maxHP_Text, thisCharacter.GetRawMaxHP(), thisCharacter.GetStatWithEquipment("HP"));
        SetFillColor(hpSlider);

        // MP

        currentMP_Text.SetText("{0}", thisCharacter.currentMP);
        maxMP_Text.SetText("/ {0}", thisCharacter.maxMP);

        StatBasedTextColor(maxMP_Text, thisCharacter.GetRawMaxMP(), thisCharacter.GetStatWithEquipment("MP"));

        // Stats. If any value is below 10%, set it to 10% on the slider (which reflects actual leveling growth; see LevelUpScreen)

        strText.SetText("{0}", thisCharacter.GetStatWithEquipment("Strength"));

        strSlider.value = thisCharacter.strGrowth / 100f;

        if(strSlider.value < 0.1f)
        {
            strSlider.value = 0.1f;
        }

        StatBasedTextColor(strText, thisCharacter.GetRawStrength(), thisCharacter.GetStatWithEquipment("Strength"));
        SetFillColor(strSlider);

        magText.SetText("{0}", thisCharacter.GetStatWithEquipment("Magic"));

        magSlider.value = thisCharacter.magGrowth / 100f;

        if (magSlider.value < 0.1f)
        {
            magSlider.value = 0.1f;
        }

        StatBasedTextColor(magText, thisCharacter.GetRawMagic(), thisCharacter.GetStatWithEquipment("Magic"));
        SetFillColor(magSlider);

        dexText.SetText("{0}", thisCharacter.GetStatWithEquipment("Dexterity"));

        dexSlider.value = thisCharacter.dexGrowth / 100f;

        if (dexSlider.value < 0.1f)
        {
            dexSlider.value = 0.1f;
        }

        StatBasedTextColor(dexText, thisCharacter.GetRawDexterity(), thisCharacter.GetStatWithEquipment("Dexterity"));
        SetFillColor(dexSlider);

        fthText.SetText("{0}", thisCharacter.GetStatWithEquipment("Faith"));

        fthSlider.value = thisCharacter.fthGrowth / 100f;

        if (fthSlider.value < 0.1f)
        {
            fthSlider.value = 0.1f;
        }

        StatBasedTextColor(fthText, thisCharacter.GetRawFaith(), thisCharacter.GetStatWithEquipment("Faith"));
        SetFillColor(fthSlider);

        // Secondary stats. Note that certain stats increase with Dex or other equipment

        hitText.SetText("{0}", 100 + thisCharacter.equippedWeapon.weaponHitMod);

        // High hit rates get blue. Low hit rates get red
        if(thisCharacter.equippedWeapon.weaponHitMod >= 20)
        {
            hitText.color = increasedColor;
        }
        else if(thisCharacter.equippedWeapon.weaponHitMod <= -20)
        {
            hitText.color = decreasedColor;
        }
        else
        {
            hitText.color = Color.black;
        }

        // Crit based on dexterity along with their weapon's crit bonus
        critText.SetText("{0}", thisCharacter.GetStatWithEquipment("Dexterity") / 2 + thisCharacter.criticalHitBonus);
        StatBasedTextColor(critText, thisCharacter.GetRawDexterity(), thisCharacter.GetStatWithEquipment("Dexterity"));

        dodgeText.SetText("{0}", thisCharacter.GetStatWithEquipment("Dexterity") / 4);
        StatBasedTextColor(dodgeText, thisCharacter.GetRawDexterity(), thisCharacter.GetStatWithEquipment("Dexterity"));

        physicalDefText.SetText("{0}", thisCharacter.GetStatWithEquipment("Physical Defense"));
        StatBasedTextColor(physicalDefText, thisCharacter.GetRawPhysicalDefense(), thisCharacter.GetStatWithEquipment("Physical Defense"));

        magicalDefText.SetText("{0}", thisCharacter.GetStatWithEquipment("Magical Defense"));
        StatBasedTextColor(magicalDefText, thisCharacter.GetRawMagicalDefense(), thisCharacter.GetStatWithEquipment("Magical Defense"));
    }

    // Change text color based on whether this stat is increased/decreased by equipment
    void StatBasedTextColor(TextMeshProUGUI text, int baseStat, int statWithEquipment)
    {
        if(statWithEquipment > baseStat)
        {
            text.color = increasedColor;
        }
        else if (statWithEquipment < baseStat)
        {
            text.color = decreasedColor;
        }
        else
        {
            text.color = Color.black;
        }
    }

    void SetFillColor(Slider thisSlider)
    {
        if(thisSlider.value < 0.25f)
        {
            thisSlider.fillRect.GetComponent<Image>().color = Color.red;
        }
        else if (thisSlider.value < 0.5f)
        {
            thisSlider.fillRect.GetComponent<Image>().color = Color.yellow;
        }
        else if (thisSlider.value < 0.75f)
        {
            thisSlider.fillRect.GetComponent<Image>().color = Color.green;
        }
        else
        {
            thisSlider.fillRect.GetComponent<Image>().color = Color.cyan;
        }
    }
}
