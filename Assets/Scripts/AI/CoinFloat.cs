using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinFloat : MonoBehaviour
{

    private float amplitude=1f;          //Set in Inspector 
    private float speed;                  //Set in Inspector 
    private  float tempVal;
    public Vector3 tempPos;

    void Start()
    {
        tempVal = transform.position.y;
    }

    void Update()
    {
        speed = Time.deltaTime;
        tempPos.y = tempVal + amplitude * Mathf.Sin(1f * Time.time);
        transform.position = tempPos;
    }
}
