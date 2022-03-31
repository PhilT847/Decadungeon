using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : MonoBehaviour
{
    public SpriteRenderer faceSprite;

    // All potential faces
    public Sprite neutralFace;
    public Sprite blinkingFace;
    public Sprite damagedFace;
    public Sprite happyFace;
    public Sprite concentratingFace;

    private float blinkTimer;

    private void Start()
    {
        faceSprite.sprite = neutralFace;
        blinkTimer = Random.Range(4f,8f);
    }

    // Blink periodically
    private void Update()
    {
        if(blinkTimer > 0f)
        {
            blinkTimer -= Time.deltaTime;
        }
        else if(faceSprite.sprite == neutralFace) // You only blink when your face is already neutral
        {
            blinkTimer = Random.Range(3f, 6f);

            SetFace("Blinking", 0.15f);
        }
    }

    // Sets the sprites needed on this face. Used by HeroSetup() to generate the required character's face
    public void AssignCharacterToFace(Character c)
    {
        neutralFace = c.faceAndHair[0];
        blinkingFace = c.faceAndHair[1];
        damagedFace = c.faceAndHair[2];
        happyFace = c.faceAndHair[3];
        concentratingFace = c.faceAndHair[4];
    }

    // Calls the Coroutine below. Used by other classes for easier typing
    public void SetFace(string faceName, float duration)
    {
        // Ensure that you do not blink during this function... unless already blinking
        if(faceName != "Blinking")
        {
            blinkTimer += (duration + 1f);
        }

        switch (faceName)
        {
            case "Neutral":
                StartCoroutine(ChangeFaceForDuration(neutralFace, duration));
                break;
            case "Blinking":
                StartCoroutine(ChangeFaceForDuration(blinkingFace, duration));
                break;
            case "Damaged":
                StartCoroutine(ChangeFaceForDuration(damagedFace, duration));
                break;
            case "Happy":
                StartCoroutine(ChangeFaceForDuration(happyFace, duration));
                break;
            case "Concentrating":
                StartCoroutine(ChangeFaceForDuration(concentratingFace, duration));
                break;
        }
    }

    // Give the character a certain expression for a certain duration. Uses SetFace above to get the correct sprite
    public IEnumerator ChangeFaceForDuration(Sprite thisSprite, float duration)
    {
        faceSprite.sprite = thisSprite;

        yield return new WaitForSeconds(duration);

        // Return to neutral face after the duration, unless the sprite changed to another face during this time
        if(faceSprite.sprite == thisSprite)
        {
            // The faceSprite will only NOT be thisSprite unless this coroutine was run again. Allow THAT coroutine to return to normal face
            faceSprite.sprite = neutralFace;
        }

        yield return null;
    }
}
