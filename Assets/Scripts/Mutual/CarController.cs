using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public const float MinSpeed = 0f;

    [Header("Experimental")]
    [SerializeField] private float _handbrakeTorque;

    [Header("Debug")]
    [Space(5)]
    [SerializeField] private bool _useManualWheelConfig;
    [SerializeField] private bool _useDownforce;
    [SerializeField] private float _downforce;
    [SerializeField] private bool _useSteerHelper;
    [SerializeField] private bool _useVfxCarSmoke;
    [SerializeField] private bool _useVfxCarSkidmark;

    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider _frontLeftWC;
    [SerializeField] private WheelCollider _frontRightWC;
    [SerializeField] private WheelCollider _rearLeftWC;
    [SerializeField] private WheelCollider _rearRightWC;

    [Header("Wheel Meshes")]
    [SerializeField] private Transform _frontLeftWheelMesh;
    [SerializeField] private Transform _frontRightWheelMesh;
    [SerializeField] private Transform _backLeftWheelMesh;
    [SerializeField] private Transform _backRightWheelMesh;

    [Header("General Car Settings")]
    [SerializeField] private AxleType _axleType;
    [SerializeField] private float _engineTorque = 500f;
    [SerializeField] private float _brakeTorque = 1500f;
    [SerializeField] private float _maxSteerAngle = 40f;
    [SerializeField] private float _steerSensitivity = 10f;
    [SerializeField] private float _handbrakeForce = 1000f;
    [SerializeField] private float _topSpeed = 50f;
    [Tooltip("If it is set to true, when the car is about to stop," +
        " car goes backwards.")]
    [SerializeField] private bool _isUsingBrakeToGoBack = false;

    /*
     * Increase or decrease front wheels stiffness 
     * according to car's speed
     * **/
    [Header("Steer Helper")]
    [Space(5)]
    [SerializeField] private AnimationCurve frontWheelsStiffnessCurve;

    [Header("Drift Settings")]
    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Threshold value to start drift. It is calculated by" +
        " dot product of car's right vector and car's velocity.")]
    private float _driftThreshold = 0.01f;
    [SerializeField] private float _minSpeedToDrift = 5f;
    [SerializeField] private float _frontStiffnessDrift = 1f;
    [SerializeField] private float _backStiffnessDrift = 1f;
    [Tooltip("Wheels' stiffness value when drifting")]
    [SerializeField] private float _wheelDriftStiffness = 1f;

    private const float MinMotorTorque = 0f;
    private const float MinBrakeTorque = 0f;

    private CarSmokeController _carSmokeController;
    private CarSoundController _carSoundController;
    private IInput _input;
    private Rigidbody _carRigidBody;
    private float _currentSpeed;

    #region CAR_EVENTS
    public event Action<bool> OnBrake;
    public event Action OnGrip;
    public event Action OnDrift;
    #endregion

    #region Handling

    private const float DefaultFrontStiffness = 2.5f;

    private float _currentFrontStiffness;

    /*
    * Initial values for sideways friction for all wheels
    * but only front and back wheels have different
    * initial stiffness
    * **/
    private float _initialExtremumSlip;
    private float _initialExtremumValue;
    private float _initialAsymptoteSlip;
    private float _initialAsymptoteValue;
    private float _initialRearStiffness;

    #endregion

    #region States
    //It is used to prevent play handbrake sfx multiple times when handbrake engaged
    private bool _hasReleasedHandbrake;
    private bool _isApplyingReverseTorque;
    private bool _isDrifting;

    #endregion

    private enum AxleType
    {
        FWD,
        RWD,
        AWD
    }

    #region Properties
    public bool IsDrifting => _isDrifting;
    public float TopSpeed => _topSpeed;
    public float CurrentSpeed => _currentSpeed;
    public WheelCollider FrontLeftWC => _frontLeftWC;
    public WheelCollider FrontRightWC => _frontRightWC;
    public WheelCollider RearLeftWC => _rearLeftWC;
    public WheelCollider RearRightWC => _rearRightWC;
    #endregion

    private void Awake()
    {
        GetSidewaysFrictionInitialValues();

        _carSmokeController = GetComponent<CarSmokeController>();
        _carSoundController = GetComponent<CarSoundController>();
        _carRigidBody = GetComponent<Rigidbody>();
        _input = GetComponent<IInput>();
    }

    private void FixedUpdate()
    {
        DecideCarMovementDirection();
        Gas();
        Brake();
        Handbrake();
        Steer();
        HelpSteer();

        SyncWheelMeshesWithColliders();


        _currentSpeed = _carRigidBody.velocity.magnitude;
        if (_useDownforce) _carRigidBody.AddForce(_downforce * -1f * transform.up);
    }

    private void HelpSteer()
    {
        if (!_useSteerHelper)
        {
            _currentFrontStiffness = DefaultFrontStiffness;

            return;
        }

        float currentSpeed = _carRigidBody.velocity.magnitude;
        float steerValue = currentSpeed / _topSpeed;
        _currentFrontStiffness = frontWheelsStiffnessCurve.Evaluate(steerValue);
    }

    private void Steer()
    {
        float targetSteerAngle = _maxSteerAngle * _input.FrameInput.SteerInput;

        FrontLeftWC.steerAngle =
            Mathf.Lerp(FrontLeftWC.steerAngle, targetSteerAngle, _steerSensitivity);
        FrontRightWC.steerAngle =
            Mathf.Lerp(FrontLeftWC.steerAngle, targetSteerAngle, _steerSensitivity);
    }

    private void Brake()
    {
        if (_isApplyingReverseTorque)
        {
            ApplyBrakeTorque(MinBrakeTorque);
            OnBrake?.Invoke(false);

            return;
        }

        float torqueToBeApplied = _brakeTorque * _input.FrameInput.BrakeInput;
        ApplyBrakeTorque(torqueToBeApplied);

        bool isBraking = torqueToBeApplied > 0f;
        OnBrake?.Invoke(isBraking);
    }

    private void Gas()
    {
        if (HasExceededTopSpeed())
        {
            ApplyMotorTorque(MinMotorTorque);
            return;
        }

        float torqueToBeApplied;

        // To move backwards
        if (_isApplyingReverseTorque)
        {
            torqueToBeApplied = -1f * _engineTorque * _input.FrameInput.BrakeInput;
        }
        else
        {
            torqueToBeApplied = _engineTorque * _input.FrameInput.GasInput;
        }

        ApplyMotorTorque(torqueToBeApplied);
    }

    private void Handbrake()
    {
        bool isUsingHandbrake = _input.FrameInput.HandbrakeInput != 0f;

        // Update handbrake state and play SFX if necessary
        if (isUsingHandbrake)
        {
            HandleHandbrakeEngaged();
        }
        else
        {
            HandleHandbrakeReleased();
        }
    }

    private void HandleHandbrakeReleased()
    {
        float driftAxis = Vector3.Dot(_carRigidBody.velocity.normalized, _carRigidBody.transform.right);

        if (_isDrifting)
        {
            //To not maintain drift
            if (Mathf.Abs(driftAxis) < _driftThreshold
                || _carRigidBody.velocity.magnitude <= _minSpeedToDrift
                || !AreWheelsGrounded())
            {
                _isDrifting = false;

                OnGrip?.Invoke();
            }
            else
            {
                OnDrift?.Invoke();
            }

        }
        else
        {
            //_skidMarkController.DisableEffect();
            _carSmokeController.TurnOffEffect();
            Grip();
        }

        _hasReleasedHandbrake = true;
    }

    private void HandleHandbrakeEngaged()
    {
        if (_hasReleasedHandbrake)
        {
            _carSoundController.PlayHandbrakeSFX();
            _hasReleasedHandbrake = false;
        }

        // Simulate braking
        _carRigidBody.AddForce(_carRigidBody.velocity * -1f * _handbrakeForce);

        if (_carRigidBody.velocity.magnitude > _minSpeedToDrift && AreWheelsGrounded())
        {
            _isDrifting = true;
            Drift();

            OnDrift?.Invoke();

            if (_useVfxCarSkidmark) //_skidMarkController.EnableEffect();
            if (_useVfxCarSmoke) _carSmokeController.TurnOnEffect();
        }
        else
        {
            _isDrifting = false;
            Grip();

            OnGrip?.Invoke();

            //_skidMarkController.DisableEffect();
            _carSmokeController.TurnOffEffect();
        }
    }

    /// <summary>
    /// It is primarily used to maintain drift
    /// If all wheels are not grounded, stop drift
    /// </summary>
    /// <returns></returns>
    private bool AreWheelsGrounded()
    {
        List<WheelCollider> _wheelColliders = GetWheelCollidersAsList();
        foreach (var _wheelCollider in _wheelColliders)
        {
            if (!_wheelCollider.isGrounded) return false;
        }

        return true;
    }

    /// <summary>
    /// Applies torque based on axle type, also if the wheel is not grounded,
    /// it does not apply torque
    /// </summary>
    /// <param name="motorTorqueAmount"></param>
    private void ApplyMotorTorque(float motorTorqueAmount)
    {
        switch (_axleType)
        {
            case AxleType.FWD:
                FrontLeftWC.motorTorque = FrontLeftWC.isGrounded switch
                {
                    true => motorTorqueAmount,
                    _ => MinMotorTorque
                };
                FrontRightWC.motorTorque = FrontRightWC.isGrounded switch
                {
                    true => motorTorqueAmount,
                    _ => MinMotorTorque
                };
                break;
            case AxleType.RWD:
                RearLeftWC.motorTorque = RearLeftWC.isGrounded switch
                {
                    true => motorTorqueAmount,
                    _ => MinMotorTorque
                };
                RearRightWC.motorTorque = RearRightWC.isGrounded switch
                {
                    true => motorTorqueAmount,
                    _ => MinMotorTorque
                };
                break;
            case AxleType.AWD:
                FrontLeftWC.motorTorque = FrontLeftWC.isGrounded switch
                {
                    true => motorTorqueAmount,
                    _ => MinMotorTorque
                };
                FrontRightWC.motorTorque = FrontRightWC.isGrounded switch
                {
                    true => motorTorqueAmount,
                    _ => MinMotorTorque
                };
                RearLeftWC.motorTorque = RearLeftWC.isGrounded switch
                {
                    true => motorTorqueAmount,
                    _ => MinMotorTorque
                };
                RearRightWC.motorTorque = RearRightWC.isGrounded switch
                {
                    true => motorTorqueAmount,
                    _ => MinMotorTorque
                };
                break;
        }
    }

    private void ApplyBrakeTorque(float brakeTorqueAmount)
    {
        FrontLeftWC.brakeTorque = brakeTorqueAmount;
        FrontRightWC.brakeTorque = brakeTorqueAmount;
        RearLeftWC.brakeTorque = brakeTorqueAmount;
        RearRightWC.brakeTorque = brakeTorqueAmount; 
    }

    private bool IsCarAboutToStop()
    {
        return _carRigidBody.velocity.magnitude < 0.1f;
    }

    /// <summary>
    /// Decide car movement forward or backward
    /// when car is about to stop, pressing brake goes backward
    /// </summary>
    private void DecideCarMovementDirection()
    {
        bool isBraking = _input.FrameInput.BrakeInput switch
        {
            0f => false,
            _ => true
        };

        bool isGassing = _input.FrameInput.GasInput switch
        {
            0f => false,
            _ => true
        };

        if (isGassing)
        {
            _isApplyingReverseTorque = false;
        }
        else if (IsCarAboutToStop() && isBraking)
        {
            _isApplyingReverseTorque = true;
        }
    }

    private bool HasExceededTopSpeed()
    {
        return _carRigidBody.velocity.magnitude > TopSpeed;
    }

    private void GetSidewaysFrictionInitialValues()
    {
        _initialExtremumSlip = FrontLeftWC.sidewaysFriction.extremumSlip;
        _initialExtremumValue = FrontLeftWC.sidewaysFriction.extremumValue;
        _initialAsymptoteSlip = FrontLeftWC.sidewaysFriction.asymptoteSlip;
        _initialAsymptoteValue = FrontLeftWC.sidewaysFriction.asymptoteValue;
        _initialRearStiffness = RearLeftWC.sidewaysFriction.stiffness;
    }

    private List<WheelCollider> GetWheelCollidersAsList()
    {
        List<WheelCollider> wheelColliders = new()
        {
            FrontLeftWC,
            FrontRightWC,
            RearLeftWC,
            RearRightWC
        };

        return wheelColliders;
    }

    private void Drift()
    {
        if (_useManualWheelConfig) return;

        List<WheelCollider> wheelColliders = GetWheelCollidersAsList();

        //Both front and rear wheels have same value for drift
        for (int i = 0; i < wheelColliders.Count; i++)
        {
            WheelFrictionCurve wheelFrictionCurve = wheelColliders[i].sidewaysFriction;
            wheelFrictionCurve.asymptoteSlip = _wheelDriftStiffness;
            wheelFrictionCurve.asymptoteValue = _wheelDriftStiffness;
            wheelFrictionCurve.extremumValue = _wheelDriftStiffness;
            wheelFrictionCurve.extremumSlip = _wheelDriftStiffness;
            wheelColliders[i].sidewaysFriction = wheelFrictionCurve;
        }

        //APPLY STIFFNESS
        //Front Wheels
        WheelFrictionCurve frictionCurve = FrontLeftWC.sidewaysFriction;
        frictionCurve.stiffness = _frontStiffnessDrift;
        FrontLeftWC.sidewaysFriction = frictionCurve;

        frictionCurve = FrontRightWC.sidewaysFriction;
        frictionCurve.stiffness = _frontStiffnessDrift;
        FrontRightWC.sidewaysFriction = frictionCurve;

        //Back Wheels
        frictionCurve = RearLeftWC.sidewaysFriction;
        frictionCurve.stiffness = _backStiffnessDrift;
        RearLeftWC.sidewaysFriction = frictionCurve;

        frictionCurve = RearRightWC.sidewaysFriction;
        frictionCurve.stiffness = _backStiffnessDrift;
        RearRightWC.sidewaysFriction = frictionCurve;
    }

    /// <summary>
    /// Both front and rear wheels have same initial values
    /// Except stiffness because rear wheels stiffness value causes
    /// for front wheels too much grip 
    /// </summary>
    private void Grip()
    {
        if (_useManualWheelConfig) return;

        List<WheelCollider> wheelColliders = GetWheelCollidersAsList();
        for (int i = 0; i < wheelColliders.Count; i++)
        {
            WheelFrictionCurve wheelFrictionCurve = wheelColliders[i].sidewaysFriction;
            wheelFrictionCurve.asymptoteSlip = _initialAsymptoteSlip;
            wheelFrictionCurve.asymptoteValue = _initialAsymptoteValue;
            wheelFrictionCurve.extremumValue = _initialExtremumValue;
            wheelFrictionCurve.extremumSlip = _initialExtremumSlip;
            wheelColliders[i].sidewaysFriction = wheelFrictionCurve;
        }

        //APPLY STIFFNESS
        //Front Wheels
        WheelFrictionCurve frictionCurve = FrontLeftWC.sidewaysFriction;
        //frictionCurve.stiffness = _initialFrontStiffness;
        frictionCurve.stiffness = _currentFrontStiffness;
        FrontLeftWC.sidewaysFriction = frictionCurve;

        frictionCurve = FrontRightWC.sidewaysFriction;
        //frictionCurve.stiffness = _initialFrontStiffness;
        frictionCurve.stiffness = _currentFrontStiffness;
        FrontRightWC.sidewaysFriction = frictionCurve;

        //Back Wheels
        frictionCurve = RearLeftWC.sidewaysFriction;
        frictionCurve.stiffness = _initialRearStiffness;
        RearLeftWC.sidewaysFriction = frictionCurve;

        frictionCurve = RearRightWC.sidewaysFriction;
        frictionCurve.stiffness = _initialRearStiffness;
        RearRightWC.sidewaysFriction = frictionCurve;
    }

    private void SyncWheelMeshesWithColliders()
    {
        SyncWheelMeshWithCollider(FrontLeftWC, _frontLeftWheelMesh);
        SyncWheelMeshWithCollider(FrontRightWC, _frontRightWheelMesh);
        SyncWheelMeshWithCollider(RearLeftWC, _backLeftWheelMesh);
        SyncWheelMeshWithCollider(RearRightWC, _backRightWheelMesh);
    }

    private void SyncWheelMeshWithCollider(WheelCollider wheelCollider, Transform wheelMesh)
    {
        wheelCollider.GetWorldPose(out Vector3 wheelColliderPosition, out Quaternion wheelColliderQuaternion);

        wheelMesh.SetPositionAndRotation(wheelColliderPosition, wheelColliderQuaternion);
    }
}