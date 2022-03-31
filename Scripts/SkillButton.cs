using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillButton : MonoBehaviour
{
    public Ability heldAbility;

    public Image buttonImage;
    public Image skillIconImage;

    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI mpCostText;

    public void SetToAbility(Ability thisAbility)
    {
        heldAbility = thisAbility;

        // When resetting the character, default the button to white
        buttonImage.color = Color.white;

        skillIconImage.sprite = thisAbility.abilityIcon;

        skillNameText.SetText(thisAbility.abilityName);

        // If the ability is a spell, display its MP cost
        if (thisAbility.GetComponent<Spell>())
        {
            mpCostText.SetText("{0}", thisAbility.GetComponent<Spell>().MP_Cost);

            mpCostText.color = Color.blue;
        }
        else
        {
            mpCostText.color = Color.clear;
        }
    }
}
