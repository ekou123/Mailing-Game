using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingState : State
{
    public bool grounded;

    float gravityValue;
    float jumpHeight;
    float playerSpeed;

    private bool hasLeftGround;
    private float timeSinceJump;
    private const float MinAirTime = 0.08f;

    [SerializeField] private LayerMask groundMask = ~0;
    private Collider selfCol;
    private CapsuleCollider capsuleCollider;

    public JumpingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();

        capsuleCollider = character.GetComponent<CapsuleCollider>();

        grounded = false;
        gravityValue = character.gravityValue;
        jumpHeight = character.jumpHeight;
        playerSpeed = character.playerSpeed;
        hasLeftGround = false;
        timeSinceJump = 0f;

        if (moveAction != null) 
        {
            
            moveAction.Enable();
        }
        else
        {
            Debug.LogError("[JumpingState] Move action missing.");
            return;
        }

        if (lookAction != null)
        {
            lookAction.Enable();
        }
        else
        {
            Debug.LogError("[JumpingState] Look action missing.");
            return;
        }

        Jump();
    }

    public override void HandleInput()
    {
        base.HandleInput();
        input = moveAction.ReadValue<Vector2>();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        bool fallingOrSettled = character.rb.velocity.y <= 0.05f;

        if (hasLeftGround && timeSinceJump >= MinAirTime && grounded && fallingOrSettled)
        {
            stateMachine.ChangeState(character.standing);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        timeSinceJump += Time.fixedDeltaTime;

        grounded = IsGrounded();
        if (!grounded) hasLeftGround = true;

        // Build yaw-only basis (so looking up/down doesn't affect movement)
        Vector3 forward = character.transform.forward; forward.y = 0f; forward.Normalize();
        Vector3 right   = character.transform.right;   right.y = 0f;   right.Normalize();

        // Desired movement from live input
        Vector3 inputDir = new Vector3(input.x, 0f, input.y);
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        Vector3 desiredHoriz = (right * inputDir.x + forward * inputDir.z) * playerSpeed;

        // Launch momentum captured at takeoff (already world-space horizontal)
        Vector3 launchHoriz = character.launchHorizontalVelocity;

        // Blend: airControl = 0 keeps launch momentum, 1 gives full steering
        Vector3 horiz = Vector3.Lerp(launchHoriz, desiredHoriz, character.airControl);

        // Apply, keep vertical velocity
        Vector3 v = character.rb.velocity;
        character.rb.velocity = new Vector3(horiz.x, v.y, horiz.z);
    }

    public override void Exit()
    {
        base.Exit();
    }

    void Jump()
    {
        // Reset Y velocity before jumping so double-jumps don't stack
        Vector3 vel = character.rb.velocity;
        vel.y = 0f;
        character.rb.velocity = vel;

        float jumpForce = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
        character.rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

    bool IsGrounded()
    {
        if (capsuleCollider == null) return false;

        float radius = Mathf.Max(0.05f, capsuleCollider.radius * 0.95f);
        float extra = 0.08f; // how forgiving the grounded check is

        // World-space center of capsule
        Vector3 center = character.transform.TransformPoint(capsuleCollider.center);

        // Distance from center to bottom of capsule
        float bottomOffset = (capsuleCollider.height * 0.5f) - capsuleCollider.radius;

        // Start slightly above the bottom so we don't begin inside the ground
        Vector3 origin = center + Vector3.up * 0.02f;

        // Cast down to just below the bottom
        float castDist = bottomOffset + extra;

        // Ignore triggers
        return Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, castDist, ~0, QueryTriggerInteraction.Ignore)
            && hit.collider.gameObject != character.gameObject;
    }
}