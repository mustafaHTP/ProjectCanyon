using System;
using UnityEngine;

public class CarSoundController : MonoBehaviour
{
    [Header("Engine SFX")]
    [SerializeField] private AudioSource _engineSFX;
    [SerializeField] private float _minPitchValue = 0.8f;
    [SerializeField] private float _maxPitchValue = 2.5f;

    [Header("Nitro SFX")]
    [SerializeField] private AudioSource _nitroSFX;

    [Header("Tire Screech SFX")]
    [SerializeField] private AudioSource _tireScreechSFX;

    [Header("Handbrake SFX")]
    [SerializeField] private AudioSource _handbrakeSFX;

    [Header("Handbrake SFX")]
    [SerializeField] private AudioSource _turboSFX;
    [SerializeField] private float _turboVolumeIncrease;
    [SerializeField] private float _turboVolumeDecrease;
    [SerializeField]
    [Range(0f, 1f)] private float _maxTurboVolume;
    [SerializeField]
    [Range(0f, 1f)] private float _minTurboVolume;

    private CarController _carController;
    private IInput _input;

    private void Awake()
    {
        _carController = GetComponent<CarController>();
        _input = GetComponent<IInput>();
    }

    private void FixedUpdate()
    {
        PlayEngineSFX();
        PlayTurboSFX();
    }

    public void PlayEngineSFX()
    {
        float minSpeed = CarController.MinSpeed;
        float topSpeed = _carController.TopSpeed;
        float currentSpeed = _carController.CurrentSpeed;

        float inverseRPM = Mathf.InverseLerp(minSpeed, topSpeed, currentSpeed);
        float currentEngineSoundPitch = Mathf.Lerp(_minPitchValue, _maxPitchValue, inverseRPM);

        _engineSFX.pitch = currentEngineSoundPitch;

        if (!_engineSFX.isPlaying)
            _engineSFX.Play();
    }

    public void StopEngineSFX()
    {
        if (_engineSFX.isPlaying)
            _engineSFX.Stop();
    }

    public void PlayNitroSFX()
    {
        if (!_nitroSFX.isPlaying)
        {
            _nitroSFX.Play();
        }
    }

    public void StopNitroSFX()
    {
        if (_nitroSFX.isPlaying)
        {
            _nitroSFX.Stop();
        }
    }

    public void PlayTireScreechSFX()
    {
        if (!_tireScreechSFX.isPlaying)
        {
            _tireScreechSFX.Play();
        }
    }

    public void StopTireScreechSFX()
    {
        if (_tireScreechSFX.isPlaying)
        {
            _tireScreechSFX.Stop();
        }
    }

    public void PlayHandbrakeSFX()
    {
        if (!_handbrakeSFX.isPlaying)
        {
            _handbrakeSFX.Play();
        }
    }

    public void PlayTurboSFX()
    {
        float normalizedVelocity = _carController.CurrentSpeed / _carController.TopSpeed;
        _turboSFX.volume = Mathf.Lerp(_minTurboVolume, _maxTurboVolume, normalizedVelocity);

        if (!_turboSFX.isPlaying)
        {
            _turboSFX.Play();
        }
    }
}