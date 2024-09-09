using System;
using UnityEngine;
using UnityEngine.UI;

public class NitroBar : MonoBehaviour
{
    private Transform _playerCar;
    private Image _nitroBar;
    private CarNitroController _nitroController;

    private void Awake()
    {
        FindPlayerCar();
    }

    private void Start()
    {
        _nitroBar = GetComponent<Image>();
        _nitroController = _playerCar.GetComponent<CarNitroController>();
    }

    private void Update()
    {
        float currentNitroAmount = _nitroController.CurrentNitroAmount;
        float maxNitroAmount = _nitroController.MaxNitroAmount;
        float nitroReductionAmount = _nitroController.NitroReductionAmount;

        float mappedValue = Mathf.InverseLerp(nitroReductionAmount, maxNitroAmount, currentNitroAmount);
        float fillAmount = Mathf.Lerp(0f, 1f, mappedValue);
        _nitroBar.fillAmount = fillAmount;
    }

    private void FindPlayerCar()
    {
        _playerCar = FindAnyObjectByType<Player>().transform;
        if (_playerCar == null)
        {
            Debug.LogError("Player car has not been found !");
        }
    }
}
