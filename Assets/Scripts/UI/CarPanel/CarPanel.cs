using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPanel : MonoBehaviour, IPanel
{
    [Header("Panel GameObject")]
    [SerializeField] private GameObject _panel;

    public GameObject AttachedGameObject => _panel;

    public bool IsPanelActive { get; set; }

    public void DisablePanel() => gameObject.SetActive(false);

    public void EnablePanel() => gameObject.SetActive(true);

    public void TogglePanel()
    {
        IsPanelActive = !IsPanelActive;
        gameObject.SetActive(IsPanelActive);
    }
}
