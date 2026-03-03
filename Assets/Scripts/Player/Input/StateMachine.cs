using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class StateMachine
{
    public State currentState;
    public Character character;

    [Header("Basic Movement")]
    private Vector2 movement; 

    public void Initialize(State startingState)
    {
        currentState = startingState;
        character = currentState.character;
        startingState.Enter();
    }

    public string GetCurrentStateName()
    {
        return currentState.GetType().Name;
    }
    
    public void ChangeState(State newState)
    {
        currentState.Exit();

        currentState = newState;
        newState.Enter();

        //character.photonView.RPC("RPC_ChangeState", RpcTarget.Others, GetCurrentStateName());
    }

    public void ChangeStateByName(string stateName)
    {
        Debug.Log("State name: " + stateName);
        switch (stateName)
        {
            case "StandingState":
                ChangeState(character.standing);
                break;
            case "JumpingState":
                ChangeState(character.jumping);
                break;
            case "SprintingState":
                ChangeState(character.sprinting);
                break;
            case "SprintJumpState":
                ChangeState(character.sprintJumping);
                break;
            case "CombatState":
                ChangeState(character.combatting);
                break;
            case "AttackingState":
                ChangeState(character.attacking);
                break;
            default:
                ChangeState(character.standing);
                break;
        }
    }
}
