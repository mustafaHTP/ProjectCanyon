using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class ActionCamera : MonoBehaviour, ICinemachineCameraLogic
{
    [Header("Throttle Behavior Config")]
    [Space(2)]
    [SerializeField] private bool _applyThrottleBehavior;
    [SerializeField] private Vector3 _throttleDamping;
    [SerializeField] private float _throttleDampingSpeed;

    [Header("Drift Behavior Config")]
    [Space(2)]
    [SerializeField] private bool _applyDriftBehavior;
    [SerializeField] private float _trackedObjectOffsetChangeSpeed;
    [SerializeField] private float _trackedObjectOffsetXDamping = 1f;

    [Header("Nitro Behavior Config")]
    [Space(2)]
    [SerializeField] private bool _applyNitroBehavior;
    [SerializeField] private float _nitroFOV;
    [SerializeField] private float _defaultFOV;
    [SerializeField] private float _FOVChangeSpeed;

    [Header("Noise Behavior Config")]
    [Space(2)]
    [SerializeField] private bool _applyNoiseBehavior;
    [SerializeField] float _minFrequencyGain;
    [SerializeField] float _maxFrequencyGain;

    private CinemachineVirtualCamera _actionCamera;
    private Rigidbody _carRigidbody;
    private CarNitroController _nitroController;
    private CarController _carController;
    private IInput _input;
    private float _deltaDamping = 0f;

    public void PerformCameraLogic()
    {
        ApplyDriftBehavior();
        ApplyNitroBehavior();
        ApplyNoiseBehavior();
        ApplyThrottleBehavior();
    }

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

        _actionCamera = GetComponent<CinemachineVirtualCamera>();
        _carRigidbody = _carController.GetComponent<Rigidbody>();
        _nitroController = _carController.GetComponent<CarNitroController>();
        _input = _carController.GetComponent<IInput>();
    }

    private void ApplyDriftBehavior()
    {
        if (!_applyDriftBehavior) return;

        float driftDirection = Vector3.Dot(_carRigidbody.velocity.normalized, _carRigidbody.transform.right.normalized);
        float currentTrackedOffsetX = _actionCamera.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.x;
        float targetTrackedOffsetX = -1f * driftDirection * _trackedObjectOffsetXDamping;
        float deltaOffsetX = Time.deltaTime * _trackedObjectOffsetChangeSpeed;

        if (_carController.IsDrifting)
        {
            float newTrackedOffsetX = Mathf.Lerp(currentTrackedOffsetX, targetTrackedOffsetX, deltaOffsetX);
            _actionCamera.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.x = newTrackedOffsetX;
        }
        else
        {
            float newTrackedOffsetX = Mathf.Lerp(currentTrackedOffsetX, 0, deltaOffsetX);
            _actionCamera.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.x = newTrackedOffsetX;
        }
    }

    private void ApplyNitroBehavior()
    {
        if (!_applyNitroBehavior) return;

        if (_nitroController.IsUsingNitro)
        {
            _actionCamera.m_Lens.FieldOfView = Mathf.Lerp(
                _actionCamera.m_Lens.FieldOfView,
                _nitroFOV,
                _FOVChangeSpeed * Time.fixedDeltaTime);
        }
        else
        {
            _actionCamera.m_Lens.FieldOfView = Mathf.Lerp(
                _actionCamera.m_Lens.FieldOfView,
                _defaultFOV,
                _FOVChangeSpeed * Time.fixedDeltaTime);
        }
    }

    private void ApplyNoiseBehavior()
    {
        if (!_applyNoiseBehavior) return;

        float mappedVelocity = Mathf.InverseLerp(
            0f,
            _carController.TopSpeed,
            _carRigidbody.velocity.magnitude);

        float currentFrequencyGain = Mathf.Lerp(
            _minFrequencyGain,
            _maxFrequencyGain,
            mappedVelocity);

        _actionCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = currentFrequencyGain;
    }

    private void ApplyThrottleBehavior()
    {
        if (!_applyThrottleBehavior) return;

        if (_input.Input.GasInput > 0f)
        {
            _deltaDamping += Time.deltaTime * _throttleDampingSpeed;
        }
        else
        {
            _deltaDamping -= Time.deltaTime * _throttleDampingSpeed;
        }

        _deltaDamping = Mathf.Clamp01(_deltaDamping);

        _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_XDamping =
                Mathf.Lerp(0f, _throttleDamping.x, _deltaDamping);
        _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_YDamping =
            Mathf.Lerp(0f, _throttleDamping.y, _deltaDamping);
        _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_ZDamping =
            Mathf.Lerp(0f, _throttleDamping.z, _deltaDamping);
    }
}
