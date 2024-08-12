using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class UM2_Animator : MonoBehaviour
{
    Animator animator;
    List<string> pastSyncedParameterValues = new List<string>();

    float pastTicksPerSecond = -1;
    [Range(1,64)]
    public float ticksPerSecond = 20;
    UM2_Sync sync;
    UM2_Object objectScript;

    public bool syncAnimation = true;
    public bool optimizeAnimationSync = true;
    [Range(0, 16)]
    public float minTicksPerSecond = 0;
    float pastSyncTime = 0;
    bool pastSyncAnimation = false;

    private void Awake() {
        objectScript = GetComponent<UM2_Object>();

        animator = GetComponent<Animator>();

        //set past parameter list to current parameters
        foreach(AnimatorControllerParameter parameter in animator.parameters.ToList()){
            string parameterValue = getParameterValue(parameter);
            pastSyncedParameterValues.Add(parameterValue);
        }
    }

    private void Start()
    {
        sync = UM2_Sync.instance;
    }

    private void Update()
    {
        //this peice of code turns on the updateTransform loop
        //if syncTransform is set to false, the updateTransform loop stops
        //but if it is set back to true, this peice of code starts it back up again
        //it also turns it on at the start

        // if object is created on the network
        if(objectScript.initialized){

            //if the syncTransform toggle was changed
            if(pastSyncAnimation != syncAnimation){
                pastSyncAnimation = syncAnimation;

                //if it was set to true, turn the loop back on
                if(syncAnimation){
                    updateAnimationParameters();
                }
            }
        }
    }

    public async void updateAnimationParameters(bool forced = false){
        if(this != null && syncAnimation){
            
            /*//if the tps is changed, sync it to other clients
            if(pastTicksPerSecond != ticksPerSecond){
                pastTicksPerSecond = ticksPerSecond;
                sync.updateTPS(objectID, ticksPerSecond);
            }*/
            
            //if the object has moved
            bool parameterChanged = checkParameterValues();//(pastSyncedPos != transform.position) || (pastSyncedRot != transform.rotation);

            bool isMinUpdateRate = (minTicksPerSecond > 0) && (1/minTicksPerSecond <= Time.time - pastSyncTime);
            
            if(parameterChanged || !optimizeAnimationSync || forced || isMinUpdateRate){
                sync.updateObject(objectScript.objectID, string.Join("_", pastSyncedParameterValues));
                pastSyncTime = Time.time;
            }

            await Task.Delay((int)(1/ticksPerSecond*1000));
            updateAnimationParameters();
        }
    }

    //does two things:
    //checks if the values of the animation parameters are the same as the past ones - returns true or false for this
    //copies over the values to the past value list 
    bool checkParameterValues(){
        bool isTheSame = true;
        List<AnimatorControllerParameter> parameters = animator.parameters.ToList();
        for(int parameterIndex = 0; parameterIndex < parameters.Count; parameterIndex++){
            string parameterValue = getParameterValue(parameters[parameterIndex]);
            
            if(pastSyncedParameterValues[parameterIndex] != parameterValue){
                isTheSame = false;
            }

            pastSyncedParameterValues[parameterIndex] = parameterValue;
        }

        return !isTheSame;
    }

    string getParameterValue(AnimatorControllerParameter parameter){
        string parameterValue = null;

        switch (parameter.type)
        {
            case AnimatorControllerParameterType.Float:
                parameterValue = animator.GetFloat(parameter.name) + "";
                break;

            case AnimatorControllerParameterType.Int:
                parameterValue = animator.GetInteger(parameter.name) + "";
                break;

            case AnimatorControllerParameterType.Bool:
                parameterValue = animator.GetBool(parameter.name) + "";
                break;
        }

        return parameterValue;
    }
}
