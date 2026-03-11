using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStateMachine : MonoBehaviour
{
    public NPCState CurrentNPCState {get; set;}

    public NPCStateMachine StateMachine {get; set;}
    public NPCIdleState NPCIdleState {get; set;}
    public NPCChaseState NPCChaseState {get; set;}
    public NPCAttackState NPCAttackState {get; set;}

     public NPCStateMachine(NPC npc, NPCStateMachine nPCStateMachine)
    {
        this.StateMachine = nPCStateMachine;
        
    }

    public void Initialize(NPCState startingState)
    {
        CurrentNPCState = startingState;
        CurrentNPCState.EnterState();
    }

    public void ChangeState(NPCState newState)
    {
        CurrentNPCState.ExitState();
        CurrentNPCState = newState;
        CurrentNPCState.EnterState();
    }
}
