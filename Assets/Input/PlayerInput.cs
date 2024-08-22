using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour, IInput
{
    public FrameInput FrameInput { get; set; } = new FrameInput();

    private PlayerInputActions _inputActions;
    private InputAction _gasAction;
    private InputAction _brakeAction;
    private InputAction _nitroAction;
    private InputAction _handbrakeAction;
    private InputAction _steerAction;
    private InputAction _shiftUpAction;
    private InputAction _shiftDownAction;
    private InputAction _changeCameraAction;
    private InputAction _toggleHeadlightAction;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();

        _gasAction = _inputActions.Player.Gas;
        _brakeAction = _inputActions.Player.Brake;
        _nitroAction = _inputActions.Player.Nitro;
        _handbrakeAction = _inputActions.Player.Handbrake;
        _steerAction = _inputActions.Player.Steer;
        _shiftUpAction = _inputActions.Player.ShiftUp;
        _shiftDownAction = _inputActions.Player.ShiftDown;
        _changeCameraAction = _inputActions.Player.ChangeCamera;
        _toggleHeadlightAction = _inputActions.Player.ToggleHeadlight;
    }

    private void OnEnable()
    {
        _inputActions.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Disable();
    }

    private void Update()
    {
        FrameInput = GetInput();
    }

    private FrameInput GetInput()
    {
        return new FrameInput
        {
            GasInput = _gasAction.ReadValue<float>(),
            BrakeInput = _brakeAction.ReadValue<float>(),
            NitroInput = _nitroAction.IsPressed(),
            HandbrakeInput = _handbrakeAction.ReadValue<float>(),
            SteerInput = _steerAction.ReadValue<float>(),
            ShiftUpInput = _shiftUpAction.WasPressedThisFrame(),
            ShiftDownInput = _shiftDownAction.WasPressedThisFrame(),
            ChangeCameraInput = _changeCameraAction.WasPressedThisFrame(),
            ToggleHeadligthInput = _toggleHeadlightAction.WasPressedThisFrame()
        };
    }
}
