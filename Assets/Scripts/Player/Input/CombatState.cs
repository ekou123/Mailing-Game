using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CombatState : State
{
    float gravityValue;
    Vector3 currentVelocity;
    bool grounded;
    float playerSpeed;
    bool sheathWeapon;
    Vector3 cVelocity;

    public CombatState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();

        sheathWeapon = false;
        input = Vector2.zero;
        currentVelocity = Vector3.zero;
        gravityVelocity.y = 0;

        playerSpeed = -character.playerBaseSpeed;
        velocity = character.playerVelocity;
        grounded = character.controller.isGrounded;
        gravityValue = character.gravityValue;
    }

    public override void HandleInput()
    {
        base.HandleInput();

        if (drawWeaponAction.triggered)
        {
            sheathWeapon = true;
        }

        if (attackAction.triggered)
        {
            stateMachine.ChangeState(character.attacking);
            character.photonView.RPC("RPC_ChangeState", RpcTarget.Others, stateMachine.GetCurrentStateName());
        }

        input = moveAction.ReadValue<Vector2>();
        velocity = new Vector3(input.x, 0, input.y);

        velocity = velocity.x * character.cameraTransform.right.normalized + velocity.z * character.cameraTransform.forward.normalized;
        velocity.y = 0f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        character.animator.SetFloat("speed", input.magnitude, character.speedDampTime, Time.deltaTime);

        if (sheathWeapon)
        {
            stateMachine.ChangeState(character.standing);
            character.photonView.RPC("RPC_ChangeState", RpcTarget.Others, stateMachine.GetCurrentStateName());
        }

        
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        gravityVelocity.y += gravityValue * Time.deltaTime;
        grounded = character.controller.isGrounded;

        if (grounded && gravityVelocity.y < 0)
        {
            gravityVelocity.y = 0f;
        }

        // currentVelocity = Vector3.SmoothDamp(currentVelocity, velocity, ref cVelocity, character.velocityDampTime);

        // character.controller.Move((-currentVelocity * playerSpeed + gravityVelocity) * Time.deltaTime);

        Vector3 rbVelocity = character.rb.velocity;
        float yVelocity = rbVelocity.y;

        Vector3 horizontalVelocity = velocity * playerSpeed;
        

        Vector3 finalVelocity = new Vector3(horizontalVelocity.x, yVelocity, horizontalVelocity.z);

        character.rb.velocity = -finalVelocity;

        //Debug.Log(character.rb.velocity);

        Vector3 flattened = new Vector3(finalVelocity.x, 0f, finalVelocity.z);
        

        if (character.photonView.IsMine)
        {
            Vector3 cameraTransform = character.cameraTransform.forward;
            cameraTransform.y = 0f;
            character.transform.forward = cameraTransform;
        }
        
    }

    public override void Exit()
    {
        character.standing.inCombat = false;
    }
}
