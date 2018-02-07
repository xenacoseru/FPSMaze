using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeToRegisterLoginOrBack : MonoBehaviour
{

    public void ChangeToRegisterScene()
    {
        SceneManager.LoadScene(1);
    }

    public void ChangeToLoginScene()
    {
        SceneManager.LoadScene(2);
    }

    public void ChanageToMenuScene()
    {
        SceneManager.LoadScene(0);
    }

    public void ExitAppByClicking()
    {
        Application.Quit();
    }

  
}

    
