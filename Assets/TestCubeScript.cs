using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCubeScript : MonoBehaviour
{
    [ObjectNetworkVariable] float cubeVariable1 = 10.5f;
    [ObjectNetworkVariable] string cubeVariable2 = "this is a cube";
}
