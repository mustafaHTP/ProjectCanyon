using System.Collections.Generic;
using UnityEngine;

public class NitroController : MonoBehaviour
{
    [Header("Experimental")]
    [Space(5)]
    [SerializeField] private float _nitroForce;

    [Header("Nitro Effects")]
    [SerializeField] private List<ParticleSystem> nitroExhaustEffects;
    [SerializeField] private List<TrailRenderer> tailLightTrailEffects;

    [Header("Nitro Settings")]
    [SerializeField] private float maxNitroAmount;
    [SerializeField] private float nitroReductionAmount;
    [SerializeField] private float nitroIncreaseAmount;
    [SerializeField] private float nitroRechargeDelayTime;
    [Range(1, 4)]
    [Tooltip("It is used multiply by default acceleration")]
    [SerializeField]
    private float accelerationMultiplier;

    private CarController _carController;
    private CarSoundController _carSoundController;
    private IInput _input;
    private float _currentNitroAmount;
    private float _nitroRPMAcceleration;
    private float _defaultRPMAcceleration;
    private bool _isUsingNitro;

    public float MaxNitroAmount { get => maxNitroAmount; }
    public float NitroReductionAmount { get => nitroReductionAmount; }
    public float CurrentNitroAmount { get => _currentNitroAmount; }
    public bool IsUsingNitro { get => _isUsingNitro; }

    private void Awake()
    {
        _carController = GetComponent<CarController>();
        _carSoundController = GetComponent<CarSoundController>();
        _input = GetComponent<IInput>();
        _currentNitroAmount = maxNitroAmount;
    }

    private void Update()
    {
        ProcessInput();

        if (_isUsingNitro)
        {
            StartNitro();
        }
        else
        {
            StopNitro();
        }
    }

    private void ProcessInput()
    {
        if (_input.Input.GasInput)
        {
            if (_input.Input.NitroInput)
            {
                _isUsingNitro = true;
            }
            else
            {
                _isUsingNitro = false;
            }
        }
        else
        {
            _isUsingNitro = false;
        }
    }

    private void StartNitro()
    {
        //Check there is enough nitro amount to play nitro
        if (_currentNitroAmount - nitroReductionAmount > 0f)
        {
            _isUsingNitro = true;

            _carSoundController.PlayNitroSound();

            //Apply Nitro Force
            _carController.GetComponent<Rigidbody>().AddForce(_carController.transform.forward * _nitroForce);

            DecreaseNitro();
            TurnOnVisualEffects();
        }
        else
        {
            _isUsingNitro = false;

            _carSoundController.StopNitroSound();
            TurnOffVisualEffects();
            Invoke(nameof(IncreaseNitro), nitroRechargeDelayTime);
        }

    }

    private void StopNitro()
    {
        _isUsingNitro = false;

        _carSoundController.StopNitroSound();
        IncreaseNitro();
        TurnOffVisualEffects();
    }

    private void TurnOnVisualEffects()
    {
        for (var i = 0; i < nitroExhaustEffects.Count; ++i)
        {
            bool isPlaying = nitroExhaustEffects[i].isPlaying;
            if (!isPlaying)
            {
                nitroExhaustEffects[i].Play();
            }
        }

        foreach (TrailRenderer trailRenderer in tailLightTrailEffects)
        {
            if (!trailRenderer.emitting)
                trailRenderer.emitting = true;
        }
    }

    private void TurnOffVisualEffects()
    {
        for (var i = 0; i < nitroExhaustEffects.Count; ++i)
        {
            bool isPlaying = nitroExhaustEffects[i].isPlaying;
            if (isPlaying)
            {
                nitroExhaustEffects[i].Stop();
            }
        }

        foreach (TrailRenderer trailRenderer in tailLightTrailEffects)
        {
            if (trailRenderer.emitting)
                trailRenderer.emitting = false;
        }
    }

    private void IncreaseNitro()
    {
        _currentNitroAmount += nitroIncreaseAmount * Time.deltaTime;
        _currentNitroAmount = _currentNitroAmount > maxNitroAmount ? maxNitroAmount : _currentNitroAmount;
    }

    private void DecreaseNitro()
    {
        _currentNitroAmount -= nitroReductionAmount * Time.deltaTime;
        _currentNitroAmount = _currentNitroAmount < 0 ? 0 : _currentNitroAmount;
    }
}