using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class ActionCamera : MonoBehaviour, ICinemachineCameraLogic
{
    [SerializeField] private CarController _carController;

    [Header("Drift Behavior Config")]
    [Space(2)]
    [SerializeField] private Vector3 _driftFollowOffset;
    [SerializeField] private float _followOffsetChangeSpeed;

    [Header("Nitro Behavior Config")]
    [Space(2)]
    [SerializeField] private float _nitroFOV;
    [SerializeField] private float _defaultFOV;
    [SerializeField] private float _FOVChangeSpeed;

    [Header("Speed Behavior Config")]
    [Space(2)]
    [SerializeField] float _minFrequencyGain;
    [SerializeField] float _maxFrequencyGain;

    private const float MinDriftDirection = -1f;
    private const float MaxDriftDirection = 1f;

    private CinemachineVirtualCamera _actionCamera;
    private Rigidbody _carRigidbody;
    private NitroController _nitroController;
    private Vector3 _defaultFollowOffset;

    public void PerformCameraLogic()
    {
        ApplyDriftOffset();
        ApplyNitroFov();
        ApplyNoise();
    }

    private void Awake()
    {
        _actionCamera = GetComponent<CinemachineVirtualCamera>();
        _carRigidbody = _carController.GetComponent<Rigidbody>();
        _nitroController = _carController.GetComponent<NitroController>();

        _defaultFollowOffset = _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
    }

    private void ApplyDriftOffset()
    {
        if (_carController.IsDrifting)
        {
            float driftDirection = Vector3.Dot(_carRigidbody.velocity.normalized, _carRigidbody.transform.right.normalized);
            float lerpFactor = Mathf.Clamp01(Mathf.Abs(driftDirection) * _followOffsetChangeSpeed * Time.deltaTime);

            float inverseDriftDirectionLerpFactor = Mathf.InverseLerp(MinDriftDirection, MaxDriftDirection, driftDirection);

            //According to car's drift direction, move camera to that direction
            _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.x =
                    Mathf.Lerp(
                    _driftFollowOffset.x * MaxDriftDirection,
                    _driftFollowOffset.x * MinDriftDirection,
                    inverseDriftDirectionLerpFactor);

            _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z =
                Mathf.Lerp(
                _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z,
                _driftFollowOffset.z,
                lerpFactor);

            _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y =
                Mathf.Lerp(
                _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y,
                _driftFollowOffset.y,
                lerpFactor);
        }
        else
        {
            float lerpFactor = Mathf.Clamp01(_followOffsetChangeSpeed * Time.deltaTime);
            _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.x =
                Mathf.Lerp(
                _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.x,
                _defaultFollowOffset.x,
                lerpFactor);

            _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z =
                Mathf.Lerp(
                _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z,
                _defaultFollowOffset.z,
                lerpFactor);

            _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y =
                Mathf.Lerp(
                _actionCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y,
                _defaultFollowOffset.y,
                lerpFactor);
        }
    }

    private void ApplyNitroFov()
    {
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

    private void ApplyNoise()
    {
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
}
