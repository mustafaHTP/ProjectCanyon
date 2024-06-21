using UnityEngine;
using UnityEngine.UI;

public class NitroBar : MonoBehaviour
{
    [SerializeField] private GameObject car;

    private Image _nitroBar;
    private CarNitroController _nitroController;

    private void Start()
    {
        _nitroBar = GetComponent<Image>();
        _nitroController = car.GetComponent<CarNitroController>();
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
}
