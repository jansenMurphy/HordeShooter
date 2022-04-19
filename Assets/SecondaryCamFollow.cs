using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryCamFollow : MonoBehaviour
{
    public Transform otherCamera;

    void Update()
    {
        transform.position = otherCamera.position;
        transform.rotation = otherCamera.rotation;
    }
}
