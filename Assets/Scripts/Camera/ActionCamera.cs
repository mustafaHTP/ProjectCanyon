using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class ActionCamera : MonoBehaviour, ICinemachineCameraLogic
{
    [SerializeField] private CarController _carController;

    [Header("Throttle Behavior Config")]
    [Space(2)]
    [SerializeField] private bool _applyThrottleBehavior;
    [SerializeField] private Vector3 _throttleDamping;
    [SerializeField] private float _throttleDampingSpeed;

    [Header("Drift Behavior Config")]
    [Space(2)]
    [SerializeField] private bool _applyDriftBehavior;
    [SerializeField] private Vector3 _driftFollowOffset;
    [SerializeField] private float _trackedOffsetChangeSpeed;
    [SerializeField] private float _steerXDamping = 1f;

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

    private const float MinDriftDirection = -1f;
    private const float MaxDriftDirection = 1f;

    private CinemachineVirtualCamera _actionCamera;
    private Rigidbody _carRigidbody;
    private NitroController _nitroController;
    private Vector3 _defaultFollowOffset;
    private float _defaultXDamping;
    private float _deltaFOV;

    private float deltaDamping = 0f;

    public void PerformCameraLogic()
    {
        ApplyDriftBehavior();
        ApplyNitroBehavior();
        ApplyNoiseBehavior();
        ApplyThrottleBehavior();
    }

    private void Awake()
    {
        _actionCamera = GetComponent<CinemachineVirtualCamera>();
        _carRigidbody = _carController.GetComponent<Rigidbody>();
        _nitroController = _carController.GetComponent<NitroController>();

        _defaultFollowOffset = _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
        _defaultXDamping = _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_XDamping;
    }

    private void ApplyDriftBehavior()
    {
        if(!_applyDriftBehavior) return;

        float driftDirection = Vector3.Dot(_carRigidbody.velocity.normalized, _carRigidbody.transform.right.normalized);
        float inverseDriftDirectionLerpFactor = Mathf.InverseLerp(MinDriftDirection, MaxDriftDirection, driftDirection);
        float lerpFactor = Time.deltaTime * _trackedOffsetChangeSpeed; //Delete AFTER TEST
        float currentTrackedOffsetX = _actionCamera.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.x;
        float targetTrackedOffsetX = -1f * driftDirection * _steerXDamping;
        float deltaOffsetX = Time.deltaTime * _trackedOffsetChangeSpeed;


        if (_carController.IsDrifting)
        {
            float newTrackedOffsetX = Mathf.Lerp(currentTrackedOffsetX, targetTrackedOffsetX, deltaOffsetX);
            _actionCamera.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.x = newTrackedOffsetX;

            //According to car's drift direction, move camera to that direction
            //_actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.x =
            //        Mathf.Lerp(
            //        _driftFollowOffset.x * MaxDriftDirection,
            //        _driftFollowOffset.x * MinDriftDirection,
            //        inverseDriftDirectionLerpFactor);

            //_actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z =
            //    Mathf.Lerp(
            //    _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z,
            //    _driftFollowOffset.z,
            //    lerpFactor);

            //_actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y =
            //    Mathf.Lerp(
            //    _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y,
            //    _driftFollowOffset.y,
            //    lerpFactor);
        }
        else
        {
            float newTrackedOffsetX = Mathf.Lerp(currentTrackedOffsetX, 0, deltaOffsetX);
            _actionCamera.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.x = newTrackedOffsetX;
            //_actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_XDamping = _defaultXDamping;

            //float lerpFactor = Mathf.Clamp01(_followOffsetChangeSpeed * Time.deltaTime);
            //_actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.x =
            //    Mathf.Lerp(
            //    _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.x,
            //    _defaultFollowOffset.x,
            //    lerpFactor);

            //_actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z =
            //    Mathf.Lerp(
            //    _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z,
            //    _defaultFollowOffset.z,
            //    lerpFactor);

            //_actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y =
            //    Mathf.Lerp(
            //    _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y,
            //    _defaultFollowOffset.y,
            //    lerpFactor);
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


        if (_carController.IsGassing)
        {
            deltaDamping += Time.deltaTime * _throttleDampingSpeed;
        }
        else
        {
            deltaDamping -= Time.deltaTime * _throttleDampingSpeed;
        }

        deltaDamping = Mathf.Clamp01(deltaDamping);

        _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_XDamping =
                Mathf.Lerp(0f, _throttleDamping.x, deltaDamping);
        _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_YDamping =
            Mathf.Lerp(0f, _throttleDamping.y, deltaDamping);
        _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_ZDamping =
            Mathf.Lerp(0f, _throttleDamping.z, deltaDamping);
    }
}
