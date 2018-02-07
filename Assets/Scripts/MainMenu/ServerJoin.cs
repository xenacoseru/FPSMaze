using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ServerJoin : MonoBehaviour {

    public void JoinButtonPressed()
    {
        GameObject btnTmp = EventSystem.current.currentSelectedGameObject;
        string serverName = btnTmp.transform.parent.gameObject.transform.Find("Text").GetComponent<Text>().text.ToString().Split(' ')[0];
        Debug.Log(serverName+"caca");
        NetworkManager.JoinRoom(serverName);
    }
    
}
