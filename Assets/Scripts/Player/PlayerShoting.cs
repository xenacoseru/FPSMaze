using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShoting : MonoBehaviour {

    private float fireRate = 0.5f;
    private float cooldown = 0f;
    private AudioSource playerAudio;                                   // Reference to the AudioSource component.
    public AudioClip shotClip;
    private Animator myAnimator;
    private int startBullets=20;
    private int currentBullets;
    private int currentCoins;

    public Text bulletsStatus;
    public Text coinsStatus;
    void Start()
    {
        currentBullets = startBullets;
        currentCoins = 0;
        playerAudio = GetComponent<AudioSource>();
        myAnimator = GetComponent<Animator>();

    }

    void Update()
    {

        bulletsStatus.text = currentBullets + "/" + startBullets;
        coinsStatus.text = currentCoins+"@";
        cooldown -= Time.deltaTime;
        if (Input.GetButton("Fire1"))
        {
            Fire();
        }
    }

    private void Fire()
    {
        if (cooldown > 0 || currentBullets==0)
        {
            return;
        }

        Debug.Log("Fire our gun");
        currentBullets -= 1;

        GetComponent<PhotonView>().RPC("SpawnMuzzleFlash", PhotonTargets.All);
        myAnimator.SetBool("Shoot", true);
        StartCoroutine("TurnOffShoot", 0.1f);

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hitInfo;
        hitInfo = FindClosetHitInfo(ray);
        if (hitInfo.collider != null)
        {
            Debug.Log("we hit " + hitInfo.transform.name);
            Health h = hitInfo.transform.parent.GetComponent<Health>();
            if (h != null)
            {
                playerAudio.clip = shotClip;
                playerAudio.Play();

                h.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, 10, UserData.userName);
            }
        }

        cooldown = fireRate;
    }

    public int returnNoCoins()
    {
        return currentCoins;
    }

    [PunRPC]
    public void GiveBullets(int amount)
    {
        currentBullets = amount;
    }

    [PunRPC]
    public void GiveCoins(int amount)
    {
        currentCoins += amount;
    }

    [PunRPC]
    public void LoseCoins(int amount)
    {
        currentCoins -= amount;
    }

    private RaycastHit FindClosetHitInfo(Ray ray)
        {
            RaycastHit[] raycasts = Physics.RaycastAll(ray);
            float distance = 0f;
            RaycastHit closet = new RaycastHit();

            foreach (RaycastHit hit in raycasts)
            {
                Debug.Log(hit.collider.gameObject.name+" "+hit.distance);
                if (closet.collider == null||(hit.distance < distance)){
                    closet = hit;
                    distance = hit.distance;
                
                }
            }

            return closet;
    
     }

    private IEnumerator TurnOffShoot(float time)
    {
         yield return new WaitForSeconds(time);
         myAnimator.SetBool("Shoot",false);

    }
}
