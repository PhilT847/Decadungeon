using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Hero : MonoBehaviour
{
    public string className;
    public int classCode; // 00-09; used for saving purposes

    public Character classOwner;

    public SpriteRenderer faceSprite;

    public SpriteRenderer frontHair;
    public SpriteRenderer backHair;
    //the ear shows through the hair, and thus needs its own sprite so that it's colored correctly
    public SpriteRenderer earSprite;

    public SpriteRenderer[] skinSprites;
    public SpriteRenderer[] primaryColorSprites;
    public SpriteRenderer[] secondaryColorSprites;

    public GameObject barrierObject;
    public TextMeshProUGUI barrierText;

    //the sprites representing the unit's weapon and badge.
    public SpriteRenderer weaponSprite;
    public SpriteRenderer badgeSprite;

    //for some classes, the "weapon" sprite needs to rotate to make the hero look like they're holding the weapon correctly. See Character's EquipWeapon().
    public int rotateForBow;

    public Button unitButton;

    public Weapon startingWeapon;

    public bool hasClassSkill; //"true" for classes that only have one Skill, turning the Skill button in the command list into just that one skill
    public string specialActionName; // Changes "Skills" to something else for classes with unique abilities, like "Enchant" for Enchanters

    public Ability[] skillsLearned;
    public int[] learnSkillAtLevel;

    //Classes have their own growth bonuses on top of character growths
    public int classHP_Growth;
    public int classSTR_Growth;
    public int classMAG_Growth;
    public int classDEX_Growth;
    public int classFTH_Growth;

    public void HeroSetup(Character owner)
    {
        owner.chosenClass = this;

        //Knights and Liches start with 1 physical defense that increases based on level (see LevelUp()).
        if (owner.chosenClass.className == "Knight")
        {
            owner.physicalDefense = 1;
        }

        // Rogues deal triple damage on criticals. Other classes deal double damage.
        if (owner.chosenClass.className == "Rogue")
        {
            owner.criticalHitCoefficient = 3f;
        }
        else
        {
            owner.criticalHitCoefficient = 2f;
        }

        // Bonus attack power is 1 for all characters. Liches can increase this value by losing HP
        owner.bonusAttackPower = 1f;

        // Turn speed mult set to 1f
        owner.turnSpeedMultiplier = 1f;

        classOwner = owner;

        classOwner.level = 1;
        classOwner.exp = 0;
        classOwner.expToNextLevel = 100;

        classOwner.currentHP = classOwner.maxHP;
        classOwner.maxMP = classOwner.magic;

        // Add the class's growths to the character, giving them their combined "final" growth
        classOwner.hpGrowth += classHP_Growth;
        classOwner.strGrowth += classSTR_Growth;
        classOwner.magGrowth += classMAG_Growth;
        classOwner.dexGrowth += classDEX_Growth;
        classOwner.fthGrowth += classFTH_Growth;

        // Set up the unit's face
        classOwner.unitFace = GetComponentInChildren<Face>();
        classOwner.unitFace.AssignCharacterToFace(classOwner);

        //equip the starting weapon for your class. Includes Monks, which equip the "Fists" weapon
        GenerateStartingWeapon();

        //VERY HARD-CODED but it works... if a unit's base class wields a bow, then swap them to the back so they can deal maximum attack damage from the start.
        if (startingWeapon.rangedWeapon)
        {
            switch (classOwner.unitName)
            {
                case "Terra":
                    FindObjectOfType<PauseMenu>(true).statusWindow.SwapHeroPosition(0);
                    break;
                case "Brick":
                    FindObjectOfType<PauseMenu>(true).statusWindow.SwapHeroPosition(1);
                    break;
                case "Iris":
                    FindObjectOfType<PauseMenu>(true).statusWindow.SwapHeroPosition(2);
                    break;
                case "Leon":
                    FindObjectOfType<PauseMenu>(true).statusWindow.SwapHeroPosition(3);
                    break;
            }
        }

        //equipping staves increases MP, so wait until here to set current MP to max.
        classOwner.currentMP = classOwner.maxMP;

        // Monks use Chi instead of MP
        if (owner.chosenClass.className == "Monk")
        {
            owner.currentChi = 0;
            owner.maxChi = 3;

            owner.currentMP = 0;
            owner.maxMP = 0;
        }

        faceSprite.sprite = owner.faceAndHair[0];
        frontHair.sprite = owner.faceAndHair[5];
        backHair.sprite = owner.faceAndHair[6];
        earSprite.sprite = owner.faceAndHair[7];

        frontHair.color = owner.hairColor;
        backHair.color = owner.hairColor;

        foreach (SpriteRenderer skinSprite in skinSprites)
        {
            skinSprite.color = owner.skinTone;
        }

        foreach (SpriteRenderer primaryColorSprite in primaryColorSprites)
        {
            primaryColorSprite.color = owner.primaryColor;
        }

        foreach (SpriteRenderer secondaryColorSprite in secondaryColorSprites)
        {
            secondaryColorSprite.color = owner.secondaryColor;
        }

        owner.characterAnim = GetComponentInChildren<Animator>(true);

        //Look for extra animators within the character. Should one exist, assign the owner's extraAnim.
        foreach(Animator anim in GetComponentsInChildren<Animator>(true))
        {
            if (anim.CompareTag("ExtraAnimator"))
            {
                owner.extraAnim = anim;
            }
        }

        //Find the user's enchantment object and clear it.
        owner.weaponEnchant = GetComponentInChildren<Enchantment>(true);

        owner.unitBuffs = GetComponentInChildren<UnitBuffs>(true);

        //enchantments get too large when added to the new unit. Scale the object down.
        owner.weaponEnchant.transform.localScale = new Vector3(0.05f, 0.05f, 1f);

        owner.weaponEnchant.ClearEnchantments();

        //gain any level 1 "innate" skills.
        CheckForLearnedSkills(1);

        //ATB time is 8s baseline.
        owner.ATB_TimeUntilAttack = 10f;

        //ensures that anything that moves to the character also appears on the hero sprite in battle mode
        owner.transform.parent = transform;
        owner.transform.localPosition = Vector3.zero;
    }

    //Displays the hero in the HeroCreator
    public void DisplayHero(Character owner)
    {
        faceSprite.sprite = owner.faceAndHair[0];
        frontHair.sprite = owner.faceAndHair[5];
        backHair.sprite = owner.faceAndHair[6];
        earSprite.sprite = owner.faceAndHair[7];

        frontHair.color = owner.hairColor;
        backHair.color = owner.hairColor;

        GetComponentInChildren<Face>().AssignCharacterToFace(owner);

        foreach (SpriteRenderer skinSprite in skinSprites)
        {
            skinSprite.color = owner.skinTone;
        }

        foreach (SpriteRenderer primaryColorSprite in primaryColorSprites)
        {
            primaryColorSprite.color = owner.primaryColor;
        }

        foreach (SpriteRenderer secondaryColorSprite in secondaryColorSprites)
        {
            secondaryColorSprite.color = owner.secondaryColor;
        }
    }

    // Generates a starting weapon. Note that the SavedHeroLoader replaces this weapon if necessary
    void GenerateStartingWeapon()
    {
        var firstWeapon = Instantiate(startingWeapon, GameObject.FindGameObjectWithTag("ItemContainer").transform);

        // Monk "Fists" weapon starts at 2 might.
        if (className == "Monk")
        {
            firstWeapon.GetComponent<Weapon>().weaponMight = 2;
        }

        firstWeapon.GetComponent<Weapon>().EquipItem(classOwner);
    }

    public bool CheckForLearnedSkills(int characterLevel)
    {
        // Return whether or not the unit learned something new.
        bool foundNewSkill = false;

        for (int i = 0; i < learnSkillAtLevel.Length; i++)
        {
            if (learnSkillAtLevel[i] <= characterLevel)
            {
                if (skillsLearned[i].GetComponent<Spell>() && !classOwner.spellList.Contains(skillsLearned[i]))
                {
                    foundNewSkill = true;
                    classOwner.spellList.Add(skillsLearned[i]);
                    classOwner.SortSpells(); // Sort their spell list
                }
                else if (!classOwner.skillList.Contains(skillsLearned[i]) && !skillsLearned[i].GetComponent<Spell>() && !skillsLearned[i].GetComponent<Passive>()) // So long as it's not passive, add it to the list
                {
                    foundNewSkill = true;
                    classOwner.skillList.Add(skillsLearned[i]);
                }
                else if(skillsLearned[i].GetComponent<Passive>() && !classOwner.passiveList.Contains(skillsLearned[i])) // Passive... ensure it's not an already learned passive first
                {
                    foundNewSkill = true;
                    classOwner.passiveList.Add(skillsLearned[i]);
                }
            }
        }

        return foundNewSkill;
    }
}
