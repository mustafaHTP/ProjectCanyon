using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugActionController : MonoBehaviour
{
    [SerializeField] private GameObject _debugUI;
    [SerializeField] private GameObject _infoUI;

    private bool _isDebugUIOn = false;
    private bool _isInfoUIOn = true;

    private void Awake()
    {
        InitUI();
    }

    private void InitUI()
    {
        _debugUI.SetActive(_isDebugUIOn);
        _infoUI.SetActive(_isInfoUIOn);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCarRotation();
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            ToggleDebugUI();
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleInfoUI();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetScene();
        }
    }

    private void ResetScene()
    {
        int currentSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneBuildIndex);
    }

    private void ResetCarRotation()
    {
        transform.rotation = Quaternion.identity;
    }

    private void ToggleDebugUI()
    {
        _isDebugUIOn = !_isDebugUIOn;
        _debugUI.SetActive(_isDebugUIOn);
    }

    private void ToggleInfoUI()
    {
        _isInfoUIOn = !_isInfoUIOn;
        _infoUI.SetActive(_isInfoUIOn);
    }
}
