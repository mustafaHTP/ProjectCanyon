using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DriftCameraController : MonoBehaviour
{
    public Transform car; // Reference to the car's transform
    public float cameraOffset = 2f; // Distance between car and camera
    public float damping = 5f; // Damping factor for camera movement

    private Vector3 lastCarPosition;

    private void Start()
    {
        lastCarPosition = car.position;
    }

    private void Update()
    {
        // Calculate the drift direction based on the change in car's position
        Vector3 driftDirection = car.position - lastCarPosition;

        // Calculate the target position for the camera
        Vector3 targetPosition = car.position - car.right * driftDirection.x * cameraOffset;

        // Apply damping to smooth camera movement
        Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * damping);

        // Update the camera position
        transform.position = newPosition;

        // Update last car position
        lastCarPosition = car.position;
    }
}
