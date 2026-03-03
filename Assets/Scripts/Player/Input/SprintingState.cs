using UnityEngine;

public class SprintingState : State
{
    bool sprintHeld;
    bool jumpPressed;

    float playerSpeed;

    public SprintingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();

        playerSpeed = character.sprintSpeed;

        moveAction?.Enable();
        lookAction?.Enable();
        sprintAction?.Enable();
        jumpAction?.Enable();
    }

    public override void HandleInput()
    {
        base.HandleInput();

        // Move input
        Vector2 input2 = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        velocity = new Vector3(input2.x, 0f, input2.y);

        // Sprint is usually "hold to sprint"
        // If you want toggle sprint, change this logic.
        sprintHeld = sprintAction != null && sprintAction.IsPressed();
        jumpPressed = jumpAction != null && jumpAction.WasPressedThisFrame();

        // Look (same as standing)
        if (lookAction != null)
        {
            Vector2 look = lookAction.ReadValue<Vector2>();
            character.yaw   += look.x * character.lookSensitivity;
            character.pitch -= look.y * character.lookSensitivity;
            character.pitch = Mathf.Clamp(character.pitch, character.pitchMin, character.pitchMax);

            character.transform.rotation = Quaternion.Euler(0f, character.yaw, 0f);

            if (character.cameraPivot != null)
                character.cameraPivot.localRotation = Quaternion.Euler(character.pitch, 0f, 0f);
        }

        // Convert velocity to yaw-relative (don’t use pitched camera)
        Vector3 forward = character.transform.forward; forward.y = 0f; forward.Normalize();
        Vector3 right   = character.transform.right;   right.y = 0f;   right.Normalize();

        velocity = velocity.x * right + velocity.z * forward;
        velocity.y = 0f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Stop sprinting if sprint not held or no input
        bool hasInput = velocity.sqrMagnitude > 0.001f;

        if (!sprintHeld || !hasInput)
        {
            stateMachine.ChangeState(character.standing);
            return;
        }

        if (jumpPressed)
        {
            Vector3 v = character.rb.velocity;
            v.y = 0f;
            character.launchHorizontalVelocity = v;

            stateMachine.ChangeState(character.jumping);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // Apply horizontal velocity, keep current vertical velocity
        Vector3 rbVel = character.rb.velocity;
        Vector3 horiz = velocity * playerSpeed;

        character.rb.velocity = new Vector3(horiz.x, rbVel.y, horiz.z);
    }

    public override void Exit()
    {
        base.Exit();
        // Optional: disable actions here, but not strictly required if other states enable them
    }
}