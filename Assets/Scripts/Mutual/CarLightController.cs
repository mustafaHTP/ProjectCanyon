using System.Collections.Generic;
using UnityEngine;

public class CarLightController : MonoBehaviour
{
    [SerializeField] private List<Light> headLights;
    [SerializeField] private List<Light> headlightFlares;
    [SerializeField] private List<Light> backLightFlares;

    private bool _isHeadlightOn = true;
    private IInput _input;

    public void TurnOnBackLights()
    {
        for (var i = 0; i < backLightFlares.Count; ++i)
        {
            backLightFlares[i].enabled = true;
        }
    }

    public void TurnOffBackLights()
    {
        for (var i = 0; i < backLightFlares.Count; ++i)
        {
            backLightFlares[i].enabled = false;
        }
    }

    private void Awake()
    {
        _input = GetComponent<IInput>();
        InitHeadlight();
    }

    private void Update()
    {
        if (_input.FrameInput.ToggleHeadligthInput)
        {
            ToggleHeadlight();
        }
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
}