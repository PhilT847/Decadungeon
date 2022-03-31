using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaknessBar : MonoBehaviour
{
    public int heldResistances;
    public int heldWeaknesses;

    public GameObject resistPanel;
    public GameObject weaknessPanel;

    public Image[] resistedElements;
    public Image[] weakElements;

    public Sprite[] elementImages; // 0 (Fire), 1 (Ice), 2 (Lightning), 3 (Shadow), 4 (Holy), 5 (Physical)

    public void SetToEnemy(Enemy newEnemy)
    {
        heldResistances = 0;
        heldWeaknesses = 0;

        // Check for relevant elements in each list
        SearchForElementsIn(newEnemy.absorbancesList, false);
        SearchForElementsIn(newEnemy.resistancesList, false);
        SearchForElementsIn(newEnemy.weaknessesList, true);

        // If there's only 1 resistance/weakness, set the extra image invisible
        if (heldResistances == 1)
        {
            resistedElements[1].color = Color.clear;
        }
        else
        {
            resistedElements[1].color = Color.white;
        }

        if (heldWeaknesses == 1)
        {
            weakElements[1].color = Color.clear;
        }
        else
        {
            weakElements[1].color = Color.white;
        }

        // If there are no resistances/weaknesses, make the panels disappear
        resistPanel.SetActive(heldResistances != 0);
        weaknessPanel.SetActive(heldWeaknesses != 0);
    }

    // Find common absorbances/resistances/weaknesses in a list of strings
    void SearchForElementsIn(List<string> chosenList, bool lookingForWeaknesses)
    {
        for (int i = 0; i < chosenList.Count; i++)
        {
            switch (chosenList[i])
            {
                case "Fire":

                    if (!lookingForWeaknesses)
                    {
                        AddResistElement(0);
                    }
                    else
                    {
                        AddWeakElement(0);
                    }

                    break;

                case "Ice":

                    if (!lookingForWeaknesses)
                    {
                        AddResistElement(1);
                    }
                    else
                    {
                        AddWeakElement(1);
                    }

                    break;
                case "Lightning":

                    if (!lookingForWeaknesses)
                    {
                        AddResistElement(2);
                    }
                    else
                    {
                        AddWeakElement(2);
                    }

                    break;
                case "Shadow":

                    if (!lookingForWeaknesses)
                    {
                        AddResistElement(3);
                    }
                    else
                    {
                        AddWeakElement(3);
                    }

                    break;
                case "Holy":

                    if (!lookingForWeaknesses)
                    {
                        AddResistElement(4);
                    }
                    else
                    {
                        AddWeakElement(4);
                    }

                    break;
                case "Physical":

                    if (!lookingForWeaknesses)
                    {
                        AddResistElement(5);
                    }
                    else
                    {
                        AddWeakElement(5);
                    }

                    break;
            }
        }
    }

    void AddResistElement(int resistIndex)
    {
        heldResistances++;
        resistedElements[heldResistances - 1].sprite = elementImages[resistIndex];
    }

    void AddWeakElement(int weakIndex)
    {
        heldWeaknesses++;
        weakElements[heldWeaknesses - 1].sprite = elementImages[weakIndex];
    }
}
