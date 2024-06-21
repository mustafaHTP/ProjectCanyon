using System.Collections.Generic;
using UnityEngine;

public class CarSkidMarkController : MonoBehaviour
{
    [Header("SkidMark Effects")]
    [SerializeField] private List<TrailRenderer> skidMarkEffects;

    [Header("Wheel Colliders")]
    [SerializeField] private List<WheelCollider> wheelColliders;

    private CarSoundController _carSoundController;

    public void TurnOnEffect()
    {
        bool areAllWheelsGrounded = AreAllWheelsGrounded();

        if (areAllWheelsGrounded)
        {
            foreach (var item in skidMarkEffects)
            {
                if (!item.emitting)
                {
                    item.emitting = true;
                    _carSoundController.PlayTireScreechSFX();
                }
            }
        }
        else
        {
            TurnOffEffect();
        }
    }

    public void TurnOffEffect()
    {
        foreach (var item in skidMarkEffects)
        {
            if (item.emitting)
            {
                item.emitting = false;
                _carSoundController.StopTireScreechSFX();
            }
        }
    }

    private void Awake()
    {
        _carSoundController = GetComponent<CarSoundController>();
    }

    private bool AreAllWheelsGrounded()
    {
        foreach (var wheel in wheelColliders)
        {
            if (!wheel.isGrounded)
            {
                return false;
            }
        }

        return true;
    }
}