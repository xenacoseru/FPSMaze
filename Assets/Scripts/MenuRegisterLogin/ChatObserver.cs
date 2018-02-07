using SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatObserver : MonoBehaviour
{

    public static Dictionary<string, GameObject> chatBoxes = new Dictionary<string, GameObject>();

    public static SocketIOComponent SocketIO;
    private JSONParser myJsonParser;
    public GameObject chatUI;
    private void Awake()
    {
        SocketIO = GameObject.Find("SetupSocketConnectionToGame").GetComponent<SocketIOComponent>();
        myJsonParser = new JSONParser();
        SocketIO.On("newMessageGlobalChat", OnMessageOnGlobalChat);
    }

    private void OnMessageOnGlobalChat(SocketIOEvent Obj)
    {
        string socket_id = myJsonParser.ElementFromJsonToString(Obj.data["socket_id"].ToString())[1];
        string listener = myJsonParser.ElementFromJsonToString(Obj.data["name"].ToString())[1];
        string message = myJsonParser.ElementFromJsonToString(Obj.data["message"].ToString())[1];
        if (!chatBoxes.ContainsKey(listener))
        {           
            GameObject chatBoxTemp = Instantiate(chatUI);
            chatBoxTemp.transform.parent = GameObject.Find("CanvasMenu").transform;
            chatBoxTemp.gameObject.transform.position = GameObject.Find("CanvasMenu").transform.position;
            chatBoxTemp.transform.Find("To").GetComponent<Text>().text = listener;
            chatBoxes.Add(listener, chatBoxTemp);
        }

        chatBoxes[listener].GetComponent<ChatBox>().inputField = message;

        chatBoxes[listener].GetComponent<ChatBox>().senderId = socket_id;
        if (chatBoxes[listener].GetComponent<ChatBox>().newMessage)
        {
            chatBoxes[listener].GetComponent<ChatBox>().AddChatEntry(UserData.userName, chatBoxes[listener].GetComponent<ChatBox>().inputField, true);
        }
        else
        {
            chatBoxes[listener].GetComponent<ChatBox>().AddChatEntry(listener, chatBoxes[listener].GetComponent<ChatBox>().inputField, false);
        }
        chatBoxes[listener].GetComponent<ChatBox>().newMessage = false;

    }

}