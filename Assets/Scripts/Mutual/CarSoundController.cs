using UnityEngine;

public class CarSoundController : MonoBehaviour
{
    [Header("Engine Sound")]
    [SerializeField] private AudioSource _engineSound;
    [SerializeField] private float _minPitchValue = 0.8f;
    [SerializeField] private float _maxPitchValue = 2.5f;

    [Header("Nitro Sound")]
    [SerializeField] private AudioSource _nitroSound;

    [Header("Tire Screech Sound")]
    [SerializeField] private AudioSource _tireScreechSound;

    private CarController _carController;

    private void Awake()
    {
        _carController = GetComponent<CarController>();
    }

    private void Update()
    {
        PlayEngineSound();
    }

    public void PlayEngineSound()
    {
        float minSpeed = CarController.MinSpeed;
        float topSpeed = _carController.TopSpeed;
        float currentSpeed = _carController.CurrentSpeed;

        float inverseRPM = Mathf.InverseLerp(minSpeed, topSpeed, currentSpeed);
        float currentEngineSoundPitch = Mathf.Lerp(_minPitchValue, _maxPitchValue, inverseRPM);

        _engineSound.pitch = currentEngineSoundPitch;

        if (!_engineSound.isPlaying)
            _engineSound.Play();
    }

    public void StopEngineSound()
    {
        if (_engineSound.isPlaying)
            _engineSound.Stop();
    }

    public void PlayNitroSound()
    {
        if (!_nitroSound.isPlaying)
        {
            _nitroSound.Play();
        }
    }

    public void StopNitroSound()
    {
        if (_nitroSound.isPlaying)
        {
            _nitroSound.Stop();
        }
    }

    public void PlayTireScreechSound()
    {
        if (!_tireScreechSound.isPlaying)
        {
            _tireScreechSound.Play();
        }
    }

    public void StopTireScreechSound()
    {
        if (_tireScreechSound.isPlaying)
        {
            _tireScreechSound.Stop();
        }
    }
}
