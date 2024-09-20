using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CarLightController : MonoBehaviour
{
    [SerializeField] private List<Light> headLights;
    [SerializeField] private List<Light> headlightFlares;
    [SerializeField] private List<Light> backLightFlares;

    private bool _isHeadlightOn = true;
    private IInput _input;
    private CarController _carController;

    private void Awake()
    {
        _input = GetComponent<IInput>();
        if(!TryGetComponent<CarController>(out _carController))
        {
            Debug.LogError($"{nameof(CarController)} has not been found !");
        }
        InitHeadlight();
    }

    private void OnEnable()
    {
        _carController.OnBrake += CarController_OnBrake;
    }

    private void Update()
    {
        if (_input.FrameInput.ToggleHeadligthInput)
        {
            ToggleHeadlight();
        }
    }

    private void OnDisable()
    {
        _carController.OnBrake -= CarController_OnBrake;
    }

    private void CarController_OnBrake(bool isBraking)
    {
        ToggleBackLights(isBraking);
    }

    private void InitHeadlight()
    {
        foreach (var item in headLights)
        {
            item.enabled = _isHeadlightOn;
        }

        foreach (var item in headlightFlares)
        {
            item.enabled = _isHeadlightOn;
        }
    }

    private void ToggleHeadlight()
    {
        _isHeadlightOn = !_isHeadlightOn;

        foreach (var item in headLights)
        {
            item.enabled = _isHeadlightOn;
        }

        foreach (var item in headlightFlares)
        {
            item.enabled = _isHeadlightOn;
        }
    }

    private void ToggleBackLights(bool isBraking)
    {
        for (var i = 0; i < backLightFlares.Count; ++i)
        {
            backLightFlares[i].enabled = isBraking;
        }
    }
}