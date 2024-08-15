using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using UnityEngine;
using System;

public class UM2_Prefab : MonoBehaviourUM2
{
    [Header("Debug:")]
    [HideInInspector] public int objectID;

    //the ID of the creator of this object
    [HideInInspector] public int clientID = -1;

    Vector3 pastPos;
    Quaternion pastRot;
    float pastTime = 0;
    Vector3 targetPos;
    Quaternion targetRot;
    float tickTime = -1;

    Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    public T getNetworkVariableValue<T>(string name){
        NetworkVariable_Client networkVariable = getNetworkVariable(name);
        if(typeof(T) != networkVariable.type){
            Debug.LogWarning("Network variable \"" + name + "\" is " + networkVariable.type + ", but you requested " + typeof(T));
        }
        return (T)networkVariable.getValue();
    }

    public async void addVarCallback(string name, Action<object> method){
        if(objectID == -1){
            while (objectID == -1){
                await Task.Delay(50);
                if(!UM2_Client.connectedToServer){
                    return;
                }
            }
        }
        
        UM2_Variables.addVarCallback(name, method, objectID);
    }

    public void setNetworkVariableValue(string name, object value){
        getNetworkVariable(name).setValue(value);
    }

    public void addToNetworkVariableValue(string name, object valueToAdd){
        getNetworkVariable(name).addToValue(valueToAdd);
    }

    public NetworkVariable_Client getNetworkVariable(string name){
        if(objectID == -1){
            Debug.LogWarning("Cannot get network variable since object doesn't have a variable yet\njust wait for a bit before getting it (or ignore)");
            return null;
        }

        return UM2_Variables.getNetworkVariable(name, objectID);
    }

    public void newAnimationParameters(string parametersString){
        List<string> parametersStringList = parametersString.Split('_').ToList();
        List<AnimatorControllerParameter> parameters = animator.parameters.ToList();
        
        for(int parameterIndex = 0; parameterIndex < parameters.Count; parameterIndex++){
            switch (parameters[parameterIndex].type)
            {
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(parameters[parameterIndex].name, float.Parse(parametersStringList[parameterIndex]));
                    break;

                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(parameters[parameterIndex].name, int.Parse(parametersStringList[parameterIndex]));
                    break;

                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(parameters[parameterIndex].name, bool.Parse(parametersStringList[parameterIndex]));
                    break;
            }

        }
    }


    public override void OnPlayerLeave(int _clientID){
        if(_clientID == clientID){
            Destroy(this.gameObject);
        }
    }

    public void newTransform(Vector3 position, Quaternion rotation, bool ease){
        //dynamic TPS (not currently using)
        //tickTime = Time.time - pastTime;

        pastTime = Time.time;
        if(ease){
            pastPos = transform.position;
            pastRot = transform.rotation;
        }
        else{
            pastPos = position;
            pastRot = rotation;
        }
        targetPos = position;
        targetRot = rotation;
    }

    public void setTPS(float newTPS){
        //if(!UM2_Client.instance.tcpRecorded && !UM2_Client.instance.udpRecorded){
        //    //if http is the only thing, it makes the tps the same as the http update rate
        //    newTPS = UM2_Client.instance.httpUpdateTPS;
        //}
        tickTime = 1/newTPS;
    }

    public void initialize(int objectID, float TPS, Vector3 position, Quaternion rotation, int clientID){
        this.objectID = objectID;
        this.clientID = clientID;

        setTPS(TPS);

        pastPos = position;
        targetPos = position;
        transform.position = position;

        pastRot = rotation;
        targetRot = rotation;
        transform.rotation = rotation;
    }

    private void Update()
    {
        float percentDone = (Time.time - pastTime)/tickTime;

        transform.position = Vector3.Lerp(pastPos, targetPos, percentDone);
        transform.rotation = Quaternion.Lerp(pastRot, targetRot, percentDone);
    }
}
