using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public const float MinSpeed = 0f;

    [Header("Experimental")]
    [Space(5)]
    [SerializeField] private bool _isUsingManualWheelConfig = false;
    [SerializeField] private float _downforce;

    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider _frontLeftWheelCollider;
    [SerializeField] private WheelCollider _frontRightWheelCollider;
    [SerializeField] private WheelCollider _backLeftWheelCollider;
    [SerializeField] private WheelCollider _backRightWheelCollider;

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

    private CarSkidMarkController _skidMarkController;
    private CarSmokeController _carSmokeController;
    private CarLightController _lightController;
    private CarSoundController _carSoundController;
    private IInput _input;
    private Rigidbody _carRigidBody;
    private float _currentSpeed;

    #region Handling

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

    public bool IsDrifting => _isDrifting;
    public float TopSpeed => _topSpeed;
    public float CurrentSpeed => _currentSpeed;
    public WheelCollider FrontLeftWheelCollider => _frontLeftWheelCollider;
    public WheelCollider FrontRightWheelCollider => _frontRightWheelCollider;
    public WheelCollider BackLeftWheelCollider => _backLeftWheelCollider;
    public WheelCollider BackRightWheelCollider => _backRightWheelCollider;

    private void Awake()
    {
        GetSidewaysFrictionInitialValues();

        _lightController = GetComponent<CarLightController>();
        _skidMarkController = GetComponent<CarSkidMarkController>();
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

        _carRigidBody.AddForce(transform.up * _downforce * -1f);
        _currentSpeed = _carRigidBody.velocity.magnitude;
    }

    private void HelpSteer()
    {
        float currentSpeed = _carRigidBody.velocity.magnitude;
        float steerValue = currentSpeed / _topSpeed;
        _currentFrontStiffness = frontWheelsStiffnessCurve.Evaluate(steerValue);
    }

    private void Steer()
    {
        float targetSteerAngle = _maxSteerAngle * _input.Input.SteerInput;

        FrontLeftWheelCollider.steerAngle =
            Mathf.Lerp(FrontLeftWheelCollider.steerAngle, targetSteerAngle, _steerSensitivity);
        FrontRightWheelCollider.steerAngle =
            Mathf.Lerp(FrontLeftWheelCollider.steerAngle, targetSteerAngle, _steerSensitivity);
    }

    private void Brake()
    {
        if (_isApplyingReverseTorque)
        {
            ApplyBrakeTorque(MinBrakeTorque);
            _lightController.TurnOffBackLights();

            return;
        }

        bool isBraking = _input.Input.BrakeInput switch
        {
            0f => false,
            _ => true
        };

        if (isBraking)
        {
            _lightController.TurnOnBackLights();
        }
        else
        {
            _lightController.TurnOffBackLights();
        }

        float torqueToBeApplied = _brakeTorque * _input.Input.BrakeInput;
        ApplyBrakeTorque(torqueToBeApplied);
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
            torqueToBeApplied = -1f * _engineTorque * _input.Input.BrakeInput;
        }
        else
        {
            torqueToBeApplied = _engineTorque * _input.Input.GasInput;
        }

        ApplyMotorTorque(torqueToBeApplied);
    }

    private void Handbrake()
    {
        if (_isUsingManualWheelConfig) return;

        bool isUsingHandbrake = _input.Input.HandbrakeInput != 0f;

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
            if (Mathf.Abs(driftAxis) < _driftThreshold
                || _carRigidBody.velocity.magnitude <= _minSpeedToDrift
                || !AreWheelsGrounded())
            {
                _isDrifting = false;
            }
        }
        else
        {
            _skidMarkController.TurnOffEffect();
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

            _skidMarkController.TurnOnEffect();
            _carSmokeController.TurnOnEffect();
        }
        else
        {
            _isDrifting = false;
            Grip();

            _skidMarkController.TurnOffEffect();
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
                FrontLeftWheelCollider.motorTorque = FrontLeftWheelCollider.isGrounded switch
                {
                    true => motorTorqueAmount,
                    _ => MinMotorTorque
                };
                FrontRightWheelCollider.motorTorque = FrontRightWheelCollider.isGrounded switch
                {
                    true => motorTorqueAmount,
                    _ => MinMotorTorque
                };
                break;
            case AxleType.RWD:
                BackLeftWheelCollider.motorTorque = BackLeftWheelCollider.isGrounded switch
                {
                    true => motorTorqueAmount,
                    _ => MinMotorTorque
                };
                BackRightWheelCollider.motorTorque = BackRightWheelCollider.isGrounded switch
                {
                    true => motorTorqueAmount,
                    _ => MinMotorTorque
                };
                break;
            case AxleType.AWD:
                FrontLeftWheelCollider.motorTorque = FrontLeftWheelCollider.isGrounded switch
                {
                    true => motorTorqueAmount,
                    _ => MinMotorTorque
                };
                FrontRightWheelCollider.motorTorque = FrontRightWheelCollider.isGrounded switch
                {
                    true => motorTorqueAmount,
                    _ => MinMotorTorque
                };
                BackLeftWheelCollider.motorTorque = BackLeftWheelCollider.isGrounded switch
                {
                    true => motorTorqueAmount,
                    _ => MinMotorTorque
                };
                BackRightWheelCollider.motorTorque = BackRightWheelCollider.isGrounded switch
                {
                    true => motorTorqueAmount,
                    _ => MinMotorTorque
                };
                break;
        }
    }

    private void ApplyBrakeTorque(float brakeTorqueAmount)
    {
        BackLeftWheelCollider.brakeTorque = brakeTorqueAmount;
        BackRightWheelCollider.brakeTorque = brakeTorqueAmount;
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
        bool isBraking = _input.Input.BrakeInput switch
        {
            0f => false,
            _ => true
        };

        bool isGassing = _input.Input.GasInput switch
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
        _initialExtremumSlip = FrontLeftWheelCollider.sidewaysFriction.extremumSlip;
        _initialExtremumValue = FrontLeftWheelCollider.sidewaysFriction.extremumValue;
        _initialAsymptoteSlip = FrontLeftWheelCollider.sidewaysFriction.asymptoteSlip;
        _initialAsymptoteValue = FrontLeftWheelCollider.sidewaysFriction.asymptoteValue;
        _initialRearStiffness = BackLeftWheelCollider.sidewaysFriction.stiffness;
    }

    private List<WheelCollider> GetWheelCollidersAsList()
    {
        List<WheelCollider> wheelColliders = new()
        {
            FrontLeftWheelCollider,
            FrontRightWheelCollider,
            BackLeftWheelCollider,
            BackRightWheelCollider
        };

        return wheelColliders;
    }

    private void Drift()
    {
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
        WheelFrictionCurve frictionCurve = FrontLeftWheelCollider.sidewaysFriction;
        frictionCurve.stiffness = _frontStiffnessDrift;
        FrontLeftWheelCollider.sidewaysFriction = frictionCurve;

        frictionCurve = FrontRightWheelCollider.sidewaysFriction;
        frictionCurve.stiffness = _frontStiffnessDrift;
        FrontRightWheelCollider.sidewaysFriction = frictionCurve;

        //Back Wheels
        frictionCurve = BackLeftWheelCollider.sidewaysFriction;
        frictionCurve.stiffness = _backStiffnessDrift;
        BackLeftWheelCollider.sidewaysFriction = frictionCurve;

        frictionCurve = BackRightWheelCollider.sidewaysFriction;
        frictionCurve.stiffness = _backStiffnessDrift;
        BackRightWheelCollider.sidewaysFriction = frictionCurve;
    }

    /// <summary>
    /// Both front and rear wheels have same initial values
    /// Except stiffness because rear wheels stiffness value causes
    /// for front wheels too much grip 
    /// </summary>
    private void Grip()
    {
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
        WheelFrictionCurve frictionCurve = FrontLeftWheelCollider.sidewaysFriction;
        //frictionCurve.stiffness = _initialFrontStiffness;
        frictionCurve.stiffness = _currentFrontStiffness;
        FrontLeftWheelCollider.sidewaysFriction = frictionCurve;

        frictionCurve = FrontRightWheelCollider.sidewaysFriction;
        //frictionCurve.stiffness = _initialFrontStiffness;
        frictionCurve.stiffness = _currentFrontStiffness;
        FrontRightWheelCollider.sidewaysFriction = frictionCurve;

        //Back Wheels
        frictionCurve = BackLeftWheelCollider.sidewaysFriction;
        frictionCurve.stiffness = _initialRearStiffness;
        BackLeftWheelCollider.sidewaysFriction = frictionCurve;

        frictionCurve = BackRightWheelCollider.sidewaysFriction;
        frictionCurve.stiffness = _initialRearStiffness;
        BackRightWheelCollider.sidewaysFriction = frictionCurve;
    }

    private void SyncWheelMeshesWithColliders()
    {
        SyncWheelMeshWithCollider(FrontLeftWheelCollider, _frontLeftWheelMesh);
        SyncWheelMeshWithCollider(FrontRightWheelCollider, _frontRightWheelMesh);
        SyncWheelMeshWithCollider(BackLeftWheelCollider, _backLeftWheelMesh);
        SyncWheelMeshWithCollider(BackRightWheelCollider, _backRightWheelMesh);
    }

    private void SyncWheelMeshWithCollider(WheelCollider wheelCollider, Transform wheelMesh)
    {
        wheelCollider.GetWorldPose(out Vector3 wheelColliderPosition, out Quaternion wheelColliderQuaternion);

        wheelMesh.SetPositionAndRotation(wheelColliderPosition, wheelColliderQuaternion);
    }
}