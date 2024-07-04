using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugActionController : MonoBehaviour
{
    [Header("Debug UI")]
    [SerializeField] private bool _isDebugUIOnAtAwake;
    [SerializeField] private GameObject _debugUI;

    [Header("Info UI")]
    [SerializeField] private bool _isInfoUIOnAtAwake;
    [SerializeField] private GameObject _infoUI;

    private bool _isDebugUIOn;
    private bool _isInfoUIOn;

    private void Awake()
    {
        InitUI();
    }

    private void InitUI()
    {
        _isDebugUIOn = _isDebugUIOnAtAwake;
        _isInfoUIOn = _isInfoUIOnAtAwake;

        _debugUI.SetActive(_isDebugUIOn);
        _infoUI.SetActive(_isInfoUIOnAtAwake);
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
