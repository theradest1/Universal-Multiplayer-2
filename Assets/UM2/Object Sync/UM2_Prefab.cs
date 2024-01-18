using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Timeline;

public class UM2_Prefab : MonoBehaviour
{
    public int objectID;
    //public float ticksPerSecond;

    Vector3 pastPos = Vector3.zero;
    Quaternion pastRot = Quaternion.identity;
    float pastTime = 0;
    Vector3 targetPos = Vector3.zero;
    Quaternion targetRot = Quaternion.identity;
    float tickTime = -1;

    public void newTransform(Vector3 position, Quaternion rotation){
        //tickTime = Time.time - pastTime;
        //pastTime = Time.time;

        pastPos = transform.position;
        pastRot = transform.rotation;

        targetPos = position;//new Vector3(-position.x, position.y, position.z);
        targetRot = rotation;
    }

    public void setTPS(float newTPS){
        tickTime = 1/newTPS;
    }

    private void Update()
    {
        float percentDone = (Time.time - pastTime)/tickTime;

        transform.position = Vector3.Lerp(pastPos, targetPos, percentDone);
        transform.rotation = Quaternion.Lerp(pastRot, targetRot, percentDone);
    }
}
