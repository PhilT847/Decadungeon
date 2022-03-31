using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldCharacter : MonoBehaviour
{
    public Room currentRoom;

    public Animator staircaseAnim;

    public void MoveInDirection(string direction)
    {
        switch (direction)
        {
            case "up":

                if(currentRoom.roomAbove != null)
                {
                    currentRoom = currentRoom.roomAbove;

                    StartCoroutine(MoveToTile());
                }

                break;

            case "right":

                if (currentRoom.roomToRight != null)
                {
                    currentRoom = currentRoom.roomToRight;

                    StartCoroutine(MoveToTile());
                }

                break;

            case "down":

                if (currentRoom.roomBelow != null)
                {
                    currentRoom = currentRoom.roomBelow;

                    StartCoroutine(MoveToTile());
                }

                break;

            case "left":

                if (currentRoom.roomToLeft != null)
                {
                    currentRoom = currentRoom.roomToLeft;

                    StartCoroutine(MoveToTile());
                }

                break;

        }
    }

    public IEnumerator CheckRoom()
    {
        yield return new WaitForSeconds(0.07f);

        currentRoom.GetComponent<Animator>().SetTrigger("EnterRoom");

        yield return new WaitForSeconds(0.33f);

        switch (currentRoom.thisRoomType)
        {
            case Room.RoomType.Encounter:
                if (FindObjectOfType<GameController>().DEBUG_MODE != 2)
                {
                    FindObjectOfType<GameController>().StartCoroutine(FindObjectOfType<GameController>().EnterBattle(false));
                }
                else
                {
                    FindObjectOfType<GameController>().EnableJoystick(false);
                }
                break;
            case Room.RoomType.Treasure:
                FindObjectOfType<TreasureRoom>(true).OpenTreasureRoom(FindObjectOfType<FloorBuilder>().floorNumber);
                break;
            case Room.RoomType.Boss:
                if (FindObjectOfType<GameController>().DEBUG_MODE != 2)
                {
                    FindObjectOfType<GameController>().StartCoroutine(FindObjectOfType<GameController>().EnterBattle(true));
                }
                else
                {
                    FindObjectOfType<GameController>().EnableJoystick(false);
                }
                break;
            case Room.RoomType.Staircase:
                StartCoroutine(DescendStairs());
                break;
            case Room.RoomType.Fountain:
                FindObjectOfType<FountainScreen>(true).OpenFountainMenu();
                break;
            case Room.RoomType.Nothing: //you can start moving again only if the tile has nothing on it
                FindObjectOfType<GameController>().EnableJoystick(false);
                break;
        }

        yield return null;
    }

    public IEnumerator DescendStairs()
    {
        staircaseAnim.SetTrigger("Descend");

        FindObjectOfType<GameController>().DisableJoystick(true);
        FindObjectOfType<Myrrh>().GetComponent<Animator>().SetTrigger("MyrrhExit");

        foreach (Character c in FindObjectsOfType<Character>(true))
        {
            c.FullRestore();
        }

        yield return new WaitForSeconds(0.75f);

        FindObjectOfType<FloorBuilder>().GenerateNewFloor(FindObjectOfType<FloorBuilder>().floorNumber + 1);

        yield return new WaitForSeconds(1f);

        FindObjectOfType<Myrrh>().GetComponent<Animator>().SetTrigger("MyrrhEnter");
        FindObjectOfType<GameController>().EnableJoystick(true);

        yield return null;
    }

    public IEnumerator MoveToTile()
    {
        //if the joystick is enabled, players can skip tiles as CheckRoom() isn't called yet
        FindObjectOfType<GameController>().DisableJoystick(false);

        Vector3 startingPos = transform.position;
        Vector3 finalPos = currentRoom.transform.position;

        float elapsedTime = 0;

        while (elapsedTime < 0.25f)
        {
            transform.position = Vector3.Lerp(startingPos, finalPos, elapsedTime / 0.25f);
            elapsedTime += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        transform.position = currentRoom.transform.position;

        StartCoroutine(CheckRoom());

        yield return null;
    }
}
