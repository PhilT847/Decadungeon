using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //since certain events can change the focus of the camera, this transform is a variable that can be altered with ChangeFocus().
    public Transform followedObject;
    private Vector3 velocity;

    //start by following the main character.
    private void Start()
    {
        followedObject = FindObjectOfType<FieldCharacter>().transform;
    }

    void Update()
    {
        //create a new Vector3 with a z of -10f (the base camera value)... otherwise, the camera will get too close to the screen
        Vector3 approachPosition = new Vector3(followedObject.position.x, followedObject.position.y, -10f);

        transform.position = Vector3.SmoothDamp(transform.position, approachPosition, ref velocity, 0.1f);
    }

    public IEnumerator ChangeFocus(Transform newFocus, float timeFocusedOn)
    {
        //save the character's transform so it can return.
        Transform originalFocus = followedObject;

        followedObject = newFocus;

        //follow this new focus for a specified amount of time before returning to the character
        yield return new WaitForSeconds(timeFocusedOn);

        followedObject = originalFocus;

        yield return null;
    }
}
