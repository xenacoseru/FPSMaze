using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class Patrol : Photon.MonoBehaviour
{

    public Transform[] points;
    private int destPoint = 0;
    private NavMeshAgent agent;
    private GameObject worldGen;
    private Animator anim;
    private float minDistance = 0.1f;
    private float lastAttackTime=0f;
    private float aiAttackRate = 3f;

    private bool waiting = false;
    void Awake()
    {
        worldGen = GameObject.Find("WorldGen").gameObject;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        points = new Transform[4];
        CreateReferencePoitns();
    }

    void Start()
    {
        if (PhotonNetwork.isMasterClient)
        {
            GetComponent<PhotonView>().RPC("GoToNextPoint", PhotonTargets.All, 0);
            GetComponent<PhotonView>().RPC("ChangeAnimToPatrol", PhotonTargets.All);
        }
    }

    void CreateReferencePoitns()
    {
        int width = worldGen.GetComponent<MazeGenerator>().Width;
        int height = worldGen.GetComponent<MazeGenerator>().Height;
        GameObject leftUpperCorner = new GameObject();
        leftUpperCorner.name = "leftUpperCorner";
        leftUpperCorner.transform.position = worldGen.GetComponent<MazeGenerator>().cellsGroundPositionSpawn[0];
        GameObject rightUpperCorner = new GameObject();
        rightUpperCorner.name = "rightUpperCorner";
        rightUpperCorner.transform.position =
            worldGen.GetComponent<MazeGenerator>().cellsGroundPositionSpawn[width - 1];
        GameObject leftDownCorner = new GameObject();
        leftDownCorner.name = "leftDownCorner";
        leftDownCorner.transform.position =
            worldGen.GetComponent<MazeGenerator>().cellsGroundPositionSpawn[width * height - width];
        GameObject rightDownCorner = new GameObject();
        rightDownCorner.name = "rightDownCorner";
        rightDownCorner.transform.position =
            worldGen.GetComponent<MazeGenerator>().cellsGroundPositionSpawn[width * height - 1];

        int k = 0;
        points[k++] = leftUpperCorner.transform;
        points[k++] = rightUpperCorner.transform;
        points[k++] = leftDownCorner.transform;
        points[k++] = rightDownCorner.transform;

    }

    private bool isReadyToAttackMinion()
    {
        return Time.time - lastAttackTime > aiAttackRate;
    }

    void Update()
    {
        if (PhotonNetwork.isMasterClient)
            DetectEnemy();
        
        if (agent.remainingDistance < 0.5f && PhotonNetwork.isMasterClient)
        {
            destPoint = (destPoint + 1) % points.Length;
            GetComponent<PhotonView>().RPC("GoToNextPoint", PhotonTargets.All, destPoint);
        }
    }





    Quaternion startingAngle = Quaternion.Euler(0,-180,0);
    Quaternion stepAngle = Quaternion.AngleAxis(10, Vector3.up);
    private RaycastHit hit;
    private GameObject posibleTarget;

    void DetectEnemy()
    {

        var angle = transform.rotation * startingAngle;
        var direction = angle * Vector3.forward;
        var pos = transform.position;
        for (var i = 0; i < 36; i++)
        {
            if (Physics.Raycast(pos, direction, out hit, 1.5f))
            {
                if(hit.collider.gameObject.CompareTag("Player"))
                {
                    posibleTarget = hit.collider.gameObject;
                    break;
                }
            }
            direction = stepAngle * direction;
        }

        for (var i = 0; i < 36; i++)
        {
            Debug.DrawRay(pos, direction, Color.red);
            direction = stepAngle * direction;

        }
        if (waiting==false && posibleTarget)
        {

            Debug.Log(isReadyToAttackMinion());
            if (isReadyToAttackMinion())
            {
                waiting = true;
                GetComponent<PhotonView>().RPC("Attack",PhotonTargets.All);
                lastAttackTime = Time.time;
            }
        }

    }

    private float GetDistanceBetweenPositions(Vector3 start, Vector3 end)
    {
        return Mathf.Sqrt(Mathf.Pow((end.x - start.x), 2) + Mathf.Pow(end.y - start.y, 2));
    }

    [PunRPC]
    public void GoToNextPoint(int destPoint)
    {
        agent.SetDestination(points[destPoint].position);
    }

    [PunRPC]
    public void ChangeAnimToPatrol()
    {
        anim.SetBool("Patrol", true);
    }

   
    [PunRPC]
    public void Attack()
    {

        agent.isStopped = true;
        Health h;
        PlayerShoting ps;
        if (posibleTarget)
        {
            h = posibleTarget.transform.parent.GetComponent<Health>();
            ps = posibleTarget.transform.parent.GetComponent<PlayerShoting>();
            if (h != null)
            {
                if (ps.returnNoCoins() < 3)
                {
                    h.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.All, 50, gameObject.name);
                }
                else
                {
                    ps.GetComponent<PhotonView>().RPC("LoseCoins", PhotonTargets.All, 3);
                }
            }
            posibleTarget = null;
            waiting = false;
        }
        agent.isStopped = false;
    }
}


