using Cinemachine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _parentOfCameras;

    private const int HighPriorityCameraValue = 20;
    private const int LowPriorityCameraValue = 10;

    private int _activeCameraIndex = 0;
    private CinemachineVirtualCamera _activeCamera;
    private List<CinemachineVirtualCamera> _cameras;

    public void SwitchCamera()
    {
        int oldActiveCameraIndex = _activeCameraIndex;
        int newActiveCameraIndex = oldActiveCameraIndex + 1;

        if (newActiveCameraIndex == _cameras.Count)
        {
            newActiveCameraIndex = 0;
        }

        _cameras[oldActiveCameraIndex].Priority = LowPriorityCameraValue;
        _cameras[newActiveCameraIndex].Priority = HighPriorityCameraValue;

        _activeCameraIndex = newActiveCameraIndex;
        _activeCamera = _cameras[newActiveCameraIndex];
    }

    private void Awake()
    {
        _cameras = _parentOfCameras.GetComponentsInChildren<CinemachineVirtualCamera>().ToList();

        _activeCamera = _cameras[_activeCameraIndex];

        for (int i = 0; i < _cameras.Count; i++)
        {
            if (i == _activeCameraIndex)
            {
                _cameras[i].Priority = HighPriorityCameraValue;
            }
            else
            {
                _cameras[i].Priority = LowPriorityCameraValue;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchCamera();
        }
    }

    private void LateUpdate()
    {
        _activeCamera.GetComponent<ICinemachineCameraLogic>()?.PerformCameraLogic();
    }
}
