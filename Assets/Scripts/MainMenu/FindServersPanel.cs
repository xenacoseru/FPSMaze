using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FindServersPanel : MonoBehaviour {

    public static Dictionary<string, GameObject> serversList;
    private RectTransform rect;
    public GameObject serverPrefab;
    public GameObject content;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        serversList = new Dictionary<string, GameObject>();
        // Join Room
        if (NetworkManager.roomsList != null)
        {
            for (int i = 0; i < NetworkManager.roomsList.Length; i++)
            {
               
            //PhotonNetwork.JoinRoom(NetworkManager.roomsList[i].Name);
            GameObject newServer = Instantiate(serverPrefab);
            newServer.transform.Find("Text").GetComponent<Text>().text = NetworkManager.roomsList[i].Name+" "+ NetworkManager.roomsList[i].PlayerCount+" / " + NetworkManager.roomsList[i].MaxPlayers;
            newServer.transform.SetParent(content.transform, false);
            serversList.Add(NetworkManager.roomsList[i].Name, newServer);
            }
        }
    }

    public void OnDrag(UnityEngine.EventSystems.BaseEventData eventData)
    {
        var pointerData = eventData as UnityEngine.EventSystems.PointerEventData;
        if (pointerData == null) { return; }

        var currentPosition = rect.position;
        currentPosition.x += pointerData.delta.x;
        currentPosition.y += pointerData.delta.y;
        rect.position = currentPosition;
    }


}
