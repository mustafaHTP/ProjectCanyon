using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIController : MonoBehaviour, IInput
{
    [Header("General Config")]
    [Space(5)]
    [SerializeField] private bool _canMove = false;

    [Header("Obstacle Avoidance Config")]
    [Space(5)]
    [SerializeField] private Vector3 _sensorStartPositionOffset;
    [SerializeField] private float _lengthBetweenFrontSensors;
    [SerializeField] private float _sensorAngle;
    [SerializeField] private float _sensorLength;
    [SerializeField] private float _obstacleAvoidanceTimeout;

    [Header("Path Config")]
    [Space(5)]
    [SerializeField] private Transform _path;
    [SerializeField] private bool _isCircuitMode;
    [SerializeField] private float _waypointGizmosRadius;
    [SerializeField] private float _waypointArriveErrorTolerance;
    [SerializeField] private float _minSpeedToBrake = 20f;
    [SerializeField] private float _minAngleBetweenWaypointToBrake = 10f;
    [SerializeField] private float _minDistanceToWaypointToBrake = 10f;

    private const float MinSteerInput = -1f;
    private const float NeutralSteerInput = 0f;
    private const float MaxSteerInput = 1f;
    private const float HighDegreeSteerMultiplier = 20f;
    private const float LowDegreeSteerMultiplier = 10f;
    private const float MinSteerMultiplier = -30f;
    private const float MaxSteerMultiplier = 30f;
    private const float NeutralSteerMultiplier = 0f;
    private const string LayerMaskObstacle = "Obstacle";

    private float _obstacleAvoidanceCounter = 0f;
    private bool _isAvoidingObstacle = false;
    private int _currentWaypointIndex = 0;

    public FrameInput Input { get; set; } = new FrameInput();

    private void OnDrawGizmos()
    {
        //Draw path
        Gizmos.color = Color.green;

        for (int i = 0; i < _path.childCount; i++)
        {
            //To show active waypoint more clear
            if (i == _currentWaypointIndex)
            {
                Gizmos.color = Color.magenta;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawSphere(_path.GetChild(i).transform.position, _waypointGizmosRadius);
            Gizmos.DrawWireSphere(_path.GetChild(i).transform.position, _waypointArriveErrorTolerance);
        }

        IEnumerable<Vector3> waypointPositions = _path.GetComponentsInChildren<Waypoint>().Select(w => w.transform.position);
        Gizmos.DrawLineStrip(waypointPositions.ToArray().AsSpan(), false);
    }

    private void Update()
    {
        if (!_canMove) return;

        AvoidObstacle();

        if (_isAvoidingObstacle)
        {
            print("OBSTACLE AVOID STATE");
            _obstacleAvoidanceCounter += Time.deltaTime;
        }
        else
        {
            _obstacleAvoidanceCounter = 0f;
        }

        if (_obstacleAvoidanceCounter > _obstacleAvoidanceTimeout)
        {
            Debug.Log("GO BACK STATE");
        }

        FollowPath();

        UpdateDefaultInputs();
    }

    private void AvoidObstacle()
    {
        float steerMultiplier = 0f;
        LayerMask layerMask = LayerMask.GetMask(LayerMaskObstacle);
        RaycastHit middleSensorHit = default;
        int numberOfSensorsDetectedObstacle = 0;

        Vector3 sensorStartPosition = transform.TransformPoint(_sensorStartPositionOffset);

        //Front Middle Sensor
        if (Physics.Raycast(sensorStartPosition, transform.forward, out RaycastHit hit, _sensorLength, layerMask))
        {
            ++numberOfSensorsDetectedObstacle;
            middleSensorHit = hit;
            Debug.DrawRay(sensorStartPosition, transform.forward * _sensorLength, Color.red);
        }
        else
        {
            Debug.DrawRay(sensorStartPosition, transform.forward * _sensorLength, Color.green);
        }

        sensorStartPosition -= _lengthBetweenFrontSensors * transform.right;

        //Front Left Sensor
        if (Physics.Raycast(sensorStartPosition, transform.forward, _sensorLength, layerMask))
        {
            ++numberOfSensorsDetectedObstacle;
            steerMultiplier += HighDegreeSteerMultiplier;
            Debug.DrawRay(sensorStartPosition, transform.forward * _sensorLength, Color.red);
        }
        else
        {
            Debug.DrawRay(sensorStartPosition, transform.forward * _sensorLength, Color.green);
        }

        //Front Left Angled Sensor
        if (Physics.Raycast(sensorStartPosition, Quaternion.AngleAxis(-_sensorAngle, transform.up) * transform.forward, _sensorLength, layerMask))
        {
            ++numberOfSensorsDetectedObstacle;
            steerMultiplier += LowDegreeSteerMultiplier;
            Debug.DrawRay(sensorStartPosition, Quaternion.AngleAxis(-_sensorAngle, transform.up) * transform.forward * _sensorLength, Color.red);
        }
        else
        {
            Debug.DrawRay(sensorStartPosition, Quaternion.AngleAxis(-_sensorAngle, transform.up) * transform.forward * _sensorLength, Color.green);
        }

        sensorStartPosition += 2f * _lengthBetweenFrontSensors * transform.right;

        //Front Right Sensor
        if (Physics.Raycast(sensorStartPosition, transform.forward, _sensorLength, layerMask))
        {
            ++numberOfSensorsDetectedObstacle;
            steerMultiplier += -1f * HighDegreeSteerMultiplier;
            Debug.DrawRay(sensorStartPosition, transform.forward * _sensorLength, Color.red);
        }
        else
        {
            Debug.DrawRay(sensorStartPosition, transform.forward * _sensorLength, Color.green);
        }

        //Front Right Angled Sensor
        if (Physics.Raycast(sensorStartPosition, Quaternion.AngleAxis(_sensorAngle, transform.up) * transform.forward, _sensorLength, layerMask))
        {
            ++numberOfSensorsDetectedObstacle;
            steerMultiplier += -1f * LowDegreeSteerMultiplier;
            Debug.DrawRay(sensorStartPosition, Quaternion.AngleAxis(_sensorAngle, transform.up) * transform.forward * _sensorLength, Color.red);
        }
        else
        {
            Debug.DrawRay(sensorStartPosition, Quaternion.AngleAxis(_sensorAngle, transform.up) * transform.forward * _sensorLength, Color.green);
        }

        //After process all raycast, check sensor hit count
        print("SENSOR HIT: " + numberOfSensorsDetectedObstacle);
        if (numberOfSensorsDetectedObstacle > 0)
        {
            _isAvoidingObstacle = true;
        }
        else
        {
            _isAvoidingObstacle = false;
            return;
        }

        //STEER SECTION
        //All ray hit
        float newSteerInput = NeutralSteerInput;
        if (_isAvoidingObstacle && Mathf.Abs(steerMultiplier - NeutralSteerMultiplier) < 0.01)
        {
            //Dot result is already between min-max steer input interval (-1, 1),
            //there is no need to inverse lerp.
            float middleSensorDotResult = Vector3.Dot(middleSensorHit.normal, transform.right);
            if (middleSensorDotResult > 0f)
            {
                newSteerInput = MaxSteerInput - middleSensorDotResult;
            }
            else
            {
                newSteerInput = MinSteerInput + middleSensorDotResult;
            }
        }
        else
        {
            float inversedSteerMultiplier = Mathf.InverseLerp(
                MinSteerMultiplier,
                MaxSteerMultiplier,
                steerMultiplier);
            float lerpedSteerMultiplier = Mathf.Lerp(
                MinSteerInput,
                MaxSteerInput,
                inversedSteerMultiplier);

            newSteerInput = lerpedSteerMultiplier;
        }

        //Update Input
        Input.SteerInput = newSteerInput;

        Gas();
    }

    private void FollowPath()
    {
        if (HasArrivedWaypoint())
        {
            SetNextWaypoint();
        }

        //If AI is avoiding obstacle, ignore path
        //but do not ignore arriving waypoints
        if (_isAvoidingObstacle) return;

        if (ShouldBrake())
        {
            Brake();
        }
        else
        {
            Gas();
        }

        Vector3 targetWaypointPosition = _path.GetChild(_currentWaypointIndex).position;
        Vector3 directionToTargetWaypoint = (targetWaypointPosition - transform.position);

        /*
         * If dot product result is > 0, obstacle is on the right of the car
         * < 0, obstacle is on the left of the car
         * == 0, in front or rear of the car
         * **/
        float steerResult = Vector3.Dot(transform.right, directionToTargetWaypoint.normalized);

        //Update Input
        Input.SteerInput = steerResult;
    }

    private bool ShouldBrake()
    {
        Transform currentWaypoint = _path.GetChild(_currentWaypointIndex);
        float distanceToWaypoint = Vector3.Distance(
            transform.position,
            currentWaypoint.position);

        Vector3 directionToWaypoint = (currentWaypoint.position - transform.position);
        float angleBetweenWaypoint = Vector3.Angle(transform.forward, directionToWaypoint);

        float currentSpeed = GetComponent<Rigidbody>().velocity.magnitude;

        if (distanceToWaypoint > _minDistanceToWaypointToBrake
            && angleBetweenWaypoint > _minAngleBetweenWaypointToBrake
            && currentSpeed > _minSpeedToBrake)
        {
            return true;
        }

        return false;
    }

    private void Gas()
    {
        Input.GasInput = true;
        Input.BrakeInput = false;
    }

    private void Brake()
    {
        Input.GasInput = false;
        Input.BrakeInput = true;
    }

    private bool HasArrivedWaypoint()
    {
        float distanceToCurrentWaypoint = Vector3.Distance(
            transform.position,
            _path.GetChild(_currentWaypointIndex).position);

        if (distanceToCurrentWaypoint <= _waypointArriveErrorTolerance)
        {
            return true;
        }

        return false;
    }

    private void SetNextWaypoint()
    {
        if (_isCircuitMode && _currentWaypointIndex == _path.childCount - 1)
        {
            _currentWaypointIndex = 0;
        }
        else if (_currentWaypointIndex < _path.childCount - 1)
        {
            ++_currentWaypointIndex;
        }
    }

    private void UpdateDefaultInputs()
    {
        Input.HandbrakeInput = false;
        Input.NitroInput = false;
        Input.ShiftDownInput = false;
        Input.ShiftUpInput = false;
    }
}
