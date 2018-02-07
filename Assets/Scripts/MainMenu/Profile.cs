using SocketIO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Profile : MonoBehaviour {

    public GameObject userName;
    public GameObject email;
    public GameObject nomatches;
    public GameObject nomatchesWon;
    public RawImage output;
    public GameObject changePasswordPanel;
    private RectTransform rect;
    private void Start()
    {

        rect = GetComponent<RectTransform>();
        userName.GetComponent<Text>().text += " " + UserData.userName;
        email.GetComponent<Text>().text += " " + UserData.email;
        nomatches.GetComponent<Text>().text += " " + UserData.nomatches;
        nomatchesWon.GetComponent<Text>().text += " " + UserData.nomatchesWon;
        //SET AVATAR LIKE IN  FRINEDS MANAGER ACTIVITY WINDOW
        output.texture = GameObject.Find("CanvasMenu").transform.GetChild(2).gameObject.transform.GetChild(0).GetComponent<RawImage>().texture;
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

    public void ChangePassword()
    {
        GameObject changePanel = Instantiate(changePasswordPanel);
        changePanel.transform.parent = GameObject.Find("CanvasMenu").transform;
        changePanel.gameObject.transform.position = GameObject.Find("CanvasMenu").transform.position;

    }
}

