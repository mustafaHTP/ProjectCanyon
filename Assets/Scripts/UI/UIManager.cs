using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Debug Panel")]
    [SerializeField] private bool _isActiveOnStartDebugPanel;
    [SerializeField] private GameObject _debugPanel;

    [Header("Car Panel")]
    [SerializeField] private bool _isActiveOnStartCarPanel;
    [SerializeField] private GameObject _carPanel;

    [Header("Debug Panel")]
    [SerializeField] private bool _isActiveOnStartInfoPanel;
    [SerializeField] private GameObject _infoPanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleDebugPanel();
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            ToggleCarPanel();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            ToggleInfoPanel();
        }
    }

    private void ToggleInfoPanel()
    {
        _infoPanel.SetActive(!_infoPanel.activeInHierarchy);
    }

    private void ToggleCarPanel()
    {
        _carPanel.SetActive(!_carPanel.activeInHierarchy);
    }

    private void ToggleDebugPanel()
    {
        _debugPanel.SetActive(!_debugPanel.activeInHierarchy);
    }
}
