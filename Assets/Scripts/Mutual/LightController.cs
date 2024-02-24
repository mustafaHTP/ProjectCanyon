using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    [SerializeField] private List<Light> headLights;
    [SerializeField] private List<Light> headlightFlares;
    [SerializeField] private List<Light> backLightFlares;

    private bool _isHeadlightOn = true;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            _isHeadlightOn = !_isHeadlightOn;
            ToggleHeadlight(_isHeadlightOn);
        }
    }

    private void ToggleHeadlight(bool isHeadlightOn)
    {
        if (isHeadlightOn)
        {
            TurnOnFrontLights();
        }
        else
        {
            TurnOffFrontLights();
        }
    }

    private void TurnOnFrontLights()
    {
        foreach (var item in headLights)
        {
            item.enabled = true;
        }

        foreach (var item in headlightFlares)
        {
            item.enabled = true;
        }
    }

    private void TurnOffFrontLights()
    {
        foreach (var item in headLights)
        {
            item.enabled = false;
        }

        foreach (var item in headlightFlares)
        {
            item.enabled = false;
        }
    }
}