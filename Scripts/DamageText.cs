using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public TextMeshProUGUI numberText;
    private float timeRemaining = 1f;

    void Update()
    {
        // Move/delete the damage text, even when pausing
        if(timeRemaining > 0f)
        {
            timeRemaining -= Time.unscaledDeltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetText(int value, bool critical, bool healing, bool mpRestore)
    {
        timeRemaining = 1f;

        numberText.SetText("{0}", value);

        numberText.color = Color.red;

        // Critical text is yellow with an exclamation point at the end
        if (critical)
        {
            numberText.color = Color.yellow;
            numberText.SetText("{0}!", value);
        }

        // Healing text is green
        if (healing)
        {
            numberText.color = Color.green;
        }

        // MP-Restore text is blue
        if (mpRestore)
        {
            numberText.color = Color.blue;
        }
    }

    public void SetText(string chosenText)
    {
        numberText.color = Color.cyan;

        numberText.SetText(chosenText);
    }
}
