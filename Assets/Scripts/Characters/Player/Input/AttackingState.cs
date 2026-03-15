using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using TMPro;
using UnityEngine;

public class AttackingState : State
{
    float timePassed;
    float clipLength;
    float clipSpeed;
    public bool canUseNextAttack;

    float timeSinceLastAttack;

    float gravityValue;
    public bool inCombat;
    Vector3 currentVelocity;
    bool grounded;
    float playerSpeed;

    Vector3 cVelocity;

    public AttackingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();

        canUseNextAttack = false;
        character.animator.applyRootMotion = true;
        timePassed = 0f;
        input = Vector2.zero;
        velocity = Vector3.zero;
        currentVelocity = Vector3.zero;
        gravityVelocity.y = 0;
        canUseNextAttack = false;

        playerSpeed = character.playerBaseSpeed;
        gravityValue = character.gravityValue;

        
        
            // int currentComboIndex = character.animator.GetInteger("comboIndex");
            // currentComboIndex = (currentComboIndex + 1) % 3;
            // character.animator.SetInteger("comboIndex", currentComboIndex);


        character.animator.SetFloat("speed", 0f);
        character.animator.SetInteger("comboIndex", 0);
        
        character.animator.SetFloat("minNormalizedTime", 0); 
        
        //character.animator.SetTrigger("attack");
        character.animator.SetBool("isAttacking", true);
       
        //character.animator.SetTrigger("attack");  
        //character.photonView.RPC("RPC_SetTrigger", RpcTarget.Others, "attack", PhotonNetwork.LocalPlayer.ActorNumber);
        //character.photonView.RPC("RPC_PlayAnimation", RpcTarget.Others, character.animator.GetCurrentAnimatorStateInfo(1).fullPathHash, character.animator.GetCurrentAnimatorStateInfo(1).normalizedTime);
        //canUseNextAttack = true;     
        //character.photonView.RPC("RPC_PlayAnimation", RpcTarget.Others, character.animator.GetCurrentAnimatorStateInfo(1).fullPathHash, character.animator.GetCurrentAnimatorStateInfo(1).normalizedTime);
    }
        
        

    public override void HandleInput()
    {
        base.HandleInput();

        timeSinceLastAttack += Time.deltaTime;
        character.animator.SetFloat("minNormalizedTime", timeSinceLastAttack);

        var clipInfo = character.animator.GetCurrentAnimatorClipInfo(1);
        if (attackAction.triggered && canUseNextAttack)
        {  
            character.animator.SetTrigger("attack");    
            character.photonView.RPC("RPC_SetTrigger", RpcTarget.Others, "attack", PhotonNetwork.LocalPlayer.ActorNumber);
            canUseNextAttack = false;
            //character.photonView.RPC("RPC_PlayAnimation", RpcTarget.Others, character.animator.GetCurrentAnimatorStateInfo(1).fullPathHash, character.animator.GetCurrentAnimatorStateInfo(1).normalizedTime);
            
            timeSinceLastAttack = 0f;
            timePassed = 0f;
            
            

            
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        timePassed += Time.deltaTime;
        var clipInfo = character.animator.GetCurrentAnimatorClipInfo(1);
        var stateInfo = character.animator.GetCurrentAnimatorStateInfo(1);
        if (clipInfo.Length > 0)
        {
            clipLength = clipInfo[0].clip.length;
        }

        clipSpeed = character.animator.GetCurrentAnimatorStateInfo(1).speed;

        // if (stateInfo.normalizedTime >= 1f && !character.animator.IsInTransition(2))
        // {
        //     character.animator.SetTrigger("move");
        //     stateMachine.ChangeState(character.combatting);
            
        // }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        gravityVelocity.y += gravityValue * Time.deltaTime;

        
        if (grounded)
        {
            gravityVelocity.y = 0f;
        }

        //currentVelocity = Vector3.SmoothDamp(currentVelocity, velocity, ref cVelocity, character.velocityDampTime);

        // character.controller.Move((currentVelocity * playerSpeed + gravityVelocity) * Time.deltaTime);
    }

    public override void Exit()
    {
        character.animator.applyRootMotion = false;
    }
}
