using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody playerRB;
    public Transform playerCam;

    public float moveSpeed = 1;
    public float camRotateSpeed = 1;
    float camX = 0;
    public float maxCamAngle = 80f;

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
        //player movement
        playerRB.MovePosition(playerRB.position + Input.GetAxis("Vertical") * moveSpeed * playerRB.transform.forward * Time.deltaTime + Input.GetAxis("Horizontal") * moveSpeed * playerRB.transform.right * Time.deltaTime);

        //player rotation
        playerRB.MoveRotation(playerRB.rotation * Quaternion.Euler(0, Input.GetAxis("Mouse X") * camRotateSpeed * Time.deltaTime, 0));

        //cam rotation
        camX = Math.Clamp(camX - Input.GetAxis("Mouse Y") * camRotateSpeed * Time.deltaTime, -maxCamAngle, maxCamAngle);
        playerCam.transform.rotation = Quaternion.Euler(camX, playerCam.transform.rotation.eulerAngles.y, playerCam.transform.rotation.eulerAngles.z);
    }
}
