using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCharacter : Photon.PunBehaviour {
    Vector3 realPositon = Vector3.zero;
    Quaternion realRotation = Quaternion.identity;
    Animator myAnimator;
    private Vector3 hiddenPosition = new Vector3(-200f, -200f, -200f);


    public delegate void Respawn(object[] parameters);
    public event Respawn RespawnMe;
    public delegate void SendMessageOnNetwork(string MessageOverlay);
    public event SendMessageOnNetwork SendNetworkMessage;


    float moveFlow = 0.1f;
    // Use this for initialization
    void Start () {
        myAnimator = GetComponent<Animator>();
        if(myAnimator == null)
        {
            Debug.Log("YOu forgot to set the animator");
        }
    }
    
    // Update is called once per frame
    void Update () {
        if (photonView.isMine) // do nothing our characer inputs is moving us
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                GetComponent<PhotonView>().RPC("SpawnHelper", PhotonTargets.All, transform.position);
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, realPositon, 0.1f);
            transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 0.1f);
        }
    }

    void OnPhotonSerializeView(PhotonStream photonStream,PhotonMessageInfo info)
    {
        if (photonStream.isWriting) // sunt eu cel care se misca
        {
            //This is our player. We need to sned our actual positon to the network
            photonStream.SendNext(transform.position);
            photonStream.SendNext(transform.rotation);
            photonStream.SendNext(myAnimator.GetFloat("Speed"));
            photonStream.SendNext(myAnimator.GetBool("Jumping"));
            photonStream.SendNext(myAnimator.GetBool("Die"));
            photonStream.SendNext(myAnimator.GetBool("Respawn"));
            photonStream.SendNext(myAnimator.GetBool("Shoot"));
            

        }
        else //sau e altul
        {
            //This is someone else's player. Wee need to receive their positon (as of a few milisecond ago, and update our version of that player(Remote))
            realPositon = (Vector3)photonStream.ReceiveNext();
            realRotation = (Quaternion)photonStream.ReceiveNext();
            myAnimator.SetFloat("Speed", (float)photonStream.ReceiveNext());
            myAnimator.SetBool("Jumping", (bool)photonStream.ReceiveNext());
            myAnimator.SetBool("Die", (bool)photonStream.ReceiveNext());
            myAnimator.SetBool("Respawn", (bool)photonStream.ReceiveNext());
            myAnimator.SetBool("Shoot", (bool)photonStream.ReceiveNext());


        }
    }

   



}
