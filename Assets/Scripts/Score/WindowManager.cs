using UnityEngine;
using System.Collections;

public class WindowManager : MonoBehaviour {

    public GameObject scoreBoard;
    public static bool checkNow = false;
    void Update () {
        if(Input.GetKeyDown(KeyCode.Tab) && checkNow) {
            scoreBoard.SetActive( !scoreBoard.activeSelf );
        }
    }
}
