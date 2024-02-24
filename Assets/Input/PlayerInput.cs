using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour, IInput
{
    public FrameInput Input { get; set; } = new FrameInput();

    private PlayerInputActions _inputActions;
    private InputAction _gasAction;
    private InputAction _brakeAction;
    private InputAction _nitroAction;
    private InputAction _handbrakeAction;
    private InputAction _steerAction;
    private InputAction _shiftUpAction;
    private InputAction _shiftDownAction;

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
        Input = GetInput();
    }

    private FrameInput GetInput()
    {
        return new FrameInput
        {
            GasInput = _gasAction.IsPressed(),
            BrakeInput = _brakeAction.IsPressed(),
            NitroInput = _nitroAction.IsPressed(),
            HandbrakeInput = _handbrakeAction.IsPressed(),
            SteerInput = _steerAction.ReadValue<float>(),
            ShiftUpInput = _shiftUpAction.WasPressedThisFrame(),
            ShiftDownInput = _shiftDownAction.WasPressedThisFrame()
        };
    }
}
