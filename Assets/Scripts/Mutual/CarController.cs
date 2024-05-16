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
    [SerializeField] private WheelCollider _frontLeftWheel;
    [SerializeField] private WheelCollider _frontRightWheel;
    [SerializeField] private WheelCollider _backLeftWheel;
    [SerializeField] private WheelCollider _backRightWheel;

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
    [SerializeField] private float _handbrakeForce = 1000f;
    [SerializeField] private float _topSpeed = 50f;
    [Tooltip("If it is set to true, when the car is about to stop," +
        " the brakes are applied and it reverses.")]
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

    private const float MinForwardSpeed = 0.1f;
    private const float MinMotorTorque = 0f;
    private SkidMarkController _skidMarkController;
    private CarSmokeController _carSmokeController;
    private LightController _lightController;
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

    private bool _isGassing;
    private bool _isBraking;
    private bool _isHandbrakeInUse;
    private bool _isMovingForward;
    private bool _isDrifting;

    #endregion

    private enum AxleType
    {
        FWD,
        RWD,
        AWD
    }

    public bool IsDrifting => _isDrifting;
    public bool IsUsingAutoReverseGear
    {
        get => _isUsingBrakeToGoBack;
        set => _isUsingBrakeToGoBack = value;
    }
    public float WheelRadius => _backLeftWheel.radius;
    public float TopSpeed => _topSpeed;
    public float CurrentSpeed => _currentSpeed;
    public bool IsGassing => _isGassing;

    private void Awake()
    {
        GetSidewaysFrictionInitialValues();

        _lightController = GetComponent<LightController>();
        _skidMarkController = GetComponent<SkidMarkController>();
        _carSmokeController = GetComponent<CarSmokeController>();
        _carRigidBody = GetComponent<Rigidbody>();
        _input = GetComponent<IInput>();
    }

    private void Update()
    {
        #region GasControl

        if (_input.Input.GasInput)
        {
            _isGassing = true;
            _isMovingForward = true;
        }
        else
        {
            _isGassing = false;
        }

        #endregion

        #region BrakeControl

        if (_input.Input.BrakeInput)
        {
            if (_isUsingBrakeToGoBack && !IsCarMovingForward()
                || _isMovingForward == false)
            {
                _isBraking = false;
                _isGassing = true;
                _isMovingForward = false;
            }
            else
            {
                _isBraking = true;
            }
        }
        else
        {
            _isBraking = false;
        }

        #endregion

        #region HandbrakeControl

        if (_input.Input.HandbrakeInput)
        {
            _isHandbrakeInUse = true;
        }
        else
        {
            _isHandbrakeInUse = false;
        }

        #endregion
    }

    private void FixedUpdate()
    {
        Gas(_isGassing, _isMovingForward);
        Brake(_isBraking);
        Handbrake(_isHandbrakeInUse);
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

    private bool IsCarMovingForward()
    {
        Vector3 currentVelocityInWorld = GetComponent<Rigidbody>().velocity;
        Vector3 currentVelocityInLocal = transform.InverseTransformDirection(currentVelocityInWorld);

        return currentVelocityInLocal.z > MinForwardSpeed;
    }

    private void Steer()
    {
        float targetSteerAngle = _maxSteerAngle * _input.Input.SteerInput;

        _frontLeftWheel.steerAngle = Mathf.Lerp(_frontLeftWheel.steerAngle, targetSteerAngle, 0.5f);
        _frontRightWheel.steerAngle = Mathf.Lerp(_frontLeftWheel.steerAngle, targetSteerAngle, 0.5f);
    }

    private void Brake(bool isBraking)
    {
        if (isBraking)
        {
            _lightController.TurnOnBackLights();

            _backLeftWheel.brakeTorque = _brakeTorque;
            _backRightWheel.brakeTorque = _brakeTorque;
        }
        else
        {
            _lightController.TurnOffBackLights();

            _backLeftWheel.brakeTorque = 0f;
            _backRightWheel.brakeTorque = 0f;
        }
    }

    private void Gas(bool isGassing, bool isMovingForward)
    {
        float currentTorque;
        if (isGassing)
        {
            bool isExceedingTopSpeed = _carRigidBody.velocity.magnitude > _topSpeed;
            if (isExceedingTopSpeed)
            {
                currentTorque = 0f;
            }
            else
            {
                currentTorque = _engineTorque;
            }

            if (!isMovingForward)
            {
                currentTorque *= -1;
            }
        }
        else
        {
            currentTorque = 0f;
        }

        ApplyTorqueToWheels(currentTorque);
    }

    private void Handbrake(bool isHandbrakeInUse)
    {
        if (_isUsingManualWheelConfig) return;

        /*
         * To start drift all wheels must be grounded
         * **/
        if (isHandbrakeInUse)
        {
            //simulate braking
            _carRigidBody.AddForce(_carRigidBody.velocity * -1f * _handbrakeForce);

            if (_carRigidBody.velocity.magnitude > _minSpeedToDrift
                && AreWheelsGrounded())
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
        else
        {
            //float driftAxis = Vector3.Dot(_carRigidBody.velocity, _carRigidBody.transform.right);
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
    /// <param name="torqueAmount"></param>
    private void ApplyTorqueToWheels(float torqueAmount)
    {
        switch (_axleType)
        {
            case AxleType.FWD:
                _frontLeftWheel.motorTorque = _frontLeftWheel.isGrounded switch
                {
                    true => torqueAmount,
                    _ => MinMotorTorque
                };
                _frontRightWheel.motorTorque = _frontRightWheel.isGrounded switch
                {
                    true => torqueAmount,
                    _ => MinMotorTorque
                };
                break;
            case AxleType.RWD:
                _backLeftWheel.motorTorque = _backLeftWheel.isGrounded switch
                {
                    true => torqueAmount,
                    _ => MinMotorTorque
                };
                _backRightWheel.motorTorque = _backRightWheel.isGrounded switch
                {
                    true => torqueAmount,
                    _ => MinMotorTorque
                };
                break;
            case AxleType.AWD:
                _frontLeftWheel.motorTorque = _frontLeftWheel.isGrounded switch
                {
                    true => torqueAmount,
                    _ => MinMotorTorque
                };
                _frontRightWheel.motorTorque = _frontRightWheel.isGrounded switch
                {
                    true => torqueAmount,
                    _ => MinMotorTorque
                };
                _backLeftWheel.motorTorque = _backLeftWheel.isGrounded switch
                {
                    true => torqueAmount,
                    _ => MinMotorTorque
                };
                _backRightWheel.motorTorque = _backRightWheel.isGrounded switch
                {
                    true => torqueAmount,
                    _ => MinMotorTorque
                };
                break;
        }
    }

    private void GetSidewaysFrictionInitialValues()
    {
        _initialExtremumSlip = _frontLeftWheel.sidewaysFriction.extremumSlip;
        _initialExtremumValue = _frontLeftWheel.sidewaysFriction.extremumValue;
        _initialAsymptoteSlip = _frontLeftWheel.sidewaysFriction.asymptoteSlip;
        _initialAsymptoteValue = _frontLeftWheel.sidewaysFriction.asymptoteValue;
        _initialRearStiffness = _backLeftWheel.sidewaysFriction.stiffness;
    }

    private List<WheelCollider> GetWheelCollidersAsList()
    {
        List<WheelCollider> wheelColliders = new()
        {
            _frontLeftWheel,
            _frontRightWheel,
            _backLeftWheel,
            _backRightWheel
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
        WheelFrictionCurve frictionCurve = _frontLeftWheel.sidewaysFriction;
        frictionCurve.stiffness = _frontStiffnessDrift;
        _frontLeftWheel.sidewaysFriction = frictionCurve;

        frictionCurve = _frontRightWheel.sidewaysFriction;
        frictionCurve.stiffness = _frontStiffnessDrift;
        _frontRightWheel.sidewaysFriction = frictionCurve;

        //Back Wheels
        frictionCurve = _backLeftWheel.sidewaysFriction;
        frictionCurve.stiffness = _backStiffnessDrift;
        _backLeftWheel.sidewaysFriction = frictionCurve;

        frictionCurve = _backRightWheel.sidewaysFriction;
        frictionCurve.stiffness = _backStiffnessDrift;
        _backRightWheel.sidewaysFriction = frictionCurve;
    }

    /*
     * Both front and rear wheels have same initial values
     * Except stiffness because rear wheels stiffness value causes
     * for front wheels too much grip 
     * **/
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
        WheelFrictionCurve frictionCurve = _frontLeftWheel.sidewaysFriction;
        //frictionCurve.stiffness = _initialFrontStiffness;
        frictionCurve.stiffness = _currentFrontStiffness;
        _frontLeftWheel.sidewaysFriction = frictionCurve;

        frictionCurve = _frontRightWheel.sidewaysFriction;
        //frictionCurve.stiffness = _initialFrontStiffness;
        frictionCurve.stiffness = _currentFrontStiffness;
        _frontRightWheel.sidewaysFriction = frictionCurve;

        //Back Wheels
        frictionCurve = _backLeftWheel.sidewaysFriction;
        frictionCurve.stiffness = _initialRearStiffness;
        _backLeftWheel.sidewaysFriction = frictionCurve;

        frictionCurve = _backRightWheel.sidewaysFriction;
        frictionCurve.stiffness = _initialRearStiffness;
        _backRightWheel.sidewaysFriction = frictionCurve;
    }

    private void SyncWheelMeshesWithColliders()
    {
        SyncWheelMeshWithCollider(_frontLeftWheel, _frontLeftWheelMesh);
        SyncWheelMeshWithCollider(_frontRightWheel, _frontRightWheelMesh);
        SyncWheelMeshWithCollider(_backLeftWheel, _backLeftWheelMesh);
        SyncWheelMeshWithCollider(_backRightWheel, _backRightWheelMesh);
    }

    private void SyncWheelMeshWithCollider(WheelCollider wheelCollider, Transform wheelMesh)
    {
        wheelCollider.GetWorldPose(out Vector3 wheelColliderPosition, out Quaternion wheelColliderQuaternion);

        wheelMesh.position = wheelColliderPosition;
        wheelMesh.rotation = wheelColliderQuaternion;
    }
}