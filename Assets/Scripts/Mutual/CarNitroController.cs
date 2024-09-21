using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarNitroController : MonoBehaviour
{
    public event Action OnNitroActivated;
    public event Action OnNitroDeactivated;
    public event Action<bool> OnNitroCooldown;

    [Header("Nitro Effects")]
    [SerializeField] private List<ParticleSystem> nitroExhaustEffects;
    [SerializeField] private List<TrailRenderer> tailLightTrailEffects;

    [Header("Nitro Settings")]
    [SerializeField] private float _nitroForce;
    [SerializeField] private float _maxNitroAmount;
    [SerializeField] private float _nitroReductionAmount;
    [SerializeField] private float _nitroIncreaseAmount;
    [Range(1f, 2f)]
    [SerializeField] private float _nitroActivateThresholdMultiplier;
    [SerializeField] private float _nitroRechargeDelayTime;
    [Tooltip("It is used multiply by default acceleration")]
    [SerializeField]
    private float accelerationMultiplier;

    private const float MinNitroAmount = 0f;

    private CarController _carController;
    private IInput _input;
    private float _currentNitroAmount;
    private float _nitroActivationThreshold;

    #region NITRO STATE
    private enum NitroState
    {
        Cooldown,
        Fill,
        Use
    }

    private NitroState _currentNitroState = NitroState.Cooldown;

    #endregion


    #region Properties
    public float MaxNitroAmount { get => _maxNitroAmount; }
    public float NitroReductionAmount { get => _nitroReductionAmount; }
    public float CurrentNitroAmount { get => _currentNitroAmount; }
    #endregion

    private void Awake()
    {
        _carController = GetComponent<CarController>();
        _input = GetComponent<IInput>();
        _currentNitroAmount = MinNitroAmount;
        _nitroActivationThreshold = CalculateNitroActivationThreshold();

        DisableEffect();
    }

    private void Update()
    {
        switch (_currentNitroState)
        {
            case NitroState.Cooldown:
                PerformCooldownState();
                CheckTransitionOnCooldown();
                break;
            case NitroState.Fill:
                PerformFillState();
                CheckTransitionOnFill();
                break;
            case NitroState.Use:
                PerformUseState();
                CheckTransitionOnUse();
                break;
            default:
                break;
        }

        print("STATE: " + _currentNitroState.ToString().ToUpper());
        print("Nitro Amount: " + CurrentNitroAmount);
    }

    private void EnableEffect()
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

    private void DisableEffect()
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
        _currentNitroAmount += _nitroIncreaseAmount * Time.deltaTime;
        //_currentNitroAmount = _currentNitroAmount > _maxNitroAmount ? _maxNitroAmount : _currentNitroAmount;
        _currentNitroAmount = Mathf.Min(CurrentNitroAmount, MaxNitroAmount);
    }

    private void DecreaseNitro()
    {
        _currentNitroAmount -= _nitroReductionAmount * Time.deltaTime;
        //_currentNitroAmount = _currentNitroAmount < 0 ? 0 : _currentNitroAmount;
        _currentNitroAmount = Mathf.Max(_currentNitroAmount, MinNitroAmount);
    }

    #region NEW NITRO LOGIC

    #region COOLDOWN STATE
    private void PerformCooldownState()
    {
        IncreaseNitro();
        OnNitroCooldown?.Invoke(true);
    }

    private void CheckTransitionOnCooldown()
    {
        if(CurrentNitroAmount > _nitroActivationThreshold)
        {
            ChangeNitroState(NitroState.Fill);
            OnNitroCooldown?.Invoke(false);
        }
    }
    #endregion

    #region FILL STATE
    private void PerformFillState()
    {
        IncreaseNitro();
    }

    private void CheckTransitionOnFill()
    {
        if (IsNitroRequestValid() && CanActivateNitro())
        {
            ChangeNitroState(NitroState.Use);
        }
    }
    #endregion

    #region USE STATE
    private void PerformUseState()
    {
        EnableEffect();
        DecreaseNitro();
        OnNitroActivated?.Invoke();
    }

    private void CheckTransitionOnUse()
    {
        NitroState nextNitroState = _currentNitroState;
        if (IsNitroRequestValid() && !CanActivateNitro())
        {
            nextNitroState = NitroState.Cooldown;
        }
        else if (!IsNitroRequestValid() && CanActivateNitro())
        {
            nextNitroState = NitroState.Fill;
        }

        if (nextNitroState != _currentNitroState)
        {
            DisableEffect();
            ChangeNitroState(nextNitroState);
            OnNitroDeactivated?.Invoke();
        }
    }
    #endregion

    private bool CanActivateNitro()
    {
        return _currentNitroAmount - _nitroReductionAmount > MinNitroAmount;
    }

    private bool IsNitroRequestValid()
    {
        bool isGassing = _input.FrameInput.GasInput switch
        {
            0f => false,
            _ => true
        };

        bool hasNitroUsageRequested = isGassing && _input.FrameInput.NitroInput;

        return hasNitroUsageRequested;
    }

    private void ChangeNitroState(NitroState newNitroState)
    {
        _currentNitroState = newNitroState;
    }

    private float CalculateNitroActivationThreshold()
    {
        return _nitroReductionAmount * _nitroActivateThresholdMultiplier;
    }
    #endregion
}