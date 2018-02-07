using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cell
{
    public bool visited;
    public GameObject north;
    public GameObject east;
    public GameObject west;
    public GameObject south;
    public bool isDown;
    public bool isUp;
}
