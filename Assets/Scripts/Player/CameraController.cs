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
        ++_activeCameraIndex;
        if (_activeCameraIndex == _cameras.Count)
        {
            _activeCameraIndex = 0;
        }

        for (int i = 0; i < _cameras.Count; i++)
        {
            if (i == _activeCameraIndex)
            {
                _cameras[i].Priority = 20;
            }
            else
            {
                _cameras[i].Priority = 10;
            }
        }

        _activeCamera = _cameras[_activeCameraIndex];
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

    private void FixedUpdate()
    {
        _activeCamera.GetComponent<ICinemachineCameraLogic>()?.PerformCameraLogic();
    }
}
