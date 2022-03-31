using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloorBuilder : MonoBehaviour
{
    public int floorNumber;

    public GameObject roomPrefab;

    public List<Room> roomsOnFloor;

    public Sprite[] tileSprites;

    private int mainHallwayLength; //allows the builder to generate vertical hallways from the main horizontal hallway only

    public TextMeshProUGUI floorNumberCounter;

    public BattleController battleControls;

    public SpriteRenderer floorSprite;
    public Sprite[] allFloors;

    void Start()
    {
        // On the first floor, randomize the boss cycle. If loading, however, use the boss indicated in the Savefile
        if (!FindObjectOfType<Savefile>().playingSavedGame)
        {
            battleControls.bossIndex = Random.Range(0, battleControls.bossEnemies.Length);
            GenerateNewFloor(1);
        }
        else
        {
            battleControls.bossIndex = FindObjectOfType<Savefile>().forcedBossIndex - 1;
            GenerateNewFloor(FindObjectOfType<Savefile>().forcedFloorNumber);

            // No longer any need to say that you're playing a saved game (characters and map are already loaded)
            FindObjectOfType<Savefile>().playingSavedGame = false;
        }
    }

    public void GenerateNewFloor(int newFloorNumber)
    {
        floorNumber = newFloorNumber;

        floorNumberCounter.SetText("{0}", floorNumber);

        //since a floor might already exist, start by clearing the current floor.
        DeleteCurrentFloor();

        var firstRoom = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity, this.transform);

        roomsOnFloor.Add(firstRoom.GetComponent<Room>());

        // Halls have 3 enemies at Floor 1, increasing to 4 on Floor 3, 5 on Floor 6, and 6 on Floor 9
        int hallLength = 4 + (floorNumber / 3);

        mainHallwayLength = hallLength;

        for (int i = 0; i < hallLength; i++)
        {
            var newRoom = Instantiate(roomPrefab, new Vector2(4 + (4 * i), 0f), Quaternion.identity, this.transform);

            roomsOnFloor.Add(newRoom.GetComponent<Room>());

            newRoom.GetComponent<Room>().roomToLeft = roomsOnFloor[roomsOnFloor.IndexOf(newRoom.GetComponent<Room>()) - 1];

            roomsOnFloor[roomsOnFloor.IndexOf(newRoom.GetComponent<Room>()) - 1].roomToRight = newRoom.GetComponent<Room>();

            PopulateWithEnemies(newRoom.GetComponent<Room>());

            //the last room in the hallway is always a boss (or an encounter on the first 2 floors)... either way, the room to the right of it is a staircase to the next floor. Unless you're on floor 10, in which there's just the final boss and no staircase
            if (i == hallLength - 1)
            {
                SelectBoss(); // First, select a boss for the floor

                // Populate the "last room" with a boss before adding a staircase room to the right. Unless on floor 10.
                PopulateWithBoss(newRoom.GetComponent<Room>());

                if (floorNumber < 10)
                {
                    var staircaseRoom = Instantiate(roomPrefab, new Vector2(8 + (4 * i), 0f), Quaternion.identity, this.transform);

                    roomsOnFloor.Add(staircaseRoom.GetComponent<Room>());

                    staircaseRoom.GetComponent<Room>().roomToLeft = newRoom.GetComponent<Room>();

                    newRoom.GetComponent<Room>().roomToRight = staircaseRoom.GetComponent<Room>();

                    staircaseRoom.GetComponent<Room>().thisRoomType = Room.RoomType.Staircase;

                    staircaseRoom.GetComponentInChildren<SpriteRenderer>().sprite = tileSprites[6];
                }
            }
        }

        FindObjectOfType<FieldCharacter>().currentRoom = firstRoom.GetComponent<Room>();
        FindObjectOfType<FieldCharacter>().transform.position = firstRoom.transform.position;

        // Add hallway sprites as well as treasure rooms
        //AddVerticalHallways();
        AddTreasureAndHallways();

        // Set the sprite. First floor is always the purple color
        if(floorNumber == 1)
        {
            floorSprite.sprite = allFloors[0];
        }
        else
        {
            floorSprite.sprite = allFloors[Random.Range(0, allFloors.Length)];
        }
    }

    void SelectBoss()
    {
        // Cycle through each boss
        battleControls.bossIndex++;

        if (battleControls.bossIndex >= battleControls.bossEnemies.Length)
        {
            battleControls.bossIndex = 0;
        }

        Boss selectedBoss = battleControls.bossEnemies[battleControls.bossIndex];

        battleControls.floorBoss = selectedBoss;
    }

    void DeleteCurrentFloor()
    {
        roomsOnFloor.Clear();

        foreach (Room existingRoom in FindObjectsOfType<Room>())
        {
            Destroy(existingRoom.gameObject);
        }
    }

    // Note that AddTreasureRooms() adds treasure rooms ADDITIONAL to the one given at the start
    // There are always 3 treasure rooms on a floor, meaning 2 more are added along with the one above the first room
    void AddTreasureAndHallways()
    {
        // First, add a fountain below the pre-boss room
        GenerateFountainRoom();

        // Create indices for each treasure room, ensuring that they're all different. There can only be one chest at the end of the hall, as there's always a fountain there.
        int firstTreasureRoomIndex = 0;
        int secondTreasureRoomIndex = Random.Range(1, mainHallwayLength - 1);
        int thirdTreasureRoomIndex = Random.Range(1, mainHallwayLength);

        // For each room, determine whether it's going above or below and then add each room
        for(int i = 0; i < 3; i++)
        {
            // Figure out which room this treasure is going in before putting it above or below
            int chosenRoom = firstTreasureRoomIndex;

            if (i == 1)
            {
                chosenRoom = secondTreasureRoomIndex;
            }
            else if (i == 2)
            {
                chosenRoom = thirdTreasureRoomIndex;
            }

            int positionRoll = Random.Range(0, 2);

            // Use the chosen position. However, if there's already a treasure room there, switch it to the opposite side
            if(positionRoll == 1)
            {
                if(roomsOnFloor[chosenRoom].roomAbove == null)
                {
                    AddTreasureAbove(roomsOnFloor[chosenRoom]);
                }
                else
                {
                    AddTreasureBelow(roomsOnFloor[chosenRoom]);
                }
            }
            else
            {
                if (roomsOnFloor[chosenRoom].roomBelow == null)
                {
                    AddTreasureBelow(roomsOnFloor[chosenRoom]);
                }
                else
                {
                    AddTreasureAbove(roomsOnFloor[chosenRoom]);
                }
            }
        }

        if (FindObjectOfType<Savefile>().playingSavedGame)
        {
            RemoveExtraTreasureAndEncounters();
        }

        // Add hallways here
        foreach (Room completedRoom in roomsOnFloor)
        {
            completedRoom.AddHallways();
        }
    }

    // Create a fountain room before the boss encounter
    void GenerateFountainRoom()
    {
        Room hallRoom = roomsOnFloor[mainHallwayLength - 1];

        // Create a fountain room
        var newRoom = Instantiate(roomPrefab, new Vector2(hallRoom.transform.position.x, -4f), Quaternion.identity, this.transform);

        roomsOnFloor.Add(newRoom.GetComponent<Room>());

        newRoom.GetComponent<Room>().roomAbove = hallRoom;
        hallRoom.roomBelow = newRoom.GetComponent<Room>();

        PopulateWithFountain(newRoom.GetComponent<Room>());
    }

    void AddTreasureAbove(Room baseRoom)
    {
        // Create an upper room
        var newRoom = Instantiate(roomPrefab, new Vector2(baseRoom.transform.position.x, 4f), Quaternion.identity, this.transform);

        roomsOnFloor.Add(newRoom.GetComponent<Room>());

        newRoom.GetComponent<Room>().roomBelow = baseRoom;
        baseRoom.roomAbove = newRoom.GetComponent<Room>();

        PopulateWithTreasure(newRoom.GetComponent<Room>());
    }

    void AddTreasureBelow(Room baseRoom)
    {
        // Create a lower room
        var newRoom = Instantiate(roomPrefab, new Vector2(baseRoom.transform.position.x, -4f), Quaternion.identity, this.transform);

        roomsOnFloor.Add(newRoom.GetComponent<Room>());

        newRoom.GetComponent<Room>().roomAbove = baseRoom;
        baseRoom.roomBelow = newRoom.GetComponent<Room>();

        PopulateWithTreasure(newRoom.GetComponent<Room>());
    }

    //returns a room with treasure/boss/enemy to an empty state after completion.
    public void ClearRoom(Room thisRoom)
    {
        thisRoom.thisRoomType = Room.RoomType.Nothing;

        thisRoom.GetComponentInChildren<SpriteRenderer>().sprite = tileSprites[0];

        thisRoom.bossImage.color = Color.clear;
    }

    void PopulateWithEnemies(Room thisRoom)
    {
        thisRoom.thisRoomType = Room.RoomType.Encounter;

        thisRoom.GetComponentInChildren<SpriteRenderer>().sprite = tileSprites[1];
    }

    void PopulateWithTreasure(Room thisRoom)
    {
        thisRoom.thisRoomType = Room.RoomType.Treasure;

        thisRoom.GetComponentInChildren<SpriteRenderer>().sprite = tileSprites[2];
    }

    void PopulateWithFountain(Room thisRoom)
    {
        thisRoom.thisRoomType = Room.RoomType.Fountain;

        thisRoom.GetComponentInChildren<SpriteRenderer>().sprite = tileSprites[3];
    }

    void PopulateWithBoss(Room thisRoom)
    {
        thisRoom.thisRoomType = Room.RoomType.Boss;

        //the final (floor 10) boss has its own unique tile sprite.
        if(floorNumber < 10)
        {
            thisRoom.GetComponentInChildren<SpriteRenderer>().sprite = tileSprites[4];
        }
        else
        {
            thisRoom.GetComponentInChildren<SpriteRenderer>().sprite = tileSprites[5];
        }

        thisRoom.bossImage.sprite = battleControls.floorBoss.bossHead;
    }

    // Returns "1" if there's a boss still alive on this floor. Used for saving
    public int BossOnFloor()
    {
        int foundBoss = 0;

        foreach (Room hallRoom in roomsOnFloor)
        {
            if (hallRoom.thisRoomType == Room.RoomType.Boss)
            {
                foundBoss = 1;
            }
        }

        return foundBoss;
    }

    // Count the amount of treasure rooms remaining on this floor. Used for saving
    public int CountRoomsWithTreasure()
    {
        int treasureRooms = 0;

        foreach(Room hallRoom in roomsOnFloor)
        {
            if(hallRoom.thisRoomType == Room.RoomType.Treasure)
            {
                treasureRooms++;
            }
        }

        return treasureRooms;
    }

    // Count the amount of encounters remaining on this floor. Used for saving
    public int CountRoomsWithEncounters()
    {
        int encounterRooms = 0;

        foreach (Room hallRoom in roomsOnFloor)
        {
            if (hallRoom.thisRoomType == Room.RoomType.Encounter)
            {
                encounterRooms++;
            }
        }

        return encounterRooms;
    }

    // When reloading a floor, remove any treasure/encounters/bosses already completed
    void RemoveExtraTreasureAndEncounters()
    {
        int encountersRemaining = FindObjectOfType<Savefile>().allowableEncounters;
        int treasureRemaining = FindObjectOfType<Savefile>().allowableTreasureChests;
        int bossesRemaining = FindObjectOfType<Savefile>().allowableBossRooms;

        // Encounters/bosses are removed in order. Treasure is removed end-first so that players can't reload with the first treasure room open automatically
        foreach (Room hallRoom in roomsOnFloor)
        {
            if (hallRoom.thisRoomType == Room.RoomType.Encounter)
            {
                if(encountersRemaining > 0)
                {
                    encountersRemaining--;
                }
                else
                {
                    ClearRoom(hallRoom);
                }
            }
            else if (hallRoom.thisRoomType == Room.RoomType.Boss)
            {
                if (bossesRemaining > 0)
                {
                    bossesRemaining--;
                }
                else
                {
                    ClearRoom(hallRoom);
                }
            }
        }

        // Remove treasure, starting with rooms at the beginning of the hallway
        for(int i = 0; i < FindObjectsOfType<Room>().Length; i++)
        {
            if (FindObjectsOfType<Room>()[i].thisRoomType == Room.RoomType.Treasure)
            {
                if (treasureRemaining > 0)
                {
                    treasureRemaining--;
                }
                else
                {
                    ClearRoom(FindObjectsOfType<Room>()[i]);
                }
            }
        }
    }
}
