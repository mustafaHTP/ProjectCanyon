using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    [Header("Text Mesh Objects")]
    [SerializeField] private TextMeshProUGUI _speedText;
    [SerializeField] private TextMeshProUGUI _driftDirectionText;
    [SerializeField] private TextMeshProUGUI _sidewaysFrictionValuesText;

    [Header("Input Feedback")]
    [SerializeField] private Image _gasIndicatorImage;
    [SerializeField] private Image _brakeIndicatorImage;
    [SerializeField] private Image _handbrakeIndicatorImage;
    [SerializeField] private Image _steerLeftIndicatorImage;
    [SerializeField] private Image _steerRightIndicatorImage;

    private const float MinSteerIndicator = 0f;

    private WheelCollider _frontWheelCollider;
    private WheelCollider _backWheelCollider;
    private CarController _carController;
    private Rigidbody _carRigidbody;
    private IInput _input;

    private void Awake()
    {
        Player player = FindAnyObjectByType<Player>();
        if (player == null)
        {
            Debug.LogError("Player has not been found !");
        }
        else
        {
            if (!player.TryGetComponent(out _carController))
            {
                Debug.LogError("Player does not have Car Controller !");
            }
        }

        _frontWheelCollider = _carController.FrontLeftWheelCollider;
        _backWheelCollider = _carController.BackLeftWheelCollider;
        _carRigidbody = _carController.GetComponent<Rigidbody>();
        _input = _carController.GetComponent<IInput>();
    }

    private void Update()
    {
        DisplaySpeed();
        DisplayDriftDirection();
        DisplaySidewaysFrictionValues();
        DisplayInputFeedback();
    }

    private void DisplayInputFeedback()
    {
        _gasIndicatorImage.fillAmount = _input.Input.GasInput;
        _brakeIndicatorImage.fillAmount = _input.Input.BrakeInput;
        _handbrakeIndicatorImage.fillAmount = _input.Input.HandbrakeInput;

        float steerInput = _input.Input.SteerInput;
        if (steerInput > 0f)
        {
            _steerRightIndicatorImage.fillAmount = steerInput;
        }
        else
        {
            _steerRightIndicatorImage.fillAmount = MinSteerIndicator;
        }

        if (steerInput < 0f)
        {
            _steerLeftIndicatorImage.fillAmount = Mathf.Abs(steerInput);
        }
        else
        {
            _steerLeftIndicatorImage.fillAmount = MinSteerIndicator;
        }
    }

    private void DisplaySpeed()
    {
        float currentVelocity = _carRigidbody.velocity.magnitude;
        _speedText.text = $"Speed: {currentVelocity:N2}";
    }

    private void DisplayDriftDirection()
    {
        //Debug.DrawRay(carRb.transform.position, carRb.velocity * 50f, Color.green);
        //Debug.DrawRay(carRb.transform.position, carRb.transform.right * 50f, Color.yellow);

        float direction = Vector3.Dot(_carRigidbody.velocity.normalized, _carRigidbody.transform.right.normalized);
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
}
