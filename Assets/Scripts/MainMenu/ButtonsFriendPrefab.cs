using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonsFriendPrefab : MonoBehaviour {

    public GameObject chatBoxPrefab;

    public void StartChatButton()
    {
        GameObject btnTmp = EventSystem.current.currentSelectedGameObject;
        string friendName = btnTmp.transform.parent.gameObject.transform.Find("Text").GetComponent<Text>().text;

        if (!ChatObserver.chatBoxes.ContainsKey(friendName))
        {
            GameObject chatBoxTemp = Instantiate(chatBoxPrefab);
            chatBoxTemp.transform.parent = GameObject.Find("CanvasMenu").transform;
            chatBoxTemp.gameObject.transform.position = GameObject.Find("CanvasMenu").transform.position;
            chatBoxTemp.transform.Find("To").GetComponent<Text>().text = friendName;
            ChatObserver.chatBoxes.Add(friendName, chatBoxTemp);
        }
       
    }

    public void RemoveFriendButton()
    {
        GameObject btnTmp = EventSystem.current.currentSelectedGameObject;
        FriendsManager.RemoveFriend(btnTmp.transform.parent.Find("Text").GetComponent<Text>().text);
    }
}
