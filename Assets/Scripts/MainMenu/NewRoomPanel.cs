using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewRoomPanel : MonoBehaviour {

    public GameObject roomName;
    public GameObject maxPlayers;
    public GameObject sizeMaze;
    public GameObject msgPrompt;

    private string nameroom;
    private int playersNO;
    private int sizeLab;
    private RectTransform rect;
    private void Start()
    {
        rect = GetComponent<RectTransform>();
    }
    public void StartRoom()
    {
        nameroom = roomName.GetComponent<InputField>().text.ToString();

        playersNO = System.Int32.Parse(maxPlayers.GetComponent<InputField>().text.ToString());
        sizeLab = System.Int32.Parse(sizeMaze.GetComponent<InputField>().text.ToString());
        if (sizeLab % 2 == 1)
        {
            sizeLab++;
        }
        if (CheckValidData(nameroom, playersNO, sizeLab))
        {
            NetworkManager.CreateRoom(nameroom, playersNO, sizeLab);
            Destroy(gameObject);
        }
        else
        {
            GameObject dialogMessage = Instantiate(msgPrompt);
            dialogMessage.transform.parent = transform;
            dialogMessage.transform.position = maxPlayers.transform.position;
            dialogMessage.transform.Find("Message").gameObject.GetComponent<Text>().text = "Invalid data";
        }
    }

    public bool CheckValidData(string nameroom,int playersNO, int sizeLab) {
        return !string.IsNullOrEmpty(nameroom) && playersNO > 1 && sizeLab > 2;
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
