using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Rooms
{
    public GameObject connection;
    public string direction;
    public string opposite;
    public bool connected = false;

    public Rooms()
    {

    }
}
