using UnityEngine;
using UnityEngine.InputSystem;

public class DrivingState : State
{
    private Vehicle vehicle;
    private InputAction accelerateAction;
    private InputAction steerAction;
    private InputAction handbrakeAction;
    private InputAction exitVehicleAction;

    private float accelInput;
    private float steerInput;
    private bool  handbrake;

    [Header("Camera")]
    private float camSensitivity = 2f;
    private float camPitchMin    = -20f;
    private float camPitchMax    =  60f;
    private float camYaw;
    private float camPitch;

    public DrivingState(Character _character, StateMachine _stateMachine)
        : base(_character, _stateMachine) { }

    public void SetVehicle(Vehicle v) => vehicle = v;

    public override void Enter()
    {
        base.Enter();

        character.playerInput.SwitchCurrentActionMap("Vehicle");

        character.playerVCam.Priority  = 0;
        character.vehicleVCam.Priority = 10;

        character.vehicleVCam.Follow = vehicle.cameraTarget;
        character.vehicleVCam.LookAt = vehicle.cameraTarget;

        camYaw = vehicle.transform.eulerAngles.y;
        camPitch = 10f;
        vehicle.cameraTarget.rotation = Quaternion.Euler(camPitch, camYaw, 0f);

        // // Grab vehicle action map
        var actions = character.playerInput.actions;
        accelerateAction  = actions["Accelerate"];
        steerAction       = actions["Steer"];
        handbrakeAction   = actions["Handbrake"];
        exitVehicleAction = actions["ExitVehicle"];
        lookAction = character.playerInput.actions.FindActionMap("Walking").FindAction("Look");
        lookAction.Enable();

        accelerateAction.Enable();
        steerAction.Enable();
        handbrakeAction.Enable();
        exitVehicleAction.Enable();

        // Disable character physics while driving
        character.rb.isKinematic = true;

        // Sit the player in the seat
        character.transform.SetParent(vehicle.driverSeat);
        character.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        vehicle.OnDriverEnter(character);
    }

    public override void HandleInput()
    {
        // Don't call base — we don't want character look while driving
        accelInput = accelerateAction.ReadValue<float>();
        steerInput  = steerAction.ReadValue<float>();
        handbrake   = handbrakeAction.IsPressed();

        if (lookAction != null)
        {
            Vector2 look = lookAction.ReadValue<Vector2>();
            Debug.Log("LOok: " + look);

            bool mouseMoving = look.sqrMagnitude > 0.01f;

            if (mouseMoving)
            {
                camYaw   += look.x * camSensitivity;
                camPitch -= look.y * camSensitivity;
                camPitch  = Mathf.Clamp(camPitch, camPitchMin, camPitchMax);
            }
            else if (accelInput > 0.1f)
            {
                // Smoothly snap back behind the vehicle when driving forward
                camYaw = Mathf.LerpAngle(camYaw, vehicle.transform.eulerAngles.y, 2f * Time.deltaTime);
            }

            // Rotate the pivot so Cinemachine follows it
            if (vehicle != null && vehicle.cameraTarget != null)
                vehicle.cameraTarget.rotation = Quaternion.Euler(camPitch, camYaw, 0f);
        }

        // Simple camera-relative look at vehicle forward (optional)
        character.yaw = vehicle.transform.eulerAngles.y;
        character.transform.rotation = Quaternion.Euler(0f, character.yaw, 0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (exitVehicleAction.WasPressedThisFrame())
            ExitVehicle();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (vehicle != null)
            vehicle.ApplyInput(accelInput, steerInput, handbrake);
    }

    private void ExitVehicle()
    {
        vehicle.OnDriverExit();

        // Unparent and move player to exit point
        character.transform.SetParent(null);
        character.transform.position = vehicle.exitPoint.position;

        // Re-enable character physics
        character.rb.isKinematic = false;

        stateMachine.ChangeState(character.standing);
    }

    public override void Exit()
    {
        base.Exit();
        character.playerInput.SwitchCurrentActionMap("Walking");

        accelerateAction?.Disable();
        steerAction?.Disable();
        handbrakeAction?.Disable();
        exitVehicleAction?.Disable();

        character.vehicleVCam.Priority = 0;
        character.playerVCam.Priority  = 10;
    }
}