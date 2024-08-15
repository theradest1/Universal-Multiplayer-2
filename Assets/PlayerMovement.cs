using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Vector2 movementInput;

    //movement settings
	public float acceleration = 11f;
	public float inAirAcceleration = 1f;
	public float decceleration = .95f;
	public float inAirDecceleration = .03f;
	public float jumpSpeed = 3.5f;

    //small settings
    public float groundCheckHeight = 1f;
    public float groundCheckDistance = .1f;
    public LayerMask groundMask;


    //cam settings
    public float camRotateSpeed = 1;
    float camX = 0;
    public float maxCamAngle = 80f;


    //references
    public Rigidbody playerRB;
    public Transform playerCam;
    public Animator animator;


    //vars
    bool isGrounded = false;
    bool jumping = false;



    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        //player rotation
        playerRB.MoveRotation(playerRB.rotation * Quaternion.Euler(0, Input.GetAxis("Mouse X") * camRotateSpeed, 0));

        //cam rotation
        camX = Math.Clamp(camX - Input.GetAxis("Mouse Y") * camRotateSpeed, -maxCamAngle, maxCamAngle);
        playerCam.transform.rotation = Quaternion.Euler(camX, playerCam.transform.rotation.eulerAngles.y, playerCam.transform.rotation.eulerAngles.z);

        //ignore how choppy the animation looks, it's just for an animation syncing example
        animator.SetFloat("x", movementInput.x);
        animator.SetFloat("y", movementInput.y);
    }

    void FixedUpdate()
    {
        RaycastHit hit;
		isGrounded = Physics.Raycast(transform.position + Vector3.up * groundCheckHeight, -Vector3.up, out hit, groundCheckDistance, groundMask);
        isGrounded = isGrounded && playerRB.velocity.y < 1f;
        jumping = Input.GetKey("space");
        Debug.DrawRay(transform.position + Vector3.up * groundCheckHeight, -Vector3.up * groundCheckDistance, Color.red);

        if (isGrounded)
        {
            playerRB.AddForce((transform.right * movementInput.x + transform.forward * movementInput.y) * acceleration);
        }
        else
        {
            playerRB.AddForce((transform.right * movementInput.x + transform.forward * movementInput.y) * inAirAcceleration);
        }

        //jumping
		if(isGrounded && jumping)
		{
			//playerRB.AddForce(Vector3.up * jumpPower);// += new Vector3(0f, jumpPower, 0f);
			playerRB.velocity = new Vector3(playerRB.velocity.x, jumpSpeed, playerRB.velocity.z);
            isGrounded = false;
        }

		//friction (not vertical)
		Vector3 velocity = playerRB.velocity;
		Vector3 yVelocity = Vector3.up * velocity.y;
		velocity.y = 0f;
		if(isGrounded)
		{
            playerRB.velocity = velocity * decceleration + yVelocity;
		}
		else
		{
			playerRB.velocity = velocity * inAirDecceleration + yVelocity;
		}
    }
}
