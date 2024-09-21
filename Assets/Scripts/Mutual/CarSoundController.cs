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

    private CarController _carController;
    private CarNitroController _carNitroController;
    private IInput _input;

    private void Awake()
    {
        if (!TryGetComponent(out _carController))
        {
            Debug.LogError($"{nameof(CarController)} has not been found !");
        }
        if (!TryGetComponent(out _carNitroController))
        {
            Debug.LogError($"{nameof(CarNitroController)} has not been found !");
        }
        _input = GetComponent<IInput>();
    }

    private void OnEnable()
    {
        _carController.OnGrip += CarController_OnGrip;
        _carController.OnDrift += CarController_OnDrift;

        _carNitroController.OnNitroActivated += CarNitroController_OnNitroActivated;
        _carNitroController.OnNitroDeactivated += CarNitroController_OnNitroDeactivated;
    }

    private void CarNitroController_OnNitroDeactivated()
    {
        StopNitroSFX();
    }

    private void CarNitroController_OnNitroActivated()
    {
        PlayNitroSFX();
    }

    private void FixedUpdate()
    {
        PlayEngineSFX();
    }

    private void OnDisable()
    {
        _carController.OnGrip -= CarController_OnGrip;
        _carController.OnDrift -= CarController_OnDrift;

        _carNitroController.OnNitroActivated -= CarNitroController_OnNitroActivated;
        _carNitroController.OnNitroDeactivated -= CarNitroController_OnNitroDeactivated;
    }

    #region EVENT METHODS
    private void CarController_OnGrip()
    {
        StopTireScreechSFX();
    }

    private void CarController_OnDrift()
    {
        PlayTireScreechSFX();
    }
    #endregion

    #region SOUND METHODS
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

    private void PlayTireScreechSFX()
    {
        if (!_tireScreechSFX.isPlaying)
        {
            _tireScreechSFX.Play();
        }
    }

    private void StopTireScreechSFX()
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
    #endregion
}