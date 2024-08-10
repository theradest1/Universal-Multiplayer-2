using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MethodsExample : MonoBehaviourUM2
{
    public LayerMask otherPlayerMask;
    public Transform cam;
    public float power;
    public Rigidbody rb;

    private void Update() {
        //if you press the left mouse button
        if(Input.GetMouseButtonDown(0)){

            //see if you are aiming at another player
            RaycastHit hit;
            if(Physics.Raycast(cam.position, cam.forward, out hit, Mathf.Infinity, otherPlayerMask)){
                Debug.Log("Hit player, making them jump :D");

                //get the client ID
                int hitPlayerID = hit.collider.gameObject.GetComponent<UM2_Prefab>().clientID;

                UM2_Methods.networkMethodDirect("jump", hitPlayerID, power);
            }
        }
    }

    public void jump(float power){
        print("Jumped");
        rb.AddForce(Vector3.up * power);
    }
}
