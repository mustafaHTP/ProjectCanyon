using System.Collections.Generic;
using UnityEngine;

public class CarSmokeController : MonoBehaviour
{
    [Header("Car Smokes")]
    [SerializeField] private List<ParticleSystem> _carSmokeVFX;

    private ParticleSystem.EmissionModule _carSmokeEmission;
    private CarController _carController;

    private void Awake()
    {
        if (!TryGetComponent<CarController>(out _carController))
        {
            Debug.LogError($"{nameof(CarController)} has not been found !");
        }

        /*
         * Car smoke is particle system. If PlayOnAwake not is ticked,
         * car smokes cannot be enabled or disabled. For that reason,
         * PlayOnAwake is ticked, and must be disabled in Awake
         */
        DisableEffect();
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
        for (int i = 0; i < _carSmokeVFX.Count; i++)
        {
            _carSmokeEmission = _carSmokeVFX[i].emission;
            _carSmokeEmission.enabled = true;
        }
    }

    private void DisableEffect()
    {
        for (int i = 0; i < _carSmokeVFX.Count; i++)
        {
            _carSmokeEmission = _carSmokeVFX[i].emission;
            _carSmokeEmission.enabled = false;
        }
    }
}
