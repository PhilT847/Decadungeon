using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoseGameScreen : MonoBehaviour
{
    public GameController gameController;

    public TextMeshProUGUI floorNumberText; //text that says which floor you lost on.
    public TextMeshProUGUI[] graveTexts; //headstone texts in character order.

    public void SetHeadstones()
    {
        floorNumberText.SetText("Your adventure ends on Floor {0}...", FindObjectOfType<FloorBuilder>().floorNumber);

        for(int i = 0; i < 4; i++)
        {
            Character deadHero = gameController.allCharacters[i];

            string headStoneString = "Lv. " + deadHero.level + " " + deadHero.chosenClass.className + "\n";

            headStoneString += "Kills: " + deadHero.lifetimeKills + "\n";

            headStoneString += "Deaths: " + deadHero.lifetimeDeaths + "\n";

            headStoneString += "Damage: " + deadHero.lifetimeDamageDealt + "\n";

            headStoneString += "Healing: " + deadHero.lifetimeHealingDealt;

            graveTexts[i].SetText(headStoneString);
        }
    }
}
