using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugActionController : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ResetCarRotation();
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
}
