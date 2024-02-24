using UnityEngine;

public class DebugActionController : MonoBehaviour
{
    [SerializeField] private GameObject _debugUI;

    private bool _isDebugUIOn = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCarPosition();
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleDebugUI();
        }
    }

    private void ResetCarPosition()
    {
        transform.rotation = Quaternion.identity;
    }

    private void ToggleDebugUI()
    {
        if (_isDebugUIOn)
        {
            _debugUI.SetActive(false);
            _isDebugUIOn = false;
        }
        else
        {
            _debugUI.SetActive(true);
            _isDebugUIOn = true;
        }
    }
}
