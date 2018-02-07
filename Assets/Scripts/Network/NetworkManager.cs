using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using System;
using System.Collections.Generic;
using SocketIO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class NetworkManager : Photon.MonoBehaviour
{

    private const string roomName = "RoomName";
    private  static TypedLobby lobbyName = new TypedLobby("New_Lobby", LobbyType.Default);
    public static RoomInfo[] roomsList;
    public GameObject player;
    public GameObject monsterAI;
    public GameObject coinS;
    private GameObject exitGameObject;
    private GameObject coinsGameObject;

    public static GameObject standbyCamera;
    public GameObject exit;
    public GameObject waitPanel;
    private GameObject waitPanelInitilized;
    public bool offlinemode = false;
    public static Dictionary<string,int>  mapRoomNameSize = new Dictionary<string, int>();
    

    //Tab
    private ScoreManager scoreManager;

    //Messages provided by spawn death 
    private bool gamestart=false;
    Queue<string> messages= new Queue<string>();
    const int messageCount = 6;

    private Text messageWindow;
    private Text timeWindow;

    //time stat
    private float _timeRemaining;

    public float TimeRemaining
    {
        get { return _timeRemaining; }
        set { _timeRemaining = value; }
    }

    private float maxTime = 3 * 60; // In seconds.



    //Map sync
    bool sent;
    int seed;

    private SocketIOComponent SocketIO;
    private void Awake()
    {
        standbyCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }
    void Start()
    {
        SocketIO = GameObject.Find("SetupSocketConnectionToGame").GetComponent<SocketIOComponent>();

        scoreManager = GameObject.Find("ScoreBoard").GetComponent<ScoreManager>();
        Connect();
    }

    public void Connect()
    {
        if (offlinemode)
        {
            PhotonNetwork.offlineMode = true;
            OnJoinedLobby();
        }
        else
        {
            TimeRemaining = maxTime;
            PhotonNetwork.ConnectUsingSettings("v4.3");
        }
    }


    public GameObject worldGen;


    private bool inmatch = false;

    //TO DO ONE TIME ACTIVATION OF MASTER CLIENT MAZE GENERATION WHEN OTHER MASTER GOING OUT:))
    void Update()
    {
        if (PhotonNetwork.inRoom && PhotonNetwork.isMasterClient && !sent)
        {
            // If i'm in a room, the master client, and i haven't already sent the seed
            sent = true;
            seed = Guid.NewGuid().GetHashCode();
            MazeGenerator.realSeed = seed;
            MazeGenerator.realSize = mapRoomNameSize[PhotonNetwork.room.Name];

            worldGen.GetComponent<PhotonView>().RPC("ReceiveSeed", PhotonTargets.OthersBuffered, seed);
            worldGen.GetComponent<PhotonView>().RPC("ReceiveSize", PhotonTargets.OthersBuffered, mapRoomNameSize[PhotonNetwork.room.Name]);

            //Others buffered means that anyone who joins later will get this RPC
         

        }
        if (PhotonNetwork.inRoom && PhotonNetwork.room.PlayerCount == PhotonNetwork.room.MaxPlayers && gamestart == false)
        {
            Destroy(waitPanelInitilized);
            gamestart = true;
            StartCoroutine(GameIsReadyToPlay());
            scoreManager.InsertDataInScoreBoard();
        }
        if (inmatch)
        {
            WindowManager.checkNow = true;
            TimeRemaining -= Time.deltaTime;
            if (TimeRemaining <= 0)
            {
                //TO DO GAME OVER
                if (PhotonNetwork.inRoom && PhotonNetwork.isMasterClient)
                {
                    GetComponent<PhotonView>().RPC("EndGame", PhotonTargets.All);
                }
            }
            else
            {
                UpdateTime(TimeRemaining);
            }
        }



    }

    void StartSpawnProcess(object[] parameters)
    {
        StartCoroutine("SpawnPlayer", parameters);
    }



    public static void CreateRoom(string roomName,int _maxPlayers,int sizeMaze)
    {
        mapRoomNameSize.Add(roomName, sizeMaze);
        PhotonNetwork.playerName = UserData.userName;
        PhotonNetwork.CreateRoom(roomName, new RoomOptions() { MaxPlayers = (byte)_maxPlayers, IsOpen = true, IsVisible = true, RealSize = sizeMaze }, lobbyName);
    }

    public static void JoinRoom(string roomName)
    {
        PhotonNetwork.playerName = UserData.userName;
        PhotonNetwork.JoinRoom(roomName);
    }
         

    void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(lobbyName);
    }

    void OnReceivedRoomListUpdate()
    {
        Debug.Log("Room was created");
        roomsList = PhotonNetwork.GetRoomList();
    }

    void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
    }

    void OnJoinedRoom()
    {
        Debug.Log("Connected to Room");
        if (PhotonNetwork.room.PlayerCount < PhotonNetwork.room.MaxPlayers)
        {
            waitPanelInitilized = Instantiate(waitPanel);
            waitPanelInitilized.transform.parent = GameObject.Find("CanvasMenu").transform;
            waitPanelInitilized.gameObject.transform.position = GameObject.Find("CanvasMenu").transform.position;
        }

    }

    private bool initSpawn = true;
    IEnumerator SpawnPlayer(object[] parameters)
    {
        float respawnTime = (float) parameters[0];
        
        int index = (int) parameters[1];
        yield return new WaitForSeconds(respawnTime);
        Vector3 initialSpawnPoint;
        if (initSpawn)
        {
            initialSpawnPoint = worldGen.GetComponent<MazeGenerator>().cellsGroundPositionSpawn[index]+new Vector3(0f,15f,0f);
            initSpawn = false;
        }
        else
        {
            initialSpawnPoint = worldGen.GetComponent<MazeGenerator>().cellsGroundPositionSpawn[index];
        }

        GameObject myPlayer = PhotonNetwork.Instantiate(player.name, initialSpawnPoint, Quaternion.identity, 0); // spawneaza la toti
        myPlayer.transform.Find("FirstPersonCharacter").gameObject.SetActive(true);
        myPlayer.transform.Find("HealthCrosshair").gameObject.SetActive(true);
        myPlayer.GetComponent<FirstPersonController>().enabled = true;
        myPlayer.GetComponent<PlayerMovement>().enabled = true;
        myPlayer.GetComponent<NetworkCharacter>().enabled = true;
        myPlayer.GetComponent<PlayerShoting>().enabled = true;
        myPlayer.GetComponent<Health>().enabled = true;
        myPlayer.GetComponent<PlayDetection>().enabled = true;

        myPlayer.GetComponent<Health>().RespawnMe += StartSpawnProcess;
        myPlayer.GetComponent<Health>().SendNetworkMessage += AddMessage;
        

        AddMessage("Spawned player: " + UserData.userName);

    }


    IEnumerator GameIsReadyToPlay()
    {

        GameObject temp = GameObject.Find("Network").gameObject.transform.GetChild(0).gameObject;
        temp.SetActive(true);
        SetMessages(temp);
        
        //CREATE SPAWN POINTS
        standbyCamera.SetActive(false);
        yield return new WaitForSeconds(0.5f);


        //Random.InitState(MazeGenerator.realSeed);
        object[] parameters = new object[2] { 0f, 0 };

        StartCoroutine("SpawnPlayer", parameters);

        if (PhotonNetwork.inRoom && PhotonNetwork.isMasterClient)
        {
            StartCoroutine(GenerateExitByCallingRpc());
            GameObject tmp = PhotonNetwork.Instantiate(monsterAI.name, worldGen.GetComponent<MazeGenerator>().cellsGroundPositionSpawn[worldGen.GetComponent<MazeGenerator>().cellsGroundPositionSpawn.Count - 1], Quaternion.identity, 0);
        }

        inmatch = true;
    }




    void SetMessages(GameObject temp)
    {
        messageWindow = temp.transform.GetChild(0).gameObject.transform.GetChild(0).GetComponent<Text>();
        timeWindow = temp.transform.GetChild(1).gameObject.transform.GetChild(0).GetComponent<Text>();
    }



    IEnumerator GenerateExitByCallingRpc()
    {
        yield return new WaitForSeconds(0.5f);
        GetComponent<PhotonView>().RPC("GenerateExitAndCoin", PhotonTargets.All, worldGen.GetComponent<MazeGenerator>().cellsGroundPositionSpawn[worldGen.GetComponent<MazeGenerator>().cellsGroundPositionSpawn.Count-1]);

    }

    void UpdateTime(float time)
    {
        GetComponent<PhotonView>().RPC("UpdateTime_RPC", PhotonTargets.All, time);

    }

    void AddMessage(string message)
    {
        GetComponent<PhotonView>().RPC("AddMessage_RPC", PhotonTargets.All, message);
    }


    [PunRPC]
    void AddMessage_RPC(string message)
    {
        messages.Enqueue(message);
        if (messages.Count > messageCount)
            messages.Dequeue();

        messageWindow.text = "";
        foreach (string m in messages)
            messageWindow.text += m + "\n";
    }


    private string FormatTime(float timeInSeconds)
    {
        return string.Format("{0}:{1:00}", Mathf.FloorToInt(timeInSeconds / 60), Mathf.FloorToInt(timeInSeconds % 60));
    }


    [PunRPC]
    void ChangePosition(int randomPositionInMatrix)
    {
        Vector3 spawnPointPosition = worldGen.GetComponent<MazeGenerator>().cellsGroundPositionSpawn[randomPositionInMatrix];
        exitGameObject.transform.position = spawnPointPosition;
    }

    [PunRPC]
    void ChangePositionCoins(int randomPositionInMatrix)
    {
        Vector3 spawnPointPosition = worldGen.GetComponent<MazeGenerator>().cellsGroundPositionSpawn[randomPositionInMatrix];
        coinsGameObject.transform.GetComponent<CoinFloat>().tempPos = spawnPointPosition;
        coinsGameObject.transform.position = spawnPointPosition;
    }

    [PunRPC]
    void UpdateTime_RPC(float time)
    {
        timeWindow.text = FormatTime(time);
    }

    [PunRPC]
    void GenerateExitAndCoin(Vector3 initialSpawnPoint)
    {
        exitGameObject = Instantiate(exit, initialSpawnPoint, exit.transform.rotation);
        coinsGameObject = Instantiate(coinS, initialSpawnPoint, exit.transform.rotation);
    }

    [PunRPC]
    void GenerateCoin(Vector3 initialSpawnPoint)
    {
        coinsGameObject = Instantiate(exit, initialSpawnPoint, exit.transform.rotation);
    }


    [PunRPC]
    void EndGame()
    {
        //add jsonobject over stats and send to node server

        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.player);
        PhotonNetwork.LeaveRoom();
        
    }

  
    void OnLeftRoom()
    {
        SocketIO.Emit("endroomgame", scoreToJsonObject(UserData.userName));
        SceneManager.LoadScene(3);
    }

    JSONObject scoreToJsonObject(string yourname)
    {
        JSONObject temp = new JSONObject();
        string[] names = scoreManager.GetPlayerNames("kills");
        foreach (string name in names)
        {
            if (name == yourname)
            {
                temp.AddField("MyName",name);
                temp.AddField("KILLS",scoreManager.GetScore(name, "kills").ToString());
                temp.AddField("DEATHS",scoreManager.GetScore(name, "deaths").ToString());
            }
        }
        return temp;
    }
}