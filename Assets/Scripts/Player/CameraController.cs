using Cinemachine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const int HighPriorityCameraValue = 20;
    private const int LowPriorityCameraValue = 10;

    private int _activeCameraIndex = 0;
    private Transform _parentOfCameras;
    private CinemachineVirtualCamera _activeCamera;
    private CinemachineVirtualCamera [] _cameras;
    private IInput _input;

    public void SwitchCamera()
    {
        int oldActiveCameraIndex = _activeCameraIndex;
        int newActiveCameraIndex = oldActiveCameraIndex + 1;

        if (newActiveCameraIndex == _cameras.Length)
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
        _input = GetComponent<IInput>();
        InitCameras();
    }

    private void InitCameras()
    {
        _parentOfCameras = FindAnyObjectByType<ParentOfCameras>().transform;
        if(_parentOfCameras == null)
        {
            Debug.LogError("Parent of Cameras has not been found !");
        }

        _cameras = new CinemachineVirtualCamera[_parentOfCameras.childCount];
        _cameras = _parentOfCameras.GetComponentsInChildren<CinemachineVirtualCamera>();

        //Select active camera when initializing
        _activeCamera = _cameras[_activeCameraIndex];

        for (int i = 0; i < _cameras.Length; i++)
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
        if (_input.Input.ChangeCameraInput)
        {
            SwitchCamera();
        }
    }

    private void LateUpdate()
    {
        _activeCamera.GetComponent<ICinemachineCameraLogic>()?.PerformCameraLogic();
    }
}
