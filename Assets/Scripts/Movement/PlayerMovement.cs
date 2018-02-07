using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : Photon.PunBehaviour
{

	/*
	 * 
	 *   This component is used just for my player my local..
	 *  
	 * 
	 */
	public float speed;
	public float jumpSpeed;

	Vector3 myDirection = Vector3.zero; // w a s d direction
	float verticalVelocity = 0f;

	CharacterController chContrl;
	Animator myAnimator;
	// Use this for initialization
	void Start () {
		chContrl = GetComponent<CharacterController>();
		myAnimator = GetComponent<Animator>();
	}
	
	//SETUP DIRECTION
	// Update is called once per frame
	void Update () {
		myDirection = transform.rotation * new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
		if (myDirection.magnitude > 1f)
		{
			myDirection = myDirection.normalized;
		}
		myAnimator.SetFloat("Speed", myDirection.magnitude);
		if(chContrl.isGrounded && Input.GetButton("Jump"))//bhops
		{
			verticalVelocity = jumpSpeed;
		}
	}


	//FixedUpdate is called once per physcs loop
	// Do all Movement and other physics here
	private void FixedUpdate()
	{
		Vector3 distance = myDirection * speed * Time.deltaTime;
		if (chContrl.isGrounded && verticalVelocity<0)//bhops
		{
			myAnimator.SetBool("Jumping", false);
			verticalVelocity = Physics.gravity.y * Time.deltaTime;
		}
		else
		{
			if (Mathf.Abs(verticalVelocity) > jumpSpeed * 0.75f)
			{
				myAnimator.SetBool("Jumping", true);
			}
			verticalVelocity += Physics.gravity.y * Time.deltaTime;
		}

		distance.y = verticalVelocity * Time.deltaTime;
		chContrl.Move(distance);
	}

   
}
