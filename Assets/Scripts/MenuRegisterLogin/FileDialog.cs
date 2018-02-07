

using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SFB;
using SocketIO;
using System;

[RequireComponent(typeof(Button))]
public class FileDialog : MonoBehaviour, IPointerDownHandler
{
    public string Title = "";
    public string FileName = "";
    public string Directory = "";
    public string Extension = "";
    public bool Multiselect = false;

    public RawImage avatarProfile;
    
    public static SocketIOComponent SocketIO;

#if UNITY_WEBGL && !UNITY_EDITOR
    //
    // WebGL
    //
    [DllImport("__Internal")]
    private static extern void UploadFile(string id);

    public void OnPointerDown(PointerEventData eventData) {
        UploadFile(gameObject.name);
    }

    // Called from browser
    public void OnFileUploaded(string url) {
        StartCoroutine(OutputRoutine(url));
    }
#else
    //
    // Standalone platforms & editor
    //


    void Start()
    {
        SocketIO = GameObject.Find("SetupSocketConnectionToGame").GetComponent<SocketIOComponent>();

    }
    public void OnPointerDown(PointerEventData eventData) { }



    public void ChangeAvatarClicked()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(Title, Directory, Extension, Multiselect);
        if (paths.Length > 0)
        {
            StartCoroutine(OutputRoutine(new System.Uri(paths[0]).AbsoluteUri));
        }
    }
#endif

    private IEnumerator OutputRoutine(string url)
    {
        Debug.Log("URL: " + url);
        var loader = new WWW(url);
        yield return loader;
        avatarProfile.texture = loader.texture;
        GameObject.Find("CanvasMenu").transform.GetChild(2).gameObject.transform.GetChild(0).GetComponent<RawImage>().texture = loader.texture;
        //TO DO JSON PARSER PENTRU IMAGINI SEND RECEIVE SOCKET
        JSONObject j = new JSONObject(JSONObject.Type.OBJECT);
        byte[] myTextureBytes  = loader.texture.EncodeToPNG();
        String myTextureBytesEncodedAsBase64  = System.Convert.ToBase64String(myTextureBytes);

        j.AddField("photo", myTextureBytesEncodedAsBase64);
        j.AddField("username", UserData.userName);

        SocketIO.Emit("avatarImg", j);

    }
}