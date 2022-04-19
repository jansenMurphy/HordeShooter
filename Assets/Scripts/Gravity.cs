using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
/// <summary>
/// Simple gravity that moves an object global down.
/// </summary>
public class Gravity : MonoBehaviour
{
    private Rigidbody rb;
    public float localGravityModifier = 2.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void Update()
    {
        //TODO If the game changes to complex gravity, this'll need to get fixed to the new paradigm
        rb.AddForce(Vector3.down * localGravityModifier);
    }
}
