using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleController : MonoBehaviour
{
    public FloorBuilder floorCounter;
    public LevelUpScreen levelScreen;
    public Image battleBackdrop; // The backdrop
    public Sprite[] battleBackgrounds; // Various background images for battles

    public HeroStatus[] allHeroStatus; // All the status (HP/MP/ATB) bars for heroes

    public GameObject[] battleFormations;

    public Transform formationObject; //the object where battleFormations are placed

    public Enemy[] lowTierEnemies;
    public Enemy[] midTierEnemies;
    public Enemy[] highTierEnemies;
    public Boss[] bossEnemies;
    public Boss floorBoss; // The boss faced on this floor. Determined at build time (see FloorBuilder)
    public int bossIndex; // The current index of bossEnemies. Used to cycle through bosses.

    public BattleFormation currentEnemies;

    public List<Unit> turnOrder;

    public List<Unit> heroesWithMaxATB; //heroes with maximum ATB gauge. Act in order with one another
    public List<Unit> enemiesWithMaxATB; //enemies with maximum ATB gauge. They wait in line for each other and act instantly once their gauge is full.

    public Ability attackAbility;
    public Ability selectedAbility;

    //the unit whose turn it is.
    public Unit unitInControl;

    public CommandList unitCommandList;
    public GameObject enemyActionText;
    public GameObject cancelCommandButton;

    public List<Button> unitButtons;
    public List<Button> enemyButtons;
    public Button areaOfEffectAttackButton;
    public Button areaOfEffectSupportButton;

    public int battleExp; //the amount of exp that winning this battle awards

    public bool wonBattle; //the WonBattle() coroutine isn't run if you already won the fight. Ensures that you don't "win" multiple times if you kill multiple enemies at once.

    public bool ATB_Active;
    public bool ATB_EnemiesActive;
    public float TimeUntilStatusCheck; //statuses are checked every 1s and reduced/removed accordingly.

    public bool isBossBattle; // Boss battles and regular battles have different setups

    private void Update()
    {
        // Summons always go right before their masters
        if (ATB_Active)
        {
            ATB_SummonBars();
        }

        if (ATB_Active)
        {
            ATB_HeroBars();
        }

        if (ATB_Active)
        {
            if (isBossBattle)
            {
                ATB_BossBar();
            }
            else
            {
                ATB_EnemyBars();
            }
        }

        if (ATB_Active)
        {
            ATB_StatusCheck();
        }
    }

    public void ATB_HeroBars()
    {
        for (int i = 0; i < FindObjectOfType<GameController>().allCharacters.Length; i++)
        {
            // If the ATB is no longer running (a hero started their turn), then stop going through other characters
            if (!ATB_Active)
            {
                return;
            }

            Character thisCharacter = FindObjectOfType<GameController>().allCharacters[i];

            if (thisCharacter.currentHP > 0 && thisCharacter.turnSpeedMultiplier > 0f)
            {
                //update the battle atb bars.
                foreach (HeroStatus heroHP in allHeroStatus)
                {
                    heroHP.VisualizeATB();

                    // Animate the ATB gauge when full
                    if(heroHP.owner.ATB_CurrentCharge >= heroHP.owner.ATB_TimeUntilAttack)
                    {
                        heroHP.ATB_Gauge.GetComponent<Animator>().SetTrigger("ATB_Full");
                    }
                }

                if (thisCharacter.ATB_CurrentCharge < thisCharacter.ATB_TimeUntilAttack)
                {
                    // The value that the unit's ATB gauge increases... time-based and increases with DEX. Faster with a higher turnSpeedMultiplier
                    float ATB_Increase = Time.deltaTime * (1f + (thisCharacter.dexterity * 0.02f) * thisCharacter.turnSpeedMultiplier);

                    thisCharacter.ATB_CurrentCharge += ATB_Increase;
                }
                else
                {
                    thisCharacter.ATB_CurrentCharge = thisCharacter.ATB_TimeUntilAttack;

                    ATB_ChooseHero(thisCharacter);

                    return;
                }
            }
        }
    }

    public void ATB_SummonBars()
    {
        for(int i = 0; i < FindObjectsOfType<Summon>().Length; i++)
        {
            Summon thisSummon = FindObjectsOfType<Summon>()[i];

            if (thisSummon.turnSpeedMultiplier > 0f)
            {
                if (thisSummon.ATB_CurrentCharge < thisSummon.ATB_TimeUntilAttack)
                {
                    // Their ATB value is equal to their master's to ensure they go after them
                    thisSummon.ATB_CurrentCharge = thisSummon.summonOwner.ATB_CurrentCharge;
                }
                else
                {
                    thisSummon.ATB_CurrentCharge = thisSummon.ATB_TimeUntilAttack;
                    thisSummon.UseAbility(); //Attack
                    return;
                }
            }
        }
    }

    public void ATB_EnemyBars()
    {
        for(int i = 0; i < currentEnemies.listOfEnemies.Length; i++)
        {
            if (currentEnemies.listOfEnemies[i] != null)
            {
                Enemy thisEnemy = currentEnemies.listOfEnemies[i];

                if (thisEnemy.ATB_CurrentCharge < thisEnemy.ATB_TimeUntilAttack)
                {
                    //note the random multiplier. Enemies have a slightly randomized ATB bar to more randomly switch their attack order.
                    thisEnemy.ATB_CurrentCharge += Time.deltaTime * (1f + (thisEnemy.dexterity * 0.02f) * thisEnemy.turnSpeedMultiplier);
                }
                else
                {
                    thisEnemy.StartCoroutine(thisEnemy.EnemyAction()); //enemy acts, deactivating all other enemy ATBs and this function until it's over.
                    return;
                }
            }
        }
    }

    public void ATB_BossBar()
    {
        Boss theBoss = FindObjectOfType<Boss>();

        if (theBoss.ATB_CurrentCharge < theBoss.ATB_TimeUntilAttack)
        {
            //note the random multiplier. Enemies have a slightly randomized ATB bar to more randomly switch their attack order.
            theBoss.ATB_CurrentCharge += Time.deltaTime * (1f + (theBoss.dexterity * 0.02f) * theBoss.turnSpeedMultiplier);
        }
        else
        {
            theBoss.StartCoroutine(theBoss.EnemyAction()); //enemy acts, deactivating all other enemy ATBs and this function until it's over.
            return;
        }
    }

    //Check status effects every 1 second and alter accordingly.
    public void ATB_StatusCheck()
    {
        if(TimeUntilStatusCheck > 0f)
        {
            TimeUntilStatusCheck -= Time.deltaTime;
        }
        else
        {
            TimeUntilStatusCheck = 1f;

            foreach(Unit u in turnOrder)
            {
                if(u != null && u.currentHP > 0)
                {
                    u.CheckStatusEffects();
                }
            }
        }
    }
    
    public void ATB_ChooseHero(Character hero)
    {
        FindObjectOfType<AudioManager>().Play("beginTurn");

        ATB_Active = false;

        unitCommandList.ReturnToMainCommandMenu();

        unitInControl = hero;

        unitCommandList.OpenCommandsMenu();
        unitCommandList.SetNewCharacter(hero);
    }

    //search through each hero/enemy and compare their Dex stats. Sort from highest to lowest.
    public void CreateTurnOrder()
    {
        turnOrder.Clear();

        List<Unit> allUnits = new List<Unit>();

        foreach (Character addHero in FindObjectOfType<GameController>().allCharacters)
        {
            allUnits.Add(addHero);
        }

        foreach (Enemy addEnemy in currentEnemies.listOfEnemies)
        {
            //Randomize enemy ATB every time they act to make the pattern more unpredictable.
            addEnemy.ATB_CurrentCharge = Random.Range(0f, addEnemy.ATB_TimeUntilAttack * 0.33f);
            allUnits.Add(addEnemy);
        }

        if (FindObjectOfType<Boss>())
        {
            //Randomize enemy ATB every time they act to make the pattern more unpredictable.
            FindObjectOfType<Boss>().ATB_CurrentCharge = Random.Range(0.1f, FindObjectOfType<Boss>().ATB_TimeUntilAttack * 0.4f);
            allUnits.Add(FindObjectOfType<Boss>());
        }

        turnOrder = allUnits;

        //once a turn order is settled and the battle begins, reset unit buttons so that they can be displayed properly
        ResetUnitButtons();
    }

    public void StartFirstTurn()
    {
        //set wonBattle to false until you defeat all enemies.
        wonBattle = false;

        // Boss battles have some special properties.
        if (isBossBattle)
        {
            CreateTurnOrder();
        }
        else
        {
            CreateTurnOrder();
        }

        foreach(Unit u in turnOrder)
        {
            // Each point of dexterity adds 0.1s of precharge (1s per 10 points, up to 4s)
            float dexPrecharge = u.dexterity * 0.1f;

            // Caps at 4s (>40 dex)
            if (u.dexterity > 40f)
            {
                dexPrecharge = 4f;
            }

            // Up to 4.5s-9.5s of precharge
            u.ATB_CurrentCharge = Random.Range(dexPrecharge, 5.5f + dexPrecharge);
        }

        ATB_Active = true;
    }

    //First, destroy the current battle formation should one exist. Next, generate a new formation and populate it with enemies.
    public void PopulateEnemies()
    {
        // Destroy any active battle formations, should they exist
        if (FindObjectOfType<BattleFormation>())
        {
            currentEnemies = null;

            Destroy(FindObjectOfType<BattleFormation>().gameObject);
        }

        //enemy level is based on the hero with the highest level, then has some variance based on floor
        int highestLevel = FindObjectOfType<GameController>().allCharacters[0].level;

        foreach (Character levelCharacter in FindObjectOfType<GameController>().allCharacters)
        {
            if (levelCharacter.level > highestLevel)
            {
                highestLevel = levelCharacter.level;
            }
        }

        // Regular battle.
        if (!isBossBattle)
        {
            battleBackdrop.sprite = battleBackgrounds[0];

            //Floors 1-2 have enemies of level highest -> highest+2. Easy enemies only, and never more than 3 enemies (2-3).
            if (floorCounter.floorNumber < 3)
            {
                var newFormation = Instantiate(battleFormations[Random.Range(1, 3)], formationObject);

                BattleFormation currentFormation = newFormation.GetComponent<BattleFormation>();

                for (int i = 0; i < currentFormation.listOfEnemies.Length; i++)
                {
                    currentFormation.listOfEnemies[i].SetBattleEnemy(GenerateEasyEnemy(), Random.Range(highestLevel, highestLevel + 2));
                }

                currentEnemies = currentFormation;
            }
            else if (floorCounter.floorNumber < 5) //Floors 3-4 have enemies of level highest -> highest+4. Easy and medium enemies... Always 3-4 enemies.
            {
                var newFormation = Instantiate(battleFormations[Random.Range(2, 4)], formationObject);

                BattleFormation currentFormation = newFormation.GetComponent<BattleFormation>();

                for (int i = 0; i < currentFormation.listOfEnemies.Length; i++)
                {
                    int randEnemyPicker = Random.Range(0, 2);

                    if (randEnemyPicker == 0)
                    {
                        currentFormation.listOfEnemies[i].SetBattleEnemy(GenerateEasyEnemy(), Random.Range(highestLevel, highestLevel + 4));
                    }
                    else
                    {
                        currentFormation.listOfEnemies[i].SetBattleEnemy(GenerateMediumEnemy(), Random.Range(highestLevel, highestLevel + 4));
                    }
                }

                currentEnemies = currentFormation;
            }
            else if (floorCounter.floorNumber < 7) //Floors 5-6 have enemies of level highest+1 -> highest+4. Medium enemies only... Always 3-4 enemies.
            {
                var newFormation = Instantiate(battleFormations[Random.Range(2, 4)], formationObject);

                BattleFormation currentFormation = newFormation.GetComponent<BattleFormation>();

                for (int i = 0; i < currentFormation.listOfEnemies.Length; i++)
                {
                    currentFormation.listOfEnemies[i].SetBattleEnemy(GenerateMediumEnemy(), Random.Range(highestLevel + 1, highestLevel + 4));
                }

                currentEnemies = currentFormation;
            }
            else if (floorCounter.floorNumber < 9) //Floors 7-8 have enemies of level highest+2 -> highest+6 (or +4 for medium enemies). Medium and hard enemies. Always 3-4 enemies.
            {
                var newFormation = Instantiate(battleFormations[Random.Range(2, 4)], formationObject);

                BattleFormation currentFormation = newFormation.GetComponent<BattleFormation>();

                for (int i = 0; i < currentFormation.listOfEnemies.Length; i++)
                {
                    int randEnemyPicker = Random.Range(0, 2);

                    if (randEnemyPicker == 0)
                    {
                        currentFormation.listOfEnemies[i].SetBattleEnemy(GenerateMediumEnemy(), Random.Range(highestLevel + 2, highestLevel + 4));
                    }
                    else
                    {
                        currentFormation.listOfEnemies[i].SetBattleEnemy(GenerateDifficultEnemy(), Random.Range(highestLevel + 2, highestLevel + 6));
                    }
                }

                currentEnemies = currentFormation;
            }
            else //Floors 9-10 have enemies of levels highest+5 -> highest+10. Hard enemies only.
            {
                var newFormation = Instantiate(battleFormations[Random.Range(2, 4)], formationObject);

                BattleFormation currentFormation = newFormation.GetComponent<BattleFormation>();

                for (int i = 0; i < currentFormation.listOfEnemies.Length; i++)
                {
                    currentFormation.listOfEnemies[i].SetBattleEnemy(GenerateDifficultEnemy(), Random.Range(highestLevel + 5, highestLevel + 10));
                }

                currentEnemies = currentFormation;
            }
        }
        else // Set up the boss
        {
            battleBackdrop.sprite = battleBackgrounds[1];

            var newFormation = Instantiate(battleFormations[4], formationObject);

            BattleFormation currentFormation = newFormation.GetComponent<BattleFormation>();

            // Find the boss and set them up
            FindObjectOfType<Boss>().SetBoss(floorBoss, highestLevel);

            currentEnemies = currentFormation;
        }
        
        //set the exp granted for winning this battle.

        battleExp = 0;

        // Bosses have their own EXP... with enemies, add all their EXP values
        if (isBossBattle)
        {
            battleExp += FindObjectOfType<Boss>().baseExpGranted;
        }
        else
        {
            foreach (Enemy expGivingEnemy in currentEnemies.listOfEnemies)
            {
                battleExp += expGivingEnemy.baseExpGranted;
            }
        }
    }

    //Removes a dead enemy from the turn order, the button checker, and the battle as a whole.
    public void RemoveEnemy(Enemy deadEnemy)
    {
        turnOrder.Remove(deadEnemy);
        enemyButtons.Remove(deadEnemy.GetComponentInChildren<Button>(true));

        //nullify the enemy in the list of enemies
        for (int i = 0; i < currentEnemies.listOfEnemies.Length; i++)
        {
            if(currentEnemies.listOfEnemies[i] == deadEnemy)
            {
                currentEnemies.listOfEnemies[i] = null;
            }
        }
    }

    //Removes a dead enemy from the turn order, the button checker, and the battle as a whole.
    public void RemoveBoss(Boss deadBoss)
    {
        turnOrder.Remove(deadBoss);
        enemyButtons.Remove(deadBoss.GetComponentInChildren<Button>());
    }

    //These enemies appear on floors 1-4 and are leveled 1-8
    public Enemy GenerateEasyEnemy()
    {
        int pickEnemy = Random.Range(0, lowTierEnemies.Length);

        return lowTierEnemies[pickEnemy];
    }

    //These enemies appear on floors 4-9 and are leveled 9-19
    public Enemy GenerateMediumEnemy()
    {
        int pickEnemy = Random.Range(0, midTierEnemies.Length);

        return midTierEnemies[pickEnemy];
    }

    //These enemies appear on floors 6-10 and are leveled 20-30
    public Enemy GenerateDifficultEnemy()
    {
        int pickEnemy = Random.Range(0, highTierEnemies.Length);

        return highTierEnemies[pickEnemy];
    }

    public Boss GenerateBoss()
    {
        int pickBoss = Random.Range(0, bossEnemies.Length);

        return bossEnemies[pickBoss];
    }

    // Reset unit buttons and add the buttons within each character/enemy
    public void ResetUnitButtons()
    {
        enemyButtons.Clear();
        unitButtons.Clear();

        foreach (Unit thisUnit in turnOrder)
        {
            Button formattedButton = null;

            if (thisUnit.GetComponent<Character>())
            {
                formattedButton = thisUnit.GetComponent<Character>().chosenClass.GetComponentInChildren<Button>(true);

                unitButtons.Add(formattedButton);
            }
            else
            {
                formattedButton = thisUnit.GetComponentInChildren<Button>(true);

                enemyButtons.Add(formattedButton);
            }

            formattedButton.gameObject.SetActive(false);

            //ensures that the playerbuttons do not cast spells multiple times
            formattedButton.onClick.RemoveAllListeners();
            formattedButton.onClick.AddListener(() => UseDirectAbility(thisUnit));
        }
    }

    public void DeselectUnitButtons()
    {
        areaOfEffectAttackButton.gameObject.SetActive(false);
        areaOfEffectSupportButton.gameObject.SetActive(false);

        foreach (Button b in unitButtons)
        {
            b.gameObject.SetActive(false);
        }
        foreach (Button b in enemyButtons)
        {
            //dead enemy buttons disappear... check if null, first.
            if(b != null)
            {
                b.gameObject.SetActive(false);
            }
        }
    }

    public void CancelAction()
    {
        DeselectUnitButtons();
        RemoveEnemyActionText();
        unitCommandList.OpenCommandsMenu();
        cancelCommandButton.SetActive(false);
    }

    //creates the proper display for the current ability
    public void SelectSpell(int index)
    {
        cancelCommandButton.SetActive(true);
        unitCommandList.CloseCommandsMenu(true);

        ActivateEnemyActionText("Select a target.");

        selectedAbility = unitCommandList.heldSpells[index];
        selectedAbility.SelectAbility(unitInControl);
    }

    //creates the proper display for the current ability... this one is for the skill menu
    public void SelectSkill(int index)
    {
        cancelCommandButton.SetActive(true);
        unitCommandList.CloseCommandsMenu(true);

        ActivateEnemyActionText("Select a target.");

        selectedAbility = unitCommandList.heldSkills[index];
        selectedAbility.SelectAbility(unitInControl);
    }

    //creates the proper display for the current ability... this one is for a unique class skill
    public void SelectClassSkill()
    {
        cancelCommandButton.SetActive(true);
        unitCommandList.CloseCommandsMenu(true);
        ActivateEnemyActionText("Select a target.");

        selectedAbility = unitInControl.skillList[0];
        selectedAbility.SelectAbility(unitInControl);
    }

    public void SelectAttack()
    {
        cancelCommandButton.SetActive(true);
        unitCommandList.CloseCommandsMenu(true);

        ActivateEnemyActionText("Select a target.");

        selectedAbility = attackAbility;

        selectedAbility.SelectAbility(unitInControl);

        /*
        // "Area attack" weapons (Whips) hit all targets
        if (!unitInControl.GetComponent<Character>().equippedWeapon.areaAttack)
        {
            DisplayAreaEnemyButton();
        }
        else
        {
            DisplayEnemyButtons();
        }
        */
    }

    // During boss battles, there's only the one enemy, so just display theirs. Otherwise, use the larger display
    public void DisplayAreaEnemyButton()
    {
        DeselectUnitButtons();

        if (isBossBattle)
        {
            DisplayEnemyButtons();
        }
        else
        {
            areaOfEffectAttackButton.gameObject.SetActive(true);
        }
    }

    public void DisplayAreaAllyButton()
    {
        DeselectUnitButtons();

        areaOfEffectSupportButton.gameObject.SetActive(true);
    }

    public void DisplayUnitButtons()
    {
        DeselectUnitButtons();

        foreach (Button b in unitButtons)
        {
            b.gameObject.SetActive(true);
        }
    }

    public void DisplayEnemyButtons()
    {
        DeselectUnitButtons();

        foreach (Button b in enemyButtons)
        {
            if (b != null)
            {
                b.gameObject.SetActive(true);
            }
        }
    }

    //Check if all enemies are dead. If they are, you win! Not run if wonBattle is already true (see EnemyTakeDamage() in Enemy)
    public bool CheckForWonBattle()
    {
        bool FoundLivingEnemy = false;

        // In boss battles, just check if the boss has any remaining HP
        if (isBossBattle)
        {
            FoundLivingEnemy = FindObjectOfType<Boss>().currentHP > 0;
        }
        else
        {
            foreach (Enemy e in FindObjectsOfType<Enemy>())
            {
                if (e.currentHP > 0)
                {
                    FoundLivingEnemy = true;
                }
            }
        }

        return !FoundLivingEnemy;
    }

    //Since the characters appear above the level-up screen, they're made invisible when exiting a battle. This function re-adds them when beginning a new battle.
    public void AddCharactersToBattleScreen()
    {
        foreach(Character c in FindObjectOfType<GameController>().allCharacters)
        {
            c.transform.parent.localScale = new Vector3(18, 18, 1);
        }
    }

    //Since the characters appear above the level-up screen, they must be made invisible when the battle ends. Also, remove status effects.
    public void RemoveCharactersFromBattleScreen()
    {
        foreach (Character c in FindObjectOfType<GameController>().allCharacters)
        {
            c.RemoveAllStatusEffects();
            c.transform.parent.localScale = Vector3.zero;
        }
    }

    //Grant EXP and potentially level up characters. Close the battle window.
    public IEnumerator WinBattle()
    {
        wonBattle = true;

        ATB_Active = false;

        //remove the unit command list to prevent the player from selecting another action.
        unitCommandList.CloseCommandsMenu(false);

        // Make each hero happy!
        foreach (Character c in FindObjectsOfType<Character>())
        {
            c.unitFace.SetFace("Happy", 1f);
        }

        //wait 2 seconds before closing... after 1s, the levelUpScreen begins to appear.
        yield return new WaitForSeconds(1f);

        //remove character images and the unit command list so that the levelup screen is in front of other UI elements.
        RemoveCharactersFromBattleScreen();

        //animate levelScreen appearing
        levelScreen.gameObject.SetActive(true);
        levelScreen.InitializeLevelScreen(isBossBattle);

        // No pausing after all enemies are dead. It returns once you're back in the map
        FindObjectOfType<GameController>().pauseButton.interactable = false;

        //1.05f to ensure that the whole level screen animation occurs (it's 1 second long)
        yield return new WaitForSeconds(1.05f);

        FindObjectOfType<GameController>().ExitBattle();

        yield return null;
    }

    public void UnitSkipTurn()
    {
        unitInControl.ATB_CurrentCharge = 0f;

        cancelCommandButton.SetActive(false);
        unitCommandList.CloseCommandsMenu(true);
        DeselectUnitButtons();

        FindObjectOfType<BattleController>().ATB_Active = true;
    }

    public IEnumerator SelectedUnitUseAreaSupport()
    {
        unitInControl.GetComponent<Character>().unitFace.SetFace("Concentrating", 1.25f);

        if (!selectedAbility.GetComponent<Attack>())
        {
            unitInControl.GetComponent<Character>().characterAnim.SetTrigger("Cast");
        }
        else
        {
            unitInControl.GetComponent<Character>().characterAnim.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(1f);

        selectedAbility.UseAbility(unitInControl, null);

        yield return new WaitForSeconds(1f);

        if (!CheckForWonBattle())
        {
            unitInControl.ATB_CurrentCharge = 0f;
            ATB_Active = true;
        }

        yield return null;
    }

    public IEnumerator SelectedUnitUseAreaDamage()
    {
        unitInControl.GetComponent<Character>().unitFace.SetFace("Concentrating", 1.25f);

        // Mixed range weapons (whips) also attack using the Cast animation... their attack is an area attack
        if (!selectedAbility.GetComponent<Attack>() || unitInControl.GetComponent<Character>().equippedWeapon.mixedRangeWeapon)
        {
            unitInControl.GetComponent<Character>().characterAnim.SetTrigger("Cast");
        }
        else
        {
            unitInControl.GetComponent<Character>().characterAnim.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(1f);

        selectedAbility.UseAbility(unitInControl, null);

        yield return new WaitForSeconds(1f);

        if (!CheckForWonBattle())
        {
            unitInControl.ATB_CurrentCharge = 0f;
            ATB_Active = true;
        }

        yield return null;
    }

    public IEnumerator SelectedUnitUseDirectAbility(Unit target)
    {
        unitInControl.GetComponent<Character>().unitFace.SetFace("Concentrating", 1.25f);

        // Spells/skills and ranged weapons (Bows) use the Cast animation
        if (!selectedAbility.GetComponent<Attack>() || unitInControl.GetComponent<Character>().equippedWeapon.rangedWeapon)
        {
            unitInControl.GetComponent<Character>().characterAnim.SetTrigger("Cast");
        }
        else
        {
            unitInControl.GetComponent<Character>().characterAnim.SetTrigger("Attack");
        }

        if (selectedAbility.amountOfHits > 1)
        {
            int chanceOfHitting = selectedAbility.chanceOfMultihit;

            int totalDamageDealt = 0; // The total damage dealt by this multi-hit attack. Used in boss mechanic calculations when the Ability finishes

            // Manually reduce unit MP on the first cast so that the multiple hits don't also deplete MP.
            if (selectedAbility.GetComponent<Spell>())
            {
                unitInControl.GetComponent<Character>().currentMP -= selectedAbility.GetComponent<Spell>().MP_Cost;
            }

            for (int i = 0; i < selectedAbility.amountOfHits; i++)
            {
                int roll = Random.Range(1, 101);

                if(roll <= chanceOfHitting) // On successful rolls, use the Ability
                {
                    unitInControl.GetComponent<Character>().characterAnim.SetTrigger("Cast");

                    yield return new WaitForSeconds(1.1f);

                    if (selectedAbility.GetComponent<Spell>())
                    {
                        int previousEnemyHP = target.currentHP;

                        // Make the spell cost 0 MP on subsequent casts
                        int savedCost = selectedAbility.GetComponent<Spell>().MP_Cost;
                        selectedAbility.GetComponent<Spell>().MP_Cost = 0;

                        selectedAbility.UseAbility(unitInControl, target);

                        totalDamageDealt += (previousEnemyHP - target.currentHP); // add to totalDamageDealt

                        selectedAbility.GetComponent<Spell>().MP_Cost = savedCost;
                    }
                    else // Abilities have no MP cost
                    {
                        int previousEnemyHP = target.currentHP;

                        selectedAbility.UseAbility(unitInControl, target);

                        totalDamageDealt += (previousEnemyHP - target.currentHP); // add to totalDamageDealt
                    }

                    // White magic loses 50% per cast; other abilities only 20%
                    if (selectedAbility.GetComponent<WhiteMagic>())
                    {
                        chanceOfHitting -= 50;
                    }
                    else
                    {
                        chanceOfHitting -= 20;
                    }

                    //Select a new random target
                    List<Unit> livingEnemies = new List<Unit>();
                    List<Unit> livingAllies = new List<Unit>();

                    for (int j = 0; j < FindObjectsOfType<Unit>().Length; j++)
                    {
                        // Any boss/enemy with HP > 0 is fair game
                        if((FindObjectsOfType<Unit>()[j].GetComponent<Enemy>() || FindObjectsOfType<Unit>()[j].GetComponent<Boss>()) && FindObjectsOfType<Unit>()[j].currentHP > 0)
                        {
                            livingEnemies.Add(FindObjectsOfType<Unit>()[j]);
                        }

                        // For white magic, any living ally that wasn't already targeted is fair game
                        if (FindObjectsOfType<Unit>()[j].GetComponent<Character>() && FindObjectsOfType<Unit>()[j].currentHP > 0 && FindObjectsOfType<Unit>()[j] != target)
                        {
                            livingAllies.Add(FindObjectsOfType<Unit>()[j]);
                        }
                    }

                    //If there are living enemies, pick a random enemy. Otherwise, break, as all enemies are defeated.
                    if (livingEnemies.Count > 0)
                    {
                        // Multi-cast white magic
                        if (selectedAbility.GetComponent<WhiteMagic>())
                        {
                            target = livingAllies[Random.Range(0, livingAllies.Count)];
                        }
                        else
                        {
                            target = livingEnemies[Random.Range(0, livingEnemies.Count)];
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else // On a failure, break and check for boss mechanics that could have activated
                {
                    if (target.GetComponent<Boss>())
                    {
                        target.GetComponent<Boss>().CheckOnDamagedMechanics(unitInControl, totalDamageDealt, selectedAbility.associatedElement == "Physical", selectedAbility.associatedElement);
                    }

                    break;
                }
            }
        }
        else
        {
            yield return new WaitForSeconds(1f);

            int previousEnemyHP = target.currentHP;

            selectedAbility.UseAbility(unitInControl, target);

            int totalDamageDealt = (previousEnemyHP - target.currentHP); // add to totalDamageDealt

            if (target.GetComponent<Boss>() && totalDamageDealt > 1) // Spells that don't deal damage have no need to check damaged mechanics
            {
                // Attack has multiple elemental types based on weapon. Otherwise, just check the ability's associated element
                if(selectedAbility.abilityName== "Attack")
                {
                    target.GetComponent<Boss>().CheckOnDamagedMechanics(unitInControl, totalDamageDealt, unitInControl.GetComponent<Character>().equippedWeapon.weaponElement == "Physical", unitInControl.GetComponent<Character>().equippedWeapon.weaponElement);
                }
                else
                {
                    target.GetComponent<Boss>().CheckOnDamagedMechanics(unitInControl, totalDamageDealt, selectedAbility.associatedElement == "Physical", selectedAbility.associatedElement);
                }
            }
        }

        yield return new WaitForSeconds(1f);

        if (!CheckForWonBattle())
        {
            unitInControl.ATB_CurrentCharge = 0f;
            ATB_Active = true;
        }

        yield return null;
    }

    public void UseAreaSupport()
    {
        cancelCommandButton.SetActive(false);
        RemoveEnemyActionText();
        unitCommandList.CloseCommandsMenu(false);
        DeselectUnitButtons();

        StartCoroutine(SelectedUnitUseAreaSupport());
    }

    public void UseAreaAttack()
    {
        cancelCommandButton.SetActive(false);
        RemoveEnemyActionText();
        unitCommandList.CloseCommandsMenu(false);
        DeselectUnitButtons();

        StartCoroutine(SelectedUnitUseAreaDamage());
    }

    public void UseDirectAbility(Unit target)
    {
        cancelCommandButton.SetActive(false);
        RemoveEnemyActionText();
        unitCommandList.CloseCommandsMenu(false);
        DeselectUnitButtons();

        StartCoroutine(SelectedUnitUseDirectAbility(target));
    }

    public void ActivateEnemyActionText(Ability a)
    {
        //enemyActionText.SetActive(true);

        enemyActionText.GetComponentInChildren<TextMeshProUGUI>().SetText(a.abilityName);

        enemyActionText.GetComponent<Animator>().SetTrigger("TextEnter");
    }

    // Activate action text using any string
    public void ActivateEnemyActionText(string s)
    {
        //enemyActionText.SetActive(true);
        enemyActionText.GetComponentInChildren<TextMeshProUGUI>().SetText(s);
        
        enemyActionText.GetComponent<Animator>().SetTrigger("TextEnter");
    }

    public void RemoveEnemyActionText()
    {
        enemyActionText.GetComponent<Animator>().SetTrigger("TextExit");
    }
}
