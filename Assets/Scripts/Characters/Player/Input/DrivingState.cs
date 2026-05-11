using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DrivingState : State
{
    [Header("Camera")]
    private float camSensitivity  = 2f;
    private float camPitchMin     = -20f;
    private float camPitchMax     = 60f;
    private float camFollowSpeed = 8f;
    private float camYaw;
    private float camPitch;
    private float camReturnSpeed  = 3f;
    private float camSmoothSpeed  = 10f;
    private float idleReturnDelay = 1f;
    private Vector3 smoothCamPos;
    private float timeSinceMouseMoved;
    private float smoothYaw;
    private float smoothPitch;

    private float accelInput;
    private float steerInput;
    private bool  handbrake;

    
    private Vehicle vehicle;
    private InputAction accelerateAction;
    private InputAction steerAction;
    private InputAction handbrakeAction;
    private InputAction exitVehicleAction;

    public DrivingState(Character _character, StateMachine _stateMachine)
        : base(_character, _stateMachine) { }

    public void SetVehicle(Vehicle v) => vehicle = v;

    public override void Enter()
    {
        base.Enter();

        if (character.cinemachineBrain != null) character.cinemachineBrain.enabled = true;
        character.playerInput.SwitchCurrentActionMap("Vehicle");

        var actions = character.playerInput.actions;
        accelerateAction  = actions["Accelerate"];
        steerAction       = actions["Steer"];
        handbrakeAction   = actions["Handbrake"];
        exitVehicleAction = actions["ExitVehicle"];
        lookAction        = character.playerInput.actions.FindActionMap("Walking").FindAction("Look");
        lookAction.Enable();

        accelerateAction.Enable();
        steerAction.Enable();
        handbrakeAction.Enable();
        exitVehicleAction.Enable();

        Collider[] charCols    = character.GetComponentsInChildren<Collider>();
        Collider[] vehicleCols = vehicle.GetComponentsInChildren<Collider>();

        foreach (var cc in charCols)
        {
            if (cc is CharacterController) continue;
            foreach (var vc in vehicleCols)
            {
                if (vc is WheelCollider) continue;
                Physics.IgnoreCollision(cc, vc, true);
            }
        }

        foreach (var col in charCols)
            col.enabled = false;

        character.GetComponentInChildren<Renderer>().enabled = false;

        character.rb.velocity        = Vector3.zero;
        character.rb.angularVelocity = Vector3.zero;
        character.rb.isKinematic     = true;

        character.transform.position = vehicle.driverSeat.position;

        character.playerVCam.Priority  = 0;
        character.vehicleVCam.Priority = 10;

        character.vehicleVCam.Follow = vehicle.cameraTarget;
        character.vehicleVCam.LookAt = vehicle.cameraTarget;

        camYaw              = vehicle.transform.eulerAngles.y;
        camPitch            = 10f;
        smoothYaw           = camYaw;
        smoothPitch         = camPitch;
        timeSinceMouseMoved = 999f;

        vehicle.cameraTarget.rotation = Quaternion.Euler(camPitch, camYaw, 0f);

        var orbital = character.vehicleVCam.GetCinemachineComponent<Cinemachine.CinemachineOrbitalTransposer>();
        if (orbital != null)
        {
            orbital.m_XAxis.Value = 0f;
            orbital.m_RecenterToTargetHeading.m_enabled = false;
        }

        character.vehicleVCam.PreviousStateIsValid = false;

        vehicle.OnDriverEnter(character);

        smoothCamPos = vehicle.transform.position;
    }

    public override void HandleInput()
    {
        accelInput = accelerateAction.ReadValue<float>();
        steerInput = steerAction.ReadValue<float>();
        handbrake  = handbrakeAction.IsPressed();

        if (lookAction != null)
        {
            Vector2 look = lookAction.ReadValue<Vector2>();
            bool mouseMoving = look.sqrMagnitude > 0.01f;

            if (mouseMoving)
            {
                camYaw   += look.x * camSensitivity;
                camPitch -= look.y * camSensitivity;
                camPitch  = Mathf.Clamp(camPitch, camPitchMin, camPitchMax);
                timeSinceMouseMoved = 0f;
            }
            else
            {
                timeSinceMouseMoved += Time.deltaTime;

                if (timeSinceMouseMoved > idleReturnDelay && Mathf.Abs(accelInput) > 0.1f)
                {
                    float vehicleYaw = vehicle.transform.eulerAngles.y;

                    if (accelInput < 0f)
                        vehicleYaw += 180f;

                    camYaw   = Mathf.LerpAngle(camYaw, vehicleYaw, camReturnSpeed * Time.deltaTime);
                    camPitch = Mathf.Lerp(camPitch, 10f, camReturnSpeed * Time.deltaTime);
                }
            }

            smoothYaw   = Mathf.LerpAngle(smoothYaw, camYaw, camSmoothSpeed * Time.deltaTime);
            smoothPitch = Mathf.Lerp(smoothPitch, camPitch, camSmoothSpeed * Time.deltaTime);

            if (vehicle != null && vehicle.cameraTarget != null)
                vehicle.cameraTarget.rotation = Quaternion.Euler(smoothPitch, smoothYaw, 0f);
        }

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
        {
            // Manually follow the seat every frame instead of parenting
            character.transform.position = vehicle.driverSeat.position;
            vehicle.ApplyInput(accelInput, steerInput, handbrake);

            smoothCamPos = Vector3.Lerp(smoothCamPos, vehicle.transform.position, camFollowSpeed * Time.fixedDeltaTime);
            vehicle.cameraTarget.position = smoothCamPos;
        }
    }

    private void ExitVehicle()
    {
        vehicle.OnDriverExit();

        foreach (var col in character.GetComponentsInChildren<Collider>())
            col.enabled = true;

        // Restore normal collision
        Collider[] charCols    = character.GetComponentsInChildren<Collider>();
        Collider[] vehicleCols = vehicle.GetComponentsInChildren<Collider>();

        foreach (var cc in charCols)
            foreach (var vc in vehicleCols)
                Physics.IgnoreCollision(cc, vc, false);

        character.transform.position = vehicle.exitPoint.position;
        character.rb.isKinematic     = false;

        character.GetComponentInChildren<Renderer>().enabled = true;

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

        if (character.cinemachineBrain != null) character.cinemachineBrain.enabled = false;

        character.GetComponent<CapsuleCollider>().enabled = true;
    }
}