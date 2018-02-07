using SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class Login : MonoBehaviour {


    public GameObject usernameField;
    public GameObject passwordField;
    public GameObject messageAlert;

    private string Username;
    private string Password;

    public static SocketIOComponent SocketIO;
    private JSONParser myJsonParser = new JSONParser();

    private void Awake()
    {
        SocketIO = GameObject.Find("SetupSocketConnectionToGame").GetComponent<SocketIOComponent>();
        SocketIO.On("wrongData", OnDataDoesntExist);
        SocketIO.On("loginSuccesfull", OnLoginSuccesfull);
        SocketIO.On("alreadyLoged", OnAlreadyLogged);
        SocketIO.On("askfriends", OnAskFriends);
    }

    private void OnAskFriends(SocketIOEvent obj)
    {
        JSONObject temp = new JSONObject();
        temp.AddField("username", UserData.userName);
        SocketIO.Emit("GetMyFriends", temp);
    }

    private void OnLoginSuccesfull(SocketIOEvent obj)
    {
        GameObject dialogMessage = Instantiate(messageAlert);
        dialogMessage.transform.parent = transform;
        dialogMessage.transform.position = passwordField.transform.position;
        dialogMessage.transform.Find("Message").gameObject.GetComponent<Text>().text = "Login succesfull.";

        UserData.userName = myJsonParser.ElementFromJsonToString(obj.data.GetField("username").ToString())[1];
        UserData.email = myJsonParser.ElementFromJsonToString(obj.data.GetField("email").ToString())[1];
        UserData.nomatches  = obj.data.GetField("nomatches").ToString();
        UserData.nomatchesWon = obj.data.GetField("nomatcheswon").ToString();
        UserData.photourl = myJsonParser.ElementFromJsonToString(obj.data.GetField("photourl").ToString())[1];
        
        

        Debug.Log("MOVE TO SCENE 3");
        //MOVE TO PROFILE FRIENDS CHAT SCENE NEW GAME FIND SERVERS
        SceneManager.LoadScene(3);

    }

    private void OnDataDoesntExist(SocketIOEvent obj)
    {
        GameObject dialogMessage = Instantiate(messageAlert);
        dialogMessage.transform.parent = transform;
        dialogMessage.transform.position = passwordField.transform.position;
        dialogMessage.transform.Find("Message").gameObject.GetComponent<Text>().text =  "Username or password wrong. Try again or new account.";
    }

    private void OnAlreadyLogged(SocketIOEvent Obj)
    {
        GameObject dialogMessage = Instantiate(messageAlert);
        dialogMessage.transform.parent = transform;
        dialogMessage.transform.position = passwordField.transform.position;
        dialogMessage.transform.Find("Message").gameObject.GetComponent<Text>().text = "Already Logged";
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (usernameField.GetComponent<InputField>().isFocused)
            {
                passwordField.GetComponent<InputField>().Select();
            }
            if (passwordField.GetComponent<InputField>().isFocused)
            {
                usernameField.GetComponent<InputField>().Select();
            }

        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            LoginUser();
        }

        Username = usernameField.GetComponent<InputField>().text;
        Password = passwordField.GetComponent<InputField>().text;

    }

    //TO DO SEND LOGIN DATA TO SERVER
    public void LoginUser()
    {
        if (checkEmpty())
            SendLoginData(Username, Password);
        else
        {

            GameObject dialogMessage = Instantiate(messageAlert);
            dialogMessage.transform.parent = transform;
            dialogMessage.transform.position = passwordField.transform.position;
            dialogMessage.transform.Find("Message").gameObject.GetComponent<Text>().text = "Username or password can not be empty";
        }
    }

    private bool checkEmpty()
    {
        return (Username != "" && Password != "");
    }
       
    public void SendLoginData(string username, string password)
    {
        SocketIO.Emit("login", new JSONObject(myJsonParser.LoginDataToJson(username, password)));
    }

  
}
