using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactHandler : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Vector3 impactForce = collision.impulse / Time.fixedDeltaTime;
        Debug.Log($"Impact Force: {impactForce.magnitude:N0}");
    }
}
