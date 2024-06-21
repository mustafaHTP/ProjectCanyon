using UnityEngine;

public class CarTilter : MonoBehaviour
{
    [Header("Car Body Tilt Settings")]
    [Space(5)]
    [SerializeField] private float _tiltSpeed;
    [SerializeField] private float _maxTiltAngle;
    [SerializeField] private Transform[] _objectsToBeTilted;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        TiltCarBody();
    }

    /// <summary>
    /// Car tilting depends on dot product between velocity 
    /// and right vector of rigidbody.
    /// When tilt car, also other effects that be affected 
    /// by tilting operation needs be rotated
    /// 
    /// </summary>
    private void TiltCarBody()
    {
        CarController carController = GetComponent<CarController>();
        float dotResult =
            Vector3.Dot(_rigidbody.velocity, _rigidbody.transform.right.normalized);
        float inversedDotResult = Mathf.InverseLerp(carController.TopSpeed, -carController.TopSpeed, dotResult);
        float tiltAmount = Mathf.Lerp(-_maxTiltAngle, _maxTiltAngle, inversedDotResult);

        foreach (Transform item in _objectsToBeTilted)
        {
            Quaternion targetRotation = Quaternion.Euler(
                item.localEulerAngles.x,
                item.localEulerAngles.y,
                tiltAmount);
            Quaternion currentRotation = item.localRotation;
            item.transform.localRotation = Quaternion.Slerp(
                currentRotation,
                targetRotation,
                _tiltSpeed * Time.fixedDeltaTime);
        }
    }
}
