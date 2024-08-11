using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UM2_Animator : MonoBehaviour
{
    Animator animator;
    List<AnimatorControllerParameter> parameters;
    public float updateRate = 5;


    private void Awake() {
        animator = GetComponent<Animator>();
        parameters =  animator.parameters.ToList();
    }

    private void Start() {
        InvokeRepeating("syncAnimationParams", .5f, updateRate);
    }

    private void Update() {
        foreach(AnimatorControllerParameter parameter in parameters){
            string parameterName = parameter.name;
            string parameterValue = null;

            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    parameterValue = animator.GetFloat(parameterName) + "";
                    break;

                case AnimatorControllerParameterType.Int:
                    parameterValue = animator.GetInteger(parameterName) + "";
                    break;

                case AnimatorControllerParameterType.Bool:
                    parameterValue = animator.GetBool(parameterName) + "";
                    break;
            }

            print(parameter.name + ": " + parameterValue);
        }
    }
}
