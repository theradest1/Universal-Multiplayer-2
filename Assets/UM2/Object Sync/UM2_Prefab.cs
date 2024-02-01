using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Timeline;

public class UM2_Prefab : MonoBehaviourUM2
{
    public int objectID;

    Vector3 pastPos;
    Quaternion pastRot;
    float pastTime = 0;
    Vector3 targetPos;
    Quaternion targetRot;
    float tickTime = -1;
    public bool destroyWhenCreatorLeaves = false;
    int creatorID = -1;

    public override void OnPlayerLeave(int clientID){
        if(clientID == creatorID && destroyWhenCreatorLeaves){
            Destroy(this.gameObject);
        }
    }

    public void newTransform(Vector3 position, Quaternion rotation){
        //dynamic TPS (not currently using)
        //tickTime = Time.time - pastTime;
        pastTime = Time.time;

        pastPos = transform.position;
        pastRot = transform.rotation;

        targetPos = position;//new Vector3(-position.x, position.y, position.z);
        targetRot = rotation;
    }

    public void setTPS(float newTPS){
        if(!UM2_Client.client.tcpRecorded && !UM2_Client.client.udpRecorded){
            //if http is the only thing, it makes the tps the same as the http update rate
            setTPS(UM2_Client.client.httpUpdateTPS);
        }
        else{
            tickTime = 1/newTPS;
        }
    }

    public void initialize(int setObjectID, float TPS, Vector3 _position, Quaternion _rotation, int _creatorID, bool _destroyWhenCreatorLeaves){
        objectID = setObjectID;
        creatorID = _creatorID;
        destroyWhenCreatorLeaves = _destroyWhenCreatorLeaves;

        setTPS(TPS);

        pastPos = _position;
        targetPos = _position;
        transform.position = _position;

        pastRot = _rotation;
        targetRot = _rotation;
        transform.rotation = _rotation;
    }

    private void Update()
    {
        float percentDone = (Time.time - pastTime)/tickTime;

        transform.position = Vector3.Lerp(pastPos, targetPos, percentDone);
        transform.rotation = Quaternion.Lerp(pastRot, targetRot, percentDone);
    }
}
