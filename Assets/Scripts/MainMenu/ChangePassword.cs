using SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangePassword : MonoBehaviour {
    
    public GameObject currentPassword;
    public GameObject newPassword;
    public GameObject rnewPassword;
    public GameObject messageAlert;

    private string currentPsd, newPsd, rnewPsd;
    private static SocketIOComponent SocketIO;
    private JSONParser myJsonParaser;

    private void Start()
    {
        SocketIO = GameObject.Find("SetupSocketConnectionToGame").GetComponent<SocketIOComponent>();
        myJsonParaser = new JSONParser();
        SocketIO.On("currentPasswordWrong", OnPasswordWrong);
        SocketIO.On("passwordChanged", OnPasswordChanged);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (currentPassword.GetComponent<InputField>().isFocused)
            {
                newPassword.GetComponent<InputField>().Select();
            }
            if (newPassword.GetComponent<InputField>().isFocused)
            {
                rnewPassword.GetComponent<InputField>().Select();
            }
            if (rnewPassword.GetComponent<InputField>().isFocused)
            {
                currentPassword.GetComponent<InputField>().Select();
            }

        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ChangePasswordFct();
        }

        currentPsd = currentPassword.GetComponent<InputField>().text;
        newPsd = newPassword.GetComponent<InputField>().text;
        rnewPsd = rnewPassword.GetComponent<InputField>().text;
    }

    public void ChangePasswordFct()
    {

        if (newPsd != rnewPsd || (string.IsNullOrEmpty(newPsd) && string.IsNullOrEmpty(rnewPsd)))
        {
            GameObject dialogMessage = Instantiate(messageAlert);
            dialogMessage.transform.parent = transform;
            dialogMessage.transform.position = newPassword.transform.position;
            dialogMessage.transform.Find("Message").gameObject.GetComponent<Text>().text = "Password doesen't match";
        }
        else
        {
            SocketIO.Emit("newPassword", new JSONObject(myJsonParaser.ChangePassword(UserData.userName, currentPsd, newPsd)));
        }
    }

    private void OnPasswordWrong(SocketIOEvent obj)
    {
        GameObject dialogMessage = Instantiate(messageAlert);
        dialogMessage.transform.parent = transform;
        dialogMessage.transform.position = newPassword.transform.position;
        dialogMessage.transform.Find("Message").gameObject.GetComponent<Text>().text = "Current Password wrong";
    }


    private void OnPasswordChanged(SocketIOEvent obj)
    {
        GameObject dialogMessage = Instantiate(messageAlert);
        dialogMessage.transform.parent = transform;
        dialogMessage.transform.position = newPassword.transform.position;
        dialogMessage.transform.Find("Message").gameObject.GetComponent<Text>().text = "Password Changed";
    }
}
