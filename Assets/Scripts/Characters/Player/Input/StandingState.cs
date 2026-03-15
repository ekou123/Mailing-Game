using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StandingState : State
{
    float gravityValue;
    bool jump;
    bool crouch;
    public bool inCombat;
    Vector3 currentVelocity;
    bool grounded;
    bool sprint;
    float playerSpeed;

    Vector3 cVelocity;
    private Vector2 lookInput;

    public StandingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();


        jump = false;
        crouch = false;
        sprint = false;
        input = Vector2.zero;
        velocity = Vector3.zero;
        currentVelocity = Vector3.zero;
        gravityVelocity.y = 0;

        playerSpeed = character.playerBaseSpeed;
        // grounded = character.controller.isGrounded;
        gravityValue = character.gravityValue;

        if (moveAction != null)
        {
            moveAction.Enable();
            moveAction.performed += OnMovePerformed;
            moveAction.canceled  += OnMovePerformed;
        }
        else
        {
            Debug.LogError("[StandingState] moveAction is NULL");
        }

        if (lookAction == null) Debug.LogError("[StandingState] Look action missing.");
        else
        {
            lookAction.Enable();
            lookAction.performed += OnLookPerformed;
            lookAction.canceled  += OnLookPerformed;
        }

        if (jumpAction != null)
        {
            jumpAction.Enable();
        }

        if (sprintAction != null)
            sprintAction.Enable();
    }

    private void OnMovePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
    }

    private void OnLookPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    public override void HandleInput()
    {
        base.HandleInput();

        input = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        velocity = new Vector3(input.x, 0, input.y);

        jump = jumpAction != null && jumpAction.WasPressedThisFrame();

        if (lookAction != null)
        {
            Vector2 look = lookAction.ReadValue<Vector2>();

            character.yaw   += look.x * character.lookSensitivity;
            character.pitch -= look.y * character.lookSensitivity;
            character.pitch = Mathf.Clamp(character.pitch, character.pitchMin, character.pitchMax);

            // Yaw rotates the body as before
            character.transform.rotation = Quaternion.Euler(0f, character.yaw, 0f);

            // Pitch rotates the FirstPersonAnchor — Cinemachine reads this
            if (character.firstPersonAnchor != null)
                character.firstPersonAnchor.localRotation = Quaternion.Euler(character.pitch, 0f, 0f);
        }

        Vector3 camForward = character.firstPersonAnchor.forward;
        Vector3 camRight   = character.firstPersonAnchor.right;
        camForward.y = 0f;
        camRight.y   = 0f;

        velocity = velocity.x * camRight.normalized + velocity.z * camForward.normalized;
        velocity.y = 0f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        //character.SetAnimatorSpeed(input.magnitude, character.speedDampTime);

        if (sprintAction != null && sprintAction.IsPressed() && input.sqrMagnitude > 0.001f)
        {
            stateMachine.ChangeState(character.sprinting);
        }
        if (jump)
        {
            Vector3 v = character.rb.velocity;
            v.y = 0f;
            character.launchHorizontalVelocity = v;

            stateMachine.ChangeState(character.jumping);
        }
        if (inCombat)
        {
            stateMachine.ChangeState(character.combatting);
            //character.animator.SetTrigger("attack1");
        }
    }

    // public override void PhysicsUpdate()
    // {
    //     base.PhysicsUpdate();

    //     grounded = character.groundSensor != null && character.groundSensor.IsGrounded;

    //     gravityVelocity.y += gravityValue * Time.deltaTime;

        
    //     if (grounded)
    //     {
    //         gravityVelocity.y = 0f;
    //     }

    //     Vector3 rbVelocity = character.rb.velocity;
    //     float yVelocity = rbVelocity.y;

    //     Vector3 horizontalVelocity = velocity * playerSpeed;
        

    //     Vector3 finalVelocity = new Vector3(horizontalVelocity.x, yVelocity, horizontalVelocity.z);

    //     character.rb.velocity = finalVelocity;


    //     // Vector3 flattened = new Vector3(finalVelocity.x, 0f, finalVelocity.z);
    //     // if (flattened.sqrMagnitude >= 0.01f)
    //     // {
    //     //     character.transform.forward = flattened.normalized;
    //     // }

    //     //currentVelocity = Vector3.SmoothDamp(currentVelocity, velocity, ref cVelocity, character.velocityDampTime);

    //     //character.controller.Move((currentVelocity * playerSpeed + gravityVelocity) * Time.deltaTime);

    //     //grounded = character.controller.isGrounded;


    //     // if (character.photonView.IsMine)
    //     // {
    //     //     if (currentVelocity.sqrMagnitude > 0)
    //     //     {
    //     //         character.transform.forward = currentVelocity.normalized;
    //     //     }
    //     // }
    // }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        grounded = character.groundSensor != null && character.groundSensor.IsGrounded;

        Vector3 rbVelocity = character.rb.velocity;
        float yVelocity = rbVelocity.y;

        if (grounded && yVelocity < 0.1f)
            yVelocity = 0f;

        Vector3 horizontalVelocity = velocity * playerSpeed;
        character.rb.velocity = new Vector3(horizontalVelocity.x, yVelocity, horizontalVelocity.z);
    }

    public override void Exit()
    {
        base.Exit();

        gravityVelocity.y = 0f;
        character.playerVelocity = new Vector3(input.x, 0, input.y);

    }
}
