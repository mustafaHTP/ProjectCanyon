using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour, IPanel
{
    [Header("Panel GameObject")]
    [SerializeField] private GameObject _panel;

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

    #region IPanel Impl.

    public GameObject AttachedGameObject => _panel;

    public bool IsPanelActive { get; set; }

    public void TogglePanel()
    {
        IsPanelActive = !IsPanelActive;
        gameObject.SetActive(IsPanelActive);
    }

    public void DisablePanel() => gameObject.SetActive(false);

    public void EnablePanel() => gameObject.SetActive(true);

    #endregion

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

        _frontWheelCollider = _carController.FrontLeftWC;
        _backWheelCollider = _carController.RearLeftWC;
        _carRigidbody = _carController.GetComponent<Rigidbody>();
        _input = _carController.GetComponent<IInput>();
    }

    private void Update()
    {
        DisplaySpeed();
        DisplayDriftDirection();
        DisplaySidewaysFrictionValues();
        DisplayInputFeedback();
        DisplaySlipValues();
    }

    private void DisplayInputFeedback()
    {
        _gasIndicatorImage.fillAmount = _input.FrameInput.GasInput;
        _brakeIndicatorImage.fillAmount = _input.FrameInput.BrakeInput;
        _handbrakeIndicatorImage.fillAmount = _input.FrameInput.HandbrakeInput;

        float steerInput = _input.FrameInput.SteerInput;
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
        _sidewaysFrictionValuesText.text = $"FRONT WHEEL{Environment.NewLine}{Environment.NewLine}";
        _sidewaysFrictionValuesText.text += $"RPM: {_frontWheelCollider.rpm}{Environment.NewLine}";
        _sidewaysFrictionValuesText.text += $"ExtremumSlip: {_frontWheelCollider.sidewaysFriction.extremumSlip}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}ExtremumValue: {_frontWheelCollider.sidewaysFriction.extremumValue}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}AsymptoteSlip: {_frontWheelCollider.sidewaysFriction.asymptoteSlip}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}AsymptoteValue: {_frontWheelCollider.sidewaysFriction.asymptoteValue}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}Stiffness: {_frontWheelCollider.sidewaysFriction.stiffness}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}{Environment.NewLine}BACK WHEEL{Environment.NewLine}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}RPM: {_backWheelCollider.rpm}{Environment.NewLine}";
        _sidewaysFrictionValuesText.text += $"ExtremumSlip: {_backWheelCollider.sidewaysFriction.extremumSlip}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}ExtremumValue: {_backWheelCollider.sidewaysFriction.extremumValue}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}AsymptoteSlip: {_backWheelCollider.sidewaysFriction.asymptoteSlip}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}AsymptoteValue: {_backWheelCollider.sidewaysFriction.asymptoteValue}";
        _sidewaysFrictionValuesText.text += $"{Environment.NewLine}Stiffness: {_backWheelCollider.sidewaysFriction.stiffness}";
    }

    private void DisplaySlipValues()
    {
        WheelHit frontWheelHit;
        if (_carController.FrontLeftWC.GetGroundHit(out frontWheelHit))
        {
            print($"FL==>Fwd Slip:{Mathf.Abs(frontWheelHit.forwardSlip):N2}, Side Slip:{Mathf.Abs(frontWheelHit.sidewaysSlip):N2}");
        }

        WheelHit backWheelHit;
        if (_carController.RearLeftWC.GetGroundHit(out backWheelHit))
        {
            print($"BL==>Fwd Slip:{Mathf.Abs(backWheelHit.forwardSlip):N2} , Side Slip: {Mathf.Abs(backWheelHit.sidewaysSlip):N2}");
        }
    }
}
