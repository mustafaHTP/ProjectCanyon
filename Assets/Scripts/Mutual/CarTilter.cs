using UnityEngine;

public class CarTilter : MonoBehaviour
{
    [Header("Car Body Tilt Settings")]
    [Space(5)]
    [SerializeField] private MeshRenderer carBody;
    [SerializeField] private GameObject nitroEffect;
    [SerializeField] private GameObject headlightFlare;
    [SerializeField] private GameObject backlightFlare;
    [SerializeField] private float maxTiltAngle;
    [SerializeField] private float tiltSpeed;

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
        float dotResult = Vector3.Dot(_rigidbody.velocity.normalized, _rigidbody.transform.right.normalized);
        float inversedDotResult = Mathf.InverseLerp(1, -1, dotResult);
        float tiltAmount = Mathf.Lerp(-maxTiltAngle, maxTiltAngle, inversedDotResult);

        //Tilt car body
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, tiltAmount);
        Quaternion currentRotation = carBody.transform.localRotation;
        carBody.transform.localRotation = Quaternion.Slerp(
            currentRotation,
            targetRotation,
            tiltSpeed * Time.fixedDeltaTime);

        //Tilt headlight flare
        currentRotation = headlightFlare.transform.localRotation;
        headlightFlare.transform.localRotation = Quaternion.Slerp(
            currentRotation,
            targetRotation,
            tiltSpeed * Time.fixedDeltaTime);

        //Tilt backlight flare
        currentRotation = backlightFlare.transform.localRotation;
        backlightFlare.transform.localRotation = Quaternion.Slerp(
            currentRotation,
            targetRotation,
            tiltSpeed * Time.fixedDeltaTime);

        //Tilt nitro effect
        currentRotation = nitroEffect.transform.localRotation;
        nitroEffect.transform.localRotation = Quaternion.Slerp(
            currentRotation,
            targetRotation,
            tiltSpeed * Time.fixedDeltaTime);
    }
}
