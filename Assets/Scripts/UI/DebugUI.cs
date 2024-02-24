using System;
using TMPro;
using UnityEngine;

public class DebugUI : MonoBehaviour
{
    [Header("Car RigidBody")]
    [SerializeField] private Rigidbody carRb;

    [Header("Text Mesh Objects")]
    [SerializeField] private TextMeshProUGUI _speedText;
    [SerializeField] private TextMeshProUGUI _driftDirectionText;
    [SerializeField] private TextMeshProUGUI _sidewaysFrictionValuesText;
    [SerializeField] private TextMeshProUGUI _gearValuesText;

    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider _frontWheelCollider;
    [SerializeField] private WheelCollider _backWheelCollider;

    CarController _carController;

    private void Awake()
    {
        _carController = carRb.GetComponent<CarController>();
    }

    private void Update()
    {
        DisplaySpeed();
        DisplayDriftDirection();
        DisplaySidewaysFrictionValues();
        DisplayGearValues();
    }

    private void DisplaySpeed()
    {
        float currentVelocity = carRb.velocity.magnitude;
        _speedText.text = $"Speed: {currentVelocity:N2}";
    }

    private void DisplayDriftDirection()
    {
        //Debug.DrawRay(carRb.transform.position, carRb.velocity * 50f, Color.green);
        //Debug.DrawRay(carRb.transform.position, carRb.transform.right * 50f, Color.yellow);

        float direction = Vector3.Dot(carRb.velocity.normalized, carRb.transform.right.normalized);
        _driftDirectionText.text = $"Drift Dir: {direction:N2}";
    }

    private void DisplaySidewaysFrictionValues()
    {
        _sidewaysFrictionValuesText.text = $"Front Wheel Values{Environment.NewLine}";
        _sidewaysFrictionValuesText.text += $"ExtremumSlip: {_frontWheelCollider.sidewaysFriction.extremumSlip}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}ExtremumValue: {_frontWheelCollider.sidewaysFriction.extremumValue}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}AsymptoteSlip: {_frontWheelCollider.sidewaysFriction.asymptoteSlip}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}AsymptoteValue: {_frontWheelCollider.sidewaysFriction.asymptoteValue}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}Stiffness: {_frontWheelCollider.sidewaysFriction.stiffness}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}Back Wheel Values{Environment.NewLine}";
        _sidewaysFrictionValuesText.text += $"ExtremumSlip: {_backWheelCollider.sidewaysFriction.extremumSlip}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}ExtremumValue: {_backWheelCollider.sidewaysFriction.extremumValue}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}AsymptoteSlip: {_backWheelCollider.sidewaysFriction.asymptoteSlip}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}AsymptoteValue: {_backWheelCollider.sidewaysFriction.asymptoteValue}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}Stiffness: {_backWheelCollider.sidewaysFriction.stiffness}";
    }

    private void DisplayGearValues()
    {
        //int currentGear = _gearController.CurrentGear;
        //float currentRPM = _carController.CurrentEngineRPM;
        //float currentTorque = _carController.CurrentTorque;
        //bool isEngineBraking = _gearController.IsEngineBraking;

        //_gearValuesText.text = $"Gear: {currentGear}";
        //_gearValuesText.text += $"{Environment.NewLine}RPM: {currentRPM:N0}";
        //_gearValuesText.text += $"{Environment.NewLine}Torque: {currentTorque:N0}";
        //_gearValuesText.text += $"{Environment.NewLine}Engine Brake: {isEngineBraking}";
    }
}
