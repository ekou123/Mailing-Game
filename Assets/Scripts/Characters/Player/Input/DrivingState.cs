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

    public DrivingState(Character _character, StateMachine _stateMachine)
        : base(_character, _stateMachine) { }

    public void SetVehicle(Vehicle v) => vehicle = v;

    public override void Enter()
    {
        base.Enter();

        // // Grab vehicle action map
        var actions = character.playerInput.actions;
        accelerateAction  = actions["Accelerate"];
        steerAction       = actions["Steer"];
        handbrakeAction   = actions["Handbrake"];
        exitVehicleAction = actions["ExitVehicle"];

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
        accelerateAction?.Disable();
        steerAction?.Disable();
        handbrakeAction?.Disable();
        exitVehicleAction?.Disable();
    }
}