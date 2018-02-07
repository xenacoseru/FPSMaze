using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatRoom : MonoBehaviour {

    class ChatEntry
    {
        public string name;
        public string message;
        public bool isMine;
        public string timeTag;
    }
    static ArrayList entries;
    static Vector2 currentScrollPos = new Vector2();
    private Rect chatRect = new Rect(Screen.width * 0.01f, Screen.height * 0.45f, Screen.width * 0.3f, Screen.height * 0.3f);

    string inputField = "";
    bool chatInFocus = false;
    string inputFieldFocus = "CIFT";
    private static bool newMessage;
    private string senderId;

    JSONParser myJsonParser = new JSONParser();
    void Awake()
    {
        InitializeChat();
        Debug.Log(true);
    }

  
    void InitializeChat()
    {
        entries = new ArrayList();
        UnfocusChat();
    }
    private void OnGUI()
    {
        if (PhotonNetwork.inRoom && PhotonNetwork.room.PlayerCount == PhotonNetwork.room.MaxPlayers)
            Draw();
    }
    //draw the chat box in size relative to your GUIlayout
    public void Draw()
    {
        ChatWindow();
    }

    void ChatWindow()
    {
        GUILayout.BeginArea(chatRect);
        GUILayout.BeginVertical("box");
        currentScrollPos = GUILayout.BeginScrollView(currentScrollPos);
        GUILayout.FlexibleSpace();
        foreach (ChatEntry ent in entries)
        {
            GUILayout.BeginHorizontal();
            GUI.skin.label.wordWrap = true;
            if (ent.isMine)
                GUI.contentColor = Color.white;
            else
                GUI.contentColor = Color.green;

            GUILayout.Label(ent.timeTag + " " + ent.name + ": " + ent.message);
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
        }
        GUILayout.EndScrollView();
        if (chatInFocus)
        {
            GUI.SetNextControlName(inputFieldFocus);
            inputField = GUILayout.TextField(inputField, GUILayout.MaxWidth(Screen.width * 0.3f), GUILayout.MinWidth(Screen.height * 0.3f));
            GUI.FocusControl(inputFieldFocus);
        }
        GUILayout.EndVertical();

        GUILayout.EndArea();

        if (chatInFocus)
        {
            HandleNewEntries();
        }
        else
        {
            checkForInput();
        }

    }

    public void UnfocusChat()
    {
        inputField = "";
        senderId = "";
        chatInFocus = false;
    }

    public void checkForInput()
    {
        if (Event.current.type == EventType.KeyDown && Event.current.character == '\n' && !chatInFocus)
        {
            GUI.FocusControl(inputFieldFocus);
            chatInFocus = true;
            currentScrollPos.y = float.PositiveInfinity;
            inputField = "";
        }
    }

    void HandleNewEntries()
    {
        if (Event.current.type == EventType.KeyDown && Event.current.character == '\n')
        {
            if (inputField.Length <= 0)
            {
                UnfocusChat();
                Debug.Log("unfocusing chat (empty entry)");
                return;
            }
            else
            {
                newMessage = true;
                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("name", UserData.userName);
                data.Add("message", inputField);
                SendMessageChatRoom(data);
                UnfocusChat();
            }
        }
    }


    public static void AddChatEntry(Dictionary<string,string> data)
    {
        Debug.Log("data from rpc " + data);
        ChatEntry newEntry = new ChatEntry();
        newEntry.name = data["name"];
        newEntry.message = data["message"];
        if (newMessage == true)
        {
            newEntry.isMine = true;
        }
        else
        {
            newEntry.isMine = false;
        }
        newMessage = false;
        newEntry.timeTag = "[" + System.DateTime.Now.Hour.ToString() + ":" + System.DateTime.Now.Minute.ToString() + "]";
        Debug.Log(newEntry.name + " " + newEntry.message + " " + newEntry.isMine + " " + newEntry.timeTag);
        entries.Add(newEntry);
        Debug.Log(entries.Count);
        currentScrollPos.y = float.PositiveInfinity;
    }


    public void SendMessageChatRoom(Dictionary<string, string> message)
    {
        GetComponent<PhotonView>().RPC("SendMessageChatInRoom", PhotonTargets.All, message);
    }

    [PunRPC]
    void SendMessageChatInRoom(Dictionary<string, string> data)
    {

        AddChatEntry(data);
    }
}
