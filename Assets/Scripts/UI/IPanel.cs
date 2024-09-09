using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPanel
{
    public GameObject AttachedGameObject { get; }
    public bool IsPanelActive { get; set; }

    void TogglePanel();
    void DisablePanel();
    void EnablePanel();
}
