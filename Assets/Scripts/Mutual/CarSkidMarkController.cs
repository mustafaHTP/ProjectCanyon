using System.Collections.Generic;
using UnityEngine;

public class CarSkidMarkController : MonoBehaviour
{
    [SerializeField] private List<TrailRenderer> _skidMarkVFX;

    private CarController _carController;

    private void Awake()
    {
        if (!TryGetComponent(out _carController))
        {
            Debug.LogError($"{nameof(CarController)} has not been found !");
        }
    }

    private void OnEnable()
    {
        _carController.OnGrip += CarController_OnGrip;
        _carController.OnDrift += CarController_OnDrift;
    }

    private void OnDisable()
    {
        _carController.OnGrip -= CarController_OnGrip;
        _carController.OnDrift -= CarController_OnDrift;
    }

    private void CarController_OnGrip()
    {
        DisableEffect();
    }

    private void CarController_OnDrift()
    {
        EnableEffect();
    }

    private void EnableEffect()
    {
        foreach (var item in _skidMarkVFX)
        {
            if (!item.emitting)
            {
                item.emitting = true;
            }
        }
    }

    private void DisableEffect()
    {
        foreach (var item in _skidMarkVFX)
        {
            if (item.emitting)
            {
                item.emitting = false;
            }
        }
    }
}