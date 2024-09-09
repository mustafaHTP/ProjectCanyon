using System;
using UnityEngine;

public class Speedometer : MonoBehaviour
{
    [Header("Speedometer Needle")]
    [SerializeField] private RectTransform _needleRT;

    [Header("Needle Angles")]
    [SerializeField] private float _minSpeedNeedleAngle;
    [SerializeField] private float _maxSpeedNeedleAngle;

    [Header("Rotation Amount Based On Speed")]
    [SerializeField] private float _maxRotationX;
    [SerializeField] private RectTransform _nitroBarRT;

    private float _initalRotationX;
    private float _topSpeed;
    private Rigidbody _carRigidBody;
    private Transform _playerCar;
    private CarController _carController;

    private void Awake()
    {
        FindPlayerCar();
    }

    private void Start()
    {
        _carController = _playerCar.GetComponent<CarController>();
        _topSpeed = _carController.TopSpeed;
        _carRigidBody = _playerCar.GetComponent<Rigidbody>();
        _initalRotationX = transform.localRotation.eulerAngles.x;
    }

    private void Update()
    {
        RotateSpeedometerAndNitroBar();
        RotateNeedle();
    }

    private void FindPlayerCar()
    {
        _playerCar = FindAnyObjectByType<Player>().transform;
        if (_playerCar == null)
        {
            Debug.LogError("Player car has not been found !");
        }
    }

    private void RotateSpeedometerAndNitroBar()
    {
        float mappedVelocity = Mathf.InverseLerp(0f, _topSpeed, _carRigidBody.velocity.magnitude);
        float targetRotation = Mathf.Lerp(_initalRotationX, _maxRotationX, mappedVelocity);
        transform.localRotation = Quaternion.Euler(targetRotation, 0f, 0f);
        _nitroBarRT.localRotation = Quaternion.Euler(targetRotation, 0f, 0f);
    }

    private void RotateNeedle()
    {
        float currentSpeed = _carController.CurrentSpeed;
        float topSpeed = _carController.TopSpeed;
        float normalizedSpeed = currentSpeed / topSpeed;
        float roundedSpeed = Mathf.Round(normalizedSpeed * Mathf.Pow(10, 3)) / Mathf.Pow(10, 3);

        _needleRT.localEulerAngles =
            new Vector3(0, 0, Mathf.Lerp(_minSpeedNeedleAngle, _maxSpeedNeedleAngle, roundedSpeed));
    }
}
