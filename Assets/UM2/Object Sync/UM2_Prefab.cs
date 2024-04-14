using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using System;

public class UM2_Prefab : MonoBehaviourUM2
{
    [Header("Debug:")]
    public int objectID;
    [HideInInspector] public int creatorID = -1;

    Vector3 pastPos;
    Quaternion pastRot;
    float pastTime = 0;
    Vector3 targetPos;
    Quaternion targetRot;
    float tickTime = -1;
    bool destroyWhenCreatorLeaves = false;


    public object getNetworkVariableValue(string name){
        return UM2_Variables.getNetworkVariable(name, objectID).getValue();
    }

    public void setNetworkVariableValue(string name, object value){
        UM2_Variables.getNetworkVariable(name, objectID).setValue(value);
    }

    public void addToNetworkVariableValue(string name, object valueToAdd){
        UM2_Variables.getNetworkVariable(name, objectID).addToValue(valueToAdd);
    }

    public NetworkVariable_Client getNetworkVariable(string name){
        return UM2_Variables.getNetworkVariable(name, objectID);
    }


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
        //if(!UM2_Client.client.tcpRecorded && !UM2_Client.client.udpRecorded){
        //    //if http is the only thing, it makes the tps the same as the http update rate
        //    newTPS = UM2_Client.client.httpUpdateTPS;
        //}
        tickTime = 1/newTPS;
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
