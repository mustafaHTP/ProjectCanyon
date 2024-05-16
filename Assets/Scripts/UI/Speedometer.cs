using TMPro;
using UnityEngine;

public class Speedometer : MonoBehaviour
{
    [Header("Speedometer Needle")]
    [SerializeField] private RectTransform needle;

    [Header("Needle Angles")]
    [SerializeField] private float minSpeedNeedleAngle;
    [SerializeField] private float maxSpeedNeedleAngle;

    [Header("Car")]
    [SerializeField] private GameObject car;

    [Header("Rotation Amount Based On Speed")]
    [SerializeField] private float maxRotationX;
    [SerializeField] private RectTransform nitroBar;

    private float _initalRotationX;
    private float _topSpeed;
    private Rigidbody _carRigidBody;
    private CarController _carController;

    private void Start()
    {
        _carController = car.GetComponent<CarController>();
        _topSpeed = _carController.TopSpeed;
        _carRigidBody = car.GetComponent<Rigidbody>();
        _initalRotationX = transform.localRotation.eulerAngles.x;
    }

    private void Update()
    {
        RotateSpeedometerAndNitroBar();
        RotateNeedle();
    }

    private void RotateSpeedometerAndNitroBar()
    {
        float mappedVelocity = Mathf.InverseLerp(0f, _topSpeed, _carRigidBody.velocity.magnitude);
        float targetRotation = Mathf.Lerp(_initalRotationX, maxRotationX, mappedVelocity);
        transform.localRotation = Quaternion.Euler(targetRotation, 0f, 0f);
        nitroBar.localRotation = Quaternion.Euler(targetRotation, 0f, 0f);
    }

    private void RotateNeedle()
    {
        float currentSpeed = _carController.CurrentSpeed;
        float topSpeed = _carController.TopSpeed;
        float normalizedSpeed = currentSpeed / topSpeed;
        float roundedSpeed = Mathf.Round(normalizedSpeed * Mathf.Pow(10, 3)) / Mathf.Pow(10, 3);

        needle.localEulerAngles =
            new Vector3(0, 0, Mathf.Lerp(minSpeedNeedleAngle, maxSpeedNeedleAngle, roundedSpeed));
    }
}
