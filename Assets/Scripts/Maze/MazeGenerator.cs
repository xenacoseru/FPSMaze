using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MazeGenerator : MonoBehaviour {
  

    private DFSAlgoritmMaze BuilderMaze;
    private Vector3 IntialPositon;
    private Vector3 IntialPositonRoof;
    public List<Vector3> cellsGroundPositionSpawn;
    public int Height, Width;
    public GameObject Wall;
    public GameObject WallRoof;
    private GameObject wallHolder;
    private GameObject roof;
    public Cell[] cells;
    public float wallLength = 3f;



    private void Start()
    {
        BuilderMaze = GetComponent<DFSAlgoritmMaze>();
        IntialPositon = new Vector3((-Width / 2) + wallLength / 2, 0.0f, (-Height / 2) + wallLength / 2);
        IntialPositonRoof = new Vector3(-wallLength/2-1, wallLength,  -wallLength-1);
        cellsGroundPositionSpawn = new List<Vector3>();
    }

    public GameObject masterClient;
    public static int realSeed;
    public static int realSize;
    private bool generatedMaze = false;

    void Update()
    {
        if (PhotonNetwork.inRoom && realSeed != 0 && !generatedMaze)
        { //Make sure you are connected and have received the seed
          //Generate world based off of the seed
            Height = realSize;
            Width = realSize;
            CreateMazeWallsByMatrix(Height,Width,realSeed);
            generatedMaze = true;
        }
    }

    [PunRPC]
    void ReceiveSeed(int seed)
    { //Get the seed from the master client in an RPC
      //This RPC is called automatically since it's a "buffered" rpc
        realSeed = seed;
    }
    [PunRPC]
    void ReceiveSize(int size)
    { //Get the seed from the master client in an RPC
      //This RPC is called automatically since it's a "buffered" rpc
        realSize = size;
    }

    public void CreateMazeWallsByMatrix(int Height,int Width,int _seed)
    {
        wallHolder = new GameObject();
        roof = new GameObject();
        wallHolder.name = "The Maze";
        roof.name = "The roof Maze";
        Vector3 myPostion = IntialPositon;
        GameObject temp;
        int index = 0;

        for (int i = 0; i < Height; i++) // acoperis
        {
            for (int j = 0; j < Width; j++)
            {
                myPostion = new Vector3(IntialPositonRoof.x + (i * wallLength), IntialPositonRoof.y, IntialPositonRoof.z + (j * wallLength));
                //temp = Instantiate(WallRoof, myPostion, Quaternion.Euler(0f, 0f, 90f)) as GameObject;
                Vector3 positonOnGround = new Vector3(myPostion.x, 2.15f, myPostion.z);
                cellsGroundPositionSpawn.Add(positonOnGround);
                //temp.transform.parent = roof.transform;
            }
        }

        for (int i = 0; i < Height; i++) // pe linii -- x
        {
            for (int j = 0; j <= Width; j++)
            {
                myPostion = new Vector3(IntialPositon.x + (j * wallLength) - wallLength/2, 0.0f, IntialPositon.z + (i * wallLength) - wallLength /2);
                temp = Instantiate(Wall, myPostion, Quaternion.identity) as GameObject;
                temp.name = "Wall " + index;
                index += 1;
                temp.transform.parent = wallHolder.transform;
            }
        }

        for (int i = 0; i <= Height; i++)
        { // pe coloane
            for (int j = 0; j < Width; j++)
            {
                myPostion = new Vector3(IntialPositon.x + (j * wallLength), 0.0f, IntialPositon.z + (i * wallLength)-wallLength);
                temp = Instantiate(Wall, myPostion, Quaternion.Euler(0.0f, 90.0f, 0f)) as GameObject;
                temp.name = "WallY " + index;
                index += 1;
                temp.transform.parent = wallHolder.transform;

            }
        }
        wallHolder.transform.position += new Vector3(0f, wallLength/2, 0f);

        CreateCells(_seed);
    }

    void CreateCells(int _seed)
    {
        int childrenNo = wallHolder.transform.childCount;
        GameObject[] allWalls = new GameObject[childrenNo];
        cells = new Cell[Height*Width];
        
        //get all child
        for (int i = 0; i < childrenNo; i++)
        {
            allWalls[i] = wallHolder.transform.GetChild(i).gameObject;
        }

        int goNorthSouth = 0, curentRow = 0;
        for (int i = 0; i < cells.Length; i++)
        {            
            Cell newCell = new Cell();
            newCell.north = allWalls[goNorthSouth];            
            newCell.south = allWalls[goNorthSouth + 1];
            newCell.west = allWalls[i + Width * (Height + 1)];
            newCell.east = allWalls[i + Width * (Height + 1) + Width];
            if (curentRow == Height - 1)
            {
                newCell.isDown = true;
                goNorthSouth += 2;
                curentRow = 0;
            }
            else
            {
                if (curentRow == 0)
                {
                    newCell.isUp = true;
                }
                else
                {
                    newCell.isDown = false;
                    newCell.isUp = false;
                }
                curentRow++;
                goNorthSouth++;              
            }
            newCell.visited = false;
            cells[i] = newCell;
         }

        BuilderMaze.SetRandoMSeed(realSeed);
        StartCoroutine(BuilderMaze.GenerateMaze(cells, Width, Height));        
    }
}
