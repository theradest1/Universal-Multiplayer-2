using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using System;

public class UM2_Prefab : MonoBehaviourUM2
{
    List<SyncedObjectVariable> syncedObjectVariables = new List<SyncedObjectVariable>();
    [SerializeField] List<string> syncedObjectVariableNames = new List<string>();


    [HideInInspector] public int objectID;
    [HideInInspector] public int creatorID = -1;

    Vector3 pastPos;
    Quaternion pastRot;
    float pastTime = 0;
    Vector3 targetPos;
    Quaternion targetRot;
    float tickTime = -1;
    bool destroyWhenCreatorLeaves = false;



    //this is when this client wants to make a new variable on this object
    public void createNewVariable<T>(string variableName, T initialValue){
        int variableID = syncedObjectVariables.Count; // hopefully this won't backfire (:
        SyncedObjectVariable newVariable = new SyncedObjectVariable(initialValue.GetType(), initialValue, objectID, variableID, variableName);
        
        syncedObjectVariableNames.Add(variableName);
        syncedObjectVariables.Add(newVariable);

        //newVariable.sendValue();
        //explenation in UM2_Object
    }

    //this is when others make a new variable for this object
    public void syncNewVariable(string variableName, Type type, object initialValue, int variableID){
        SyncedObjectVariable newVariable = new SyncedObjectVariable(type, initialValue, objectID, variableID, variableName);
        
        syncedObjectVariableNames.Add(variableName);
        syncedObjectVariables.Add(newVariable);
    }

    public object getVariableValue(string variableName){
        return getVariableValue(syncedObjectVariableNames.IndexOf(variableName));
    }

    public object getVariableValue(int variableID){
        try{
            return syncedObjectVariables[variableID].value;
        }
        catch{
            return null;
        }
    }

    public void setVariableValue(string variableName, object value){
        setVariableValue(syncedObjectVariableNames.IndexOf(variableName), value);
    }

    public void setVariableValue(int variableID, object value){
        syncedObjectVariables[variableID].setValue(value, false);
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
