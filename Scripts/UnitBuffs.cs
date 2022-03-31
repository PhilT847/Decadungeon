using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitBuffs : MonoBehaviour
{
    public Sprite upwardsArrow;
    public Sprite downwardsArrow;
    public Sprite invisibleBox; //invisible sprite for when no buff exists.

    public Image[] buffBar; //0 (attack), 1 (defense), 2 (dex)
    public Image[] statIcons; //0 (attack), 1 (defense), 2 (dex)

    public Image stanceBuffImage; // Special image for Monks using their Stances

    public List<Buff> allBuffs;

    public int currentAtkMod; // Mods to STR/MAG
    public int currentDefMod; // Mods to physical/magical DEF
    public int currentDexMod; // Mods to DEX
    public float currentTimeMod; // Mods to turn speed

    private void Start()
    {
        UpdateBuffBar(null);
    }

    public void UpdateBuffBar(Unit thisUnit)
    {
        // Reset the mods before adding all buff effects
        currentAtkMod = 0;
        currentDefMod = 0;
        currentDexMod = 0;
        currentTimeMod = 0;

        bool addAtkArrow = false;
        bool addDefArrow = false;
        bool addDexArrow = false;

        // For regen to appear, they must currently have an active regen effect
        bool addRegen = (thisUnit != null && thisUnit.GetComponent<Character>() && thisUnit.GetComponent<Character>().regenTimeRemaining > 0f);

        // Add total stat buffs
        for (int i = 0; i < allBuffs.Count; i++)
        {
            currentAtkMod += allBuffs[i].attackMod;
            currentDefMod += allBuffs[i].defenseMod;
            currentDexMod += allBuffs[i].dexMod;
            currentTimeMod += allBuffs[i].turnSpeedMod;
        }

        //Buffed stats get a green arrow. Debuffed stats get a red arrow.
        if (currentAtkMod > 0)
        {
            buffBar[0].sprite = upwardsArrow;
            statIcons[0].color = Color.white;

            addAtkArrow = true;
        }
        else if (currentAtkMod < 0)
        {
            buffBar[0].sprite = downwardsArrow;
            statIcons[0].color = Color.white;

            addAtkArrow = true;
        }
        else //remove the arrow.
        {
            buffBar[0].sprite = invisibleBox;
            statIcons[0].color = Color.clear;
        }

        if (currentDefMod > 0)
        {
            buffBar[1].sprite = upwardsArrow;
            statIcons[1].color = Color.white;

            addDefArrow = true;
        }
        else if (currentDefMod < 0)
        {
            buffBar[1].sprite = downwardsArrow;
            statIcons[1].color = Color.white;

            addDefArrow = true;
        }
        else //remove the arrow.
        {
            buffBar[1].sprite = invisibleBox;
            statIcons[1].color = Color.clear;
        }

        if (currentDexMod > 0)
        {
            buffBar[2].sprite = upwardsArrow;
            statIcons[2].color = Color.white;

            addDexArrow = true;
        }
        else if (currentDexMod < 0)
        {
            buffBar[2].sprite = downwardsArrow;
            statIcons[2].color = Color.white;

            addDexArrow = true;
        }
        else //remove the arrow.
        {
            buffBar[2].sprite = invisibleBox;
            statIcons[2].color = Color.clear;
        }

        if (addRegen)
        {
            buffBar[3].color = Color.white;
        }
        else
        {
            buffBar[3].color = Color.clear;
        }

        PositionBuffBar(addAtkArrow, addDefArrow, addDexArrow, addRegen);
    }

    //Orders buffs from top to bottom.
    public void PositionBuffBar(bool atkBuff, bool defBuff, bool dexBuff, bool regen)
    {
        int yPos = 0;

        if (atkBuff)
        {
            buffBar[0].rectTransform.anchoredPosition = new Vector3(0f, yPos, 0f);
            yPos -= 26;
        }

        if (defBuff)
        {
            buffBar[1].rectTransform.anchoredPosition = new Vector3(0f, yPos, 0f);
            yPos -= 26;
        }

        if (dexBuff)
        {
            buffBar[2].rectTransform.anchoredPosition = new Vector3(0f, yPos, 0f);
            yPos -= 26;
        }

        if (regen)
        {
            buffBar[3].rectTransform.anchoredPosition = new Vector3(0f, yPos, 0f);
        }
    }
}
