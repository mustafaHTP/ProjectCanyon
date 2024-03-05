using Unity.VisualScripting;
using UnityEngine;

public class DebugActionController : MonoBehaviour
{
    [SerializeField] private GameObject _debugUI;
    [SerializeField] private Transform[] _spawnPositions; 

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

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SpawnCar(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SpawnCar(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SpawnCar(2);
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

    private void SpawnCar(int spawnPositionIndex)
    {
        if(spawnPositionIndex < 0 || spawnPositionIndex >= _spawnPositions.Length)
        {
            Debug.LogError("Spawn Position Index Out Of Range");
            return;
        }

        GetComponent<Rigidbody>().velocity = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.position = _spawnPositions[spawnPositionIndex].position;
    }
}
