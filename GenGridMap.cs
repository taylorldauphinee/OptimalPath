using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenGridMap : MonoBehaviour
{
    //fields
    // the rooms available to be used
    [SerializeField] List<GameObject> rooms = new List<GameObject>();
    [SerializeField] GameObject startRoom;
    [SerializeField] GameObject endRoom;
    [SerializeField] GameObject bossRoom;

    string[,] gridMap;
    // List of directions for the path
    List<string> path = new List<string>();
    // List of rooms instantiated
    List<GameObject> roomPath = new List<GameObject>();

    // variables
    int numRooms = 12;
    bool isBoss = false;

    // variables to generate the map
    string direct;
    int random;
    bool roomFound = false;
    int gridX;
    int gridZ;
    // to add the cloned objects to the list
    RoomData thisRoom;
    GameObject currentRoom;
    // to instantiate cloned objects and line them up with the previous object
    Vector3 newPos;
    // exit hallways
    GameObject exitConn;

    // Start is called before the first frame update
    void Start()
    {
        // sets the newPos
        newPos = startRoom.transform.position;

        SetArrayPath();
    }

    // Update is called once per frame
    void Update()
    {
        // changes the generated path
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Reset();
        }
    }

    // set up the level array/grid to prevent crossover
    void SetArrayPath()
    {
        // set the grid for the level map
        gridMap = new string[numRooms + 2, numRooms + 2]; // add a border

        // ensure every grid space starts empty
        for (int x = 0; x < numRooms; x++)
        {
            for (int z = 0; z < numRooms; z++)
            {
                if (x == 0 || z == 0)
                {
                    gridMap[x, z] = "Border";
                }
                else
                {
                    gridMap[x, z] = null;
                }
            }
        }

        // place start room on the right hand side
        int randZ = Random.Range(1, numRooms - 1);
        gridMap[numRooms, randZ] = startRoom.GetComponent<RoomData>().connections[0].direction;
        path.Add(startRoom.GetComponent<RoomData>().connections[0].direction);
        gridX = numRooms;
        gridZ = randZ;
        // find the corresponding open grid
        GetSpace(path[0], gridX, gridZ);
        CheckBlocks();

        // lay out a path, keeping overlap from happening
        for (int i = 1; i < numRooms - 1; i++)
        {
            // choose one of the available directions
            try
            {
                ChooseDirection(gridX, gridZ);
            }
            catch (System.ArgumentOutOfRangeException) // to retry level generation if the path gets blocked completely
            {
                Debug.Log("Didn't Generate Properly");
                ResetPath();
                return;
            }
            // add the next direction of the path to the list
            gridMap[gridX, gridZ] = direct;
            path.Add(direct);
            GetSpace(path[i], gridX, gridZ);
            CheckBlocks();

            if (i >= numRooms - 4) // for last three rooms - check for direction to equal end/boss room opposite - then end loop
            {
                if (isBoss == false)
                {
                    if (direct == endRoom.GetComponent<RoomData>().connections[0].opposite)
                    {
                        break;
                    }
                }
                else
                {
                    if (direct == bossRoom.GetComponent<RoomData>().connections[0].opposite)
                    {
                        break;
                    }
                }
            }
        }

        // if the last direction is not the opposite of the final room - need a way to add extra if the last room in the loop still doesn't give the necessary direction
        if (isBoss == false)
        {
            if (path[path.Count - 1] != endRoom.GetComponent<RoomData>().connections[0].opposite)
            {
                while (direct != endRoom.GetComponent<RoomData>().connections[0].opposite)
                {
                    if (path.Count > numRooms + 3)
                    {
                        Debug.Log("Exceeded Room Cap");
                        ResetPath();
                        return;
                    }
                    try
                    {
                        ChooseDirection(gridX, gridZ);
                    }
                    catch (System.ArgumentOutOfRangeException) // to retry level generation if the path gets blocked completely
                    {
                        Debug.Log("Didn't Generate Properly");
                        ResetPath();
                        return;
                    }
                    // add next direction of the path to the list
                    gridMap[gridX, gridZ] = direct;
                    path.Add(direct);
                    GetSpace(path[path.Count - 1], gridX, gridZ);
                    CheckBlocks();
                }
            }
        }
        else
        {
            if (path[path.Count - 1] != bossRoom.GetComponent<RoomData>().connections[0].opposite)
            {
                while (direct != bossRoom.GetComponent<RoomData>().connections[0].opposite)
                {
                    if (path.Count > numRooms + 3) // retry level generation if room count exceeds allotted number of rooms
                    {
                        Debug.Log("Exceeded Room Cap");
                        ResetPath();
                        return;
                    }
                    try
                    {
                        ChooseDirection(gridX, gridZ);
                    }
                    catch (System.ArgumentOutOfRangeException) // to retry level generation if the path gets blocked completely
                    {
                        Debug.Log("Didn't Generate Properly");
                        ResetPath();
                        return;
                    }
                    gridMap[gridX, gridZ] = direct;
                    path.Add(direct);
                    GetSpace(path[path.Count - 1], gridX, gridZ);
                    CheckBlocks();
                }
            }
            if (gridMap[gridX - 1, gridZ - 1] != null || gridMap[gridX, gridZ - 1] != null || gridMap[gridX + 1, gridZ - 1] != null) // check if the line below the boss room is clear
            {
                Debug.Log("Boss Room blocked");
                ResetPath();
                return;
            }
        }

        GenMap();
    }

    string ChooseDirection(int posX, int posZ) // choose the next random direction to go
    {
        List<string> pathOption = new List<string>();
        if (gridX - 1 > -1)
        {
            if (gridMap[gridX - 1, gridZ] == null)
            {
                pathOption.Add("Left");
            }
        }
        if (gridX + 1 < numRooms)
        {
            if (gridMap[gridX + 1, gridZ] == null)
            {
                pathOption.Add("Right");
            }
        }
        if (gridZ - 1 > -1)
        {
            if (gridMap[gridX, gridZ - 1] == null)
            {
                pathOption.Add("Down");
            }
        }
        if (gridZ + 1 < numRooms)
        {
            if (gridMap[gridX, gridZ + 1] == null)
            {
                pathOption.Add("Up");
            }
        }
        random = Random.Range(0, pathOption.Count);
        switch (random)
        {
            case 0:
                direct = pathOption[0];
                break;
            case 1:
                direct = pathOption[1];
                break;
            case 2:
                direct = pathOption[2];
                break;
            case 3:
                direct = pathOption[3];
                break;
        }
        return direct;
    }

    void GetSpace(string direction, int posX, int posZ) // move to the next grid space
    {
        switch (direction)
        {
            case "Left":
                posX -= 1;
                break;
            case "Right":
                posX += 1;
                break;
            case "Up":
                posZ += 1;
                break;
            case "Down":
                posZ -= 1;
                break;
        }
        gridX = posX;
        gridZ = posZ;
    }

    void CheckBlocks() // check if any empty blocks have become unusable
    {
        for (int x = 0; x < numRooms; x++)
        {
            for (int z = 0; z < numRooms; z++)
            {
                if (gridMap[x, z] == null)
                {
                    int blocked = 0;
                    if (gridMap[x - 1, z] != null)
                    {
                        blocked++;
                    }
                    if (gridMap[x + 1, z] != null)
                    {
                        blocked++;
                    }
                    if (gridMap[x, z - 1] != null)
                    {
                        blocked++;
                    }
                    if (gridMap[x, z + 1] != null)
                    {
                        blocked++;
                    }
                    if (blocked >= 3)
                    {
                        gridMap[x, z] = "Unusable";
                    }
                }
            }
        }
    }

    void GenMap() // grabs and instantiates the rooms based on the path
    {
        // create a clone of the start room
        currentRoom = Instantiate(startRoom);
        roomPath.Add(currentRoom);
        currentRoom.GetComponent<RoomData>().connections[0].connected = true;
        exitConn = currentRoom.GetComponent<RoomData>().connections[0].connection;
        for (int i = 0; i < path.Count - 1; i++)
        {
            // search for a room that fits the path
            for (int z = 0; z < rooms.Count; z++)
            {
                thisRoom = rooms[z].GetComponent<RoomData>();
                for (int x = 0; x < thisRoom.GetComponent<RoomData>().connections.Count; x++)
                {
                    if (thisRoom.connections[x].opposite == path[i])
                    {
                        for (int y = 0; y < thisRoom.GetComponent<RoomData>().connections.Count; y++)
                        {
                            if (thisRoom.connections[y].direction == path[i + 1] && y != x)
                            {
                                // create the fitting room and set position to line up with the previous room's hallway
                                currentRoom = Instantiate(rooms[z], newPos, rooms[z].transform.localRotation);
                                SetPosition(exitConn, currentRoom.GetComponent<RoomData>().connections[x].connection);
                                currentRoom.transform.position += newPos;
                                roomPath.Add(currentRoom);
                                exitConn = currentRoom.GetComponent<RoomData>().connections[y].connection;
                                currentRoom.GetComponent<RoomData>().connections[x].connected = true;
                                currentRoom.GetComponent<RoomData>().connections[y].connected = true;
                                roomFound = true;
                                break;
                            }
                        }
                        if (roomFound == true)
                        {
                            break;
                        }
                    }
                }
                if (roomFound == true)
                {
                    break;
                }
            }
            roomFound = false; // reset for the next path
        }
        // create the final room
        if (isBoss == false)
        {
            currentRoom = Instantiate(endRoom);
            SetPosition(exitConn, currentRoom.GetComponent<RoomData>().connections[0].connection);
            currentRoom.transform.position += newPos;
        }
        else
        {
            currentRoom = Instantiate(bossRoom);
            SetPosition(exitConn, currentRoom.GetComponent<RoomData>().connections[0].connection);
            currentRoom.transform.position += newPos;
        }
        roomPath.Add(currentRoom);
        currentRoom.GetComponent<RoomData>().connections[0].connected = true;
    }

    Vector3 SetPosition(GameObject lastRoom, GameObject thisRoom) // connect the rooms by the connector hallways
    {
        newPos = lastRoom.transform.position - thisRoom.transform.position;
        return newPos;
    }

    void ResetPath() // if the path cannot continue for any reason
    {
        path.Clear();
        SetArrayPath();
    }

    void Reset() // to create new dungeons
    {
        path.Clear();
        //remove the old rooms
        for (int i = 0; i < roomPath.Count; i++)
        {
            Destroy(roomPath[i]);
        }
        roomPath.Clear();
        //randomly set number of rooms and isBoss
        numRooms = Random.Range(10, 16);
        if (Random.value >= 0.8f)
        {
            isBoss = true;
        }
        else
        {
            isBoss = false;
        }
        //isBoss = true; // for testing purposes
        // resets the position for cloning and placing rooms
        newPos = startRoom.transform.position;

        SetArrayPath();
    }
}
