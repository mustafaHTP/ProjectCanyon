using System.Collections.Generic;
using UnityEngine;

public class CarSmokeController : MonoBehaviour
{
    [Header("Car Smokes")]
    [SerializeField] private List<ParticleSystem> _carSmokes;

    private ParticleSystem.EmissionModule _carSmokeEmission;

    public void TurnOnEffect()
    {
        for (int i = 0; i < _carSmokes.Count; i++)
        {
            _carSmokeEmission = _carSmokes[i].emission;
            _carSmokeEmission.enabled = true;
        }
    }

    public void TurnOffEffect()
    {
        for (int i = 0; i < _carSmokes.Count; i++)
        {
            _carSmokeEmission = _carSmokes[i].emission;
            _carSmokeEmission.enabled = false;
        }
    }
}
