using SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketConnectionAndEvents : MonoBehaviour {

    SocketIOComponent socket;
    private static bool created = false;

    // Use this for initialization
    void Awake () {
        socket = GetComponent<SocketIOComponent>();
        if (created == false)
        {
            DontDestroyOnLoad(this.gameObject);
            socket.Connect();
            created = true;
           
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
	
	
}
