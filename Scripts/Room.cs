using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public enum RoomType { Nothing, Encounter, Treasure, Fountain, Boss, Staircase };

    public RoomType thisRoomType;

    public Room roomAbove;
    public Room roomToRight;
    public Room roomBelow;
    public Room roomToLeft;

    public SpriteRenderer hallAbove;
    public SpriteRenderer hallToRight;
    public SpriteRenderer hallBelow;
    public SpriteRenderer hallToLeft;

    public SpriteRenderer bossImage;

    //Adds lines as hallways to rooms that are already set up
    public void AddHallways()
    {
        if(roomAbove != null)
        {
            hallAbove.color = new Color32(20, 0, 50, 255);
        }

        if (roomToRight != null)
        {
            hallToRight.color = new Color32(20, 0, 50, 255);
        }

        if (roomBelow != null)
        {
            hallBelow.color = new Color32(20, 0, 50, 255);
        }

        if (roomToLeft != null)
        {
            hallToLeft.color = new Color32(20, 0, 50, 255);
        }
    }
}
