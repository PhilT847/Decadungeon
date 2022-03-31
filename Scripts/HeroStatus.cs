using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroStatus : MonoBehaviour
{
    public Character owner;
    //public TextMeshProUGUI heroText;

    public Slider ATB_Gauge;
    public Image ATB_Fill;

    public Slider HP_Slider;
    public Slider MP_Slider;
    public Slider Chi_Slider; // Chi slider used for Monks

    //public TextMeshProUGUI HP_Max;
    //public TextMeshProUGUI MP_Max;

    public TextMeshProUGUI HP_Text;
    public TextMeshProUGUI MP_Text;

    public void UpdateHeroStatus()
    {
        // Most classes use the regular MP bar. Monks use a special bar.
        if (owner.maxChi == 0 && Chi_Slider.gameObject.activeSelf)
        {
            MP_Slider.gameObject.SetActive(true);
            Chi_Slider.gameObject.SetActive(false);
        }
        else if (owner.maxChi > 0 && MP_Slider.gameObject.activeSelf)
        {
            MP_Slider.gameObject.SetActive(false);
            Chi_Slider.gameObject.SetActive(true);
        }

        //HP_Slider.value = (float)owner.currentHP / owner.maxHP;
        HP_Text.SetText("{0}", owner.currentHP);

        SetResourceBar();

        float hpPercentage = (float)owner.currentHP / owner.maxHP;

        // When hurt, animate
        if(hpPercentage < HP_Slider.value)
        {
            GetComponent<Animator>().SetTrigger("Hurt");
        }

        StartCoroutine(MoveValueTo(HP_Slider, HP_Slider.value, hpPercentage));

        // HP bar appears red at low HP, yellow at middling HP... otherwise, it's green
        if (hpPercentage > 0.66f)
        {
            HP_Slider.fillRect.GetComponent<Image>().color = new Color32(100, 220, 45, 255);
        }
        else if (hpPercentage > 0.33f)
        {
            HP_Slider.fillRect.GetComponent<Image>().color = new Color32(230, 230, 40, 255);
        }
        else
        {
            HP_Slider.fillRect.GetComponent<Image>().color = new Color32(220, 45, 45, 255);
        }

        // For liches, any updates to their HP also affects their Blood Rush mechanic
        if(owner.chosenClass.className == "Lich")
        {
            owner.UpdateBloodRush();
        }
    }

    // Sets MP or Chi based on class
    void SetResourceBar()
    {
        if(MP_Slider.gameObject.activeSelf)
        {
            //MP_Slider.value = (float)owner.currentMP / owner.maxMP;
            MP_Text.SetText("{0}", owner.currentMP);

            StartCoroutine(MoveValueTo(MP_Slider, MP_Slider.value, (float)owner.currentMP / owner.maxMP));
        }
        else
        {
            //Chi_Slider.value = (float)owner.currentChi / owner.maxChi;

            StartCoroutine(MoveValueTo(Chi_Slider, Chi_Slider.value, (float)owner.currentChi / owner.maxChi));
        }
    }

    // Fills the ATB bars of all characters
    public void VisualizeATB()
    {
        ATB_Gauge.value = owner.ATB_CurrentCharge / owner.ATB_TimeUntilAttack;

        // Check for statuses that alter the color of the ATB Gauge

        if (owner.currentHP == 0 || owner.turnSpeedMultiplier == 0f)
        {
            ATB_Fill.color = new Color32(25, 25, 25, 220);
        }
        else if (owner.turnSpeedMultiplier < 0.95f) // Decreased (orange)
        {
            ATB_Fill.color = new Color32(255, 200, 0, 200);
        }
        else if (owner.turnSpeedMultiplier > 1.05f) // Increased (cyan)
        {
            ATB_Fill.color = new Color32(125, 255, 255, 200);
        }
        else // Normal ATB (white)
        {
            ATB_Fill.color = new Color32(255, 255, 255, 150);
        }
    }

    // Used by each slider. Moves one value to another in 0.1s
    public IEnumerator MoveValueTo(Slider thisSlider, float startPercentage, float endPercentage)
    {
        thisSlider.value = startPercentage;

        float elapsedTime = 0f;

        while(elapsedTime <= 0.1f)
        {
            elapsedTime += Time.deltaTime;

            thisSlider.value = Mathf.Lerp(startPercentage, endPercentage, elapsedTime / 0.1f);

            // Also colors the HP bar as appropriate
            if (thisSlider == HP_Slider)
            {
                // HP bar appears red at low HP, yellow at middling HP... otherwise, it's green
                if (thisSlider.value > 0.66f)
                {
                    HP_Slider.fillRect.GetComponent<Image>().color = new Color32(100, 220, 45, 255);
                }
                else if (thisSlider.value > 0.33f)
                {
                    HP_Slider.fillRect.GetComponent<Image>().color = new Color32(230, 230, 40, 255);
                }
                else
                {
                    HP_Slider.fillRect.GetComponent<Image>().color = new Color32(220, 45, 45, 255);
                }
            }

            yield return new WaitForEndOfFrame();
        }

        thisSlider.value = endPercentage;

        yield return null;
    }
}
