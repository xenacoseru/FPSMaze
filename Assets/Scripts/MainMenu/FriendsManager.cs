using SocketIO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendsManager : MonoBehaviour {

    public GameObject friendOnlinePrefab;
    public GameObject friendOfflinePrefab;
    public GameObject contentParent;
    public RawImage avatar;
    public GameObject inputNameFriendSearch;
    public GameObject messageAlert;

    private static SocketIOComponent SocketIO;
    private static Dictionary<string, GameObject> friendList = new Dictionary<string, GameObject>();
    private JSONParser myJsonParser;
    private void Start()
    {
        SocketIO = GameObject.Find("SetupSocketConnectionToGame").GetComponent<SocketIOComponent>();
        myJsonParser = new JSONParser();
        GameObject.Find("CanvasMenu").transform.GetChild(2).gameObject.transform.GetChild(1).GetComponent<Text>().text = "Welcome, " + UserData.userName;
        if (UserData.photourl == "empty")
        {
            avatar.texture = Resources.Load<Texture2D>("unknown");
        }
        else
        {
            SocketIO.Emit("getPhoto", new JSONObject(myJsonParser.LoginUserAndUrlPhotoToJSON(UserData.userName, UserData.photourl)));
        }

        SocketIO.On("photobase64", OnPhotoReceive);
        SocketIO.On("playerNotOnline", OnPlayerNotOnline);
        SocketIO.On("newFriend", OnNewFriend);
        SocketIO.On("listFriends", OnReceiveListFriends);
        SocketIO.On("friendPhoto", OnReceivePhotoFriend);
        SocketIO.On("removeFriend", OnRemoveFriend);
        SocketIO.On("askfriends", OnAskFriends);

    }

    private void OnAskFriends(SocketIOEvent obj)
    {
        JSONObject temp = new JSONObject();
        temp.AddField("username", UserData.userName);
        SocketIO.Emit("GetMyFriends", temp);
    }

    private void OnReceivePhotoFriend(SocketIOEvent obj)
    {
        Debug.Log("FRIEND PHOTO RECEIVED");
        String base64string = myJsonParser.ElementFromJsonToString(obj.data.GetField("photoBase64").ToString())[1];
        String friendName = myJsonParser.ElementFromJsonToString(obj.data.GetField("friendName").ToString())[1];
        Texture2D convertedBase64String = new Texture2D(128, 128);
        byte[] decodedBytes = Convert.FromBase64String(base64string);
        convertedBase64String.LoadImage(decodedBytes);
        friendList[friendName].transform.Find("RawImage").GetComponent<RawImage>().texture = convertedBase64String;
    }

    private void OnNewFriend(SocketIOEvent obj)
    {
        GameObject dialogMessage = Instantiate(messageAlert);
        dialogMessage.transform.parent = transform;
        dialogMessage.transform.position = inputNameFriendSearch.transform.position;
        dialogMessage.transform.Find("Message").gameObject.GetComponent<Text>().text = "New friend";

        JSONObject temp = new JSONObject();
        temp.AddField("username", UserData.userName);
        SocketIO.Emit("GetMyFriends",temp);

    }

    private void OnPlayerNotOnline(SocketIOEvent obj)
    {
        GameObject dialogMessage = Instantiate(messageAlert);
        dialogMessage.transform.parent = transform;
        dialogMessage.transform.position = inputNameFriendSearch.transform.position;
        dialogMessage.transform.Find("Message").gameObject.GetComponent<Text>().text = "User not online";
    }

    public void OnPhotoReceive(SocketIOEvent eventObj)
    {
        String base64string = myJsonParser.ElementFromJsonToString(eventObj.data.GetField("photoBase64").ToString())[1];
        Texture2D convertedBase64String = new Texture2D(128, 128);
        byte[] decodedBytes = Convert.FromBase64String(base64string);
        convertedBase64String.LoadImage(decodedBytes);
        avatar.texture = convertedBase64String;
    }

    public void AddFriendButtonClicked()
    {
        string posibleFriend = inputNameFriendSearch.GetComponent<InputField>().text;
        if (String.IsNullOrEmpty(posibleFriend)==false)
        {
            SocketIO.Emit("addFriend", new JSONObject(myJsonParser.NewFriendPackage(UserData.userName, posibleFriend)));
        }
        else
        {
            GameObject dialogMessage = Instantiate(messageAlert);
            dialogMessage.transform.parent = transform;
            dialogMessage.transform.position = inputNameFriendSearch.transform.position;
            dialogMessage.transform.Find("Message").gameObject.GetComponent<Text>().text = "Set an valid user";
        }
    }

    public static void RemoveFriend(string friendname)
    {
        if (CheckDuplicates(friendname))
        {
            Destroy(friendList[friendname]);
            friendList.Remove(friendname);
            JSONObject tmp = new JSONObject();
            tmp.AddField("friendName", friendname);
            SocketIO.Emit("removeFriend", tmp);
        }
    }

    private void OnReceiveListFriends(SocketIOEvent obj)
    {
        DestroyAllFriendsGameObjects();
        friendList = new Dictionary<string, GameObject>();

        JSONObject friends = obj.data.GetField("friends");
        for (int i = 0; i < friends.Count; i++)
        {
            string username = myJsonParser.ElementFromJsonToString(friends[i].GetField("username").ToString())[1];
            string isOnline = friends[i].GetField("isOnline").ToString().Replace("\"", "");
            string url = myJsonParser.ElementFromJsonToString(friends[i].GetField("photourl").ToString())[1];
            
            if (isOnline == "0")
            {
                GameObject newFriend = Instantiate(friendOfflinePrefab);
                newFriend.transform.Find("Text").GetComponent<Text>().text = username;
                newFriend.transform.SetParent(contentParent.transform, false);
                friendList.Add(username, newFriend);
            }
            else if (isOnline == "1")
            {
                GameObject newFriend = Instantiate(friendOnlinePrefab);
                newFriend.transform.Find("Text").GetComponent<Text>().text = username;
                newFriend.transform.SetParent(contentParent.transform, false);
                friendList.Add(username, newFriend);
            }
            if (url != "empty")
            {
                SocketIO.Emit("getPhotoFriend", new JSONObject(myJsonParser.LoginUserAndUrlPhotoToJSON(username, url)));
            }
            else
            {
                friendList[username].transform.Find("Image").GetComponent<RawImage>().texture = (Texture2D)Resources.Load("unknown");
            }
        }
    }

    private void OnRemoveFriend(SocketIOEvent obj)
    {
        JSONParser myJsonParser = new JSONParser();
        var friendName = myJsonParser.ElementFromJsonToString(obj.data.GetField("name").ToString())[1];

        GameObject dialogMessage = Instantiate(messageAlert);
        dialogMessage.transform.parent = transform;
        dialogMessage.transform.position = inputNameFriendSearch.transform.position;
        dialogMessage.transform.Find("Message").gameObject.GetComponent<Text>().text = friendName+" removed you";
        JSONObject temp = new JSONObject();
        temp.AddField("username", UserData.userName);
        SocketIO.Emit("GetMyFriends", temp);
    }

    private void DestroyAllFriendsGameObjects()
    {
        foreach (string key in friendList.Keys)
        {
            Destroy(friendList[key]);
        }
    }

    private static bool CheckDuplicates(string name)
    {
        foreach (string key in friendList.Keys)
        {
            if (key == name)
                return true;
        }
        return false;
    }

}
