using System;
using UnityEngine;
using UnityEngine.UI;

public class NitroBar : MonoBehaviour
{
    [SerializeField] private Color _nitroEnteredCooldownColor;
    [SerializeField] private Color _nitroExitedCooldownColor;

    private Transform _playerCar;
    private Image _nitroBarImage;
    private CarNitroController _carNitroController;

    private void Awake()
    {
        FindPlayerCar();
        _nitroBarImage = GetComponent<Image>();
        if (!_playerCar.TryGetComponent(out _carNitroController))
        {
            Debug.LogError($"{nameof(CarNitroController)} has not been found !");
        }
        UpdateFillValue();
        ChangeColorNitroBar(true);
    }

    private void OnEnable()
    {
        _carNitroController.OnNitroCooldown += CarNitroController_OnNitroCooldown;
    }

    private void Update()
    {
        UpdateFillValue();
    }
    
    private void OnDisable()
    {
        _carNitroController.OnNitroCooldown -= CarNitroController_OnNitroCooldown;
    }

    private void UpdateFillValue()
    {
        float currentNitroAmount = _carNitroController.CurrentNitroAmount;
        float maxNitroAmount = _carNitroController.MaxNitroAmount;
        float nitroReductionAmount = _carNitroController.NitroReductionAmount;

        float mappedValue = Mathf.InverseLerp(nitroReductionAmount, maxNitroAmount, currentNitroAmount);
        float fillAmount = Mathf.Lerp(0f, 1f, mappedValue);
        _nitroBarImage.fillAmount = fillAmount;
    }

    private void FindPlayerCar()
    {
        _playerCar = FindAnyObjectByType<Player>().transform;
        if (_playerCar == null)
        {
            Debug.LogError("Player car has not been found !");
        }
    }

    private void CarNitroController_OnNitroCooldown(bool isOnCooldown)
    {
        ChangeColorNitroBar(isOnCooldown);
    }

    private void ChangeColorNitroBar(bool isOnCooldown)
    {
        if (isOnCooldown)
        {
            _nitroBarImage.color = _nitroEnteredCooldownColor;
        }
        else
        {
            _nitroBarImage.color = _nitroExitedCooldownColor;
        }
    }
}
