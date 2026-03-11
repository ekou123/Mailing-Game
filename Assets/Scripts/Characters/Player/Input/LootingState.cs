using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootingState : State
{
    public LootingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    // Start is called before the first frame update
    public override void Enter()
    {
        Debug.Log("Enter state: " + this.ToString());
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        character.playerInput.actions.Disable();
    }

    private void OnEnable() 
    {
        //switchItemAction
    }

    public override void HandleInput()
    {
        
        if (interactAction.triggered)
        {
            
            
            //character.GetComponent<Inventory>().UpdateInventoryUI();
        }
    }

    public override void LogicUpdate()
    {

    }

    public override void Exit()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        character.playerInput.actions.Enable();
    }
}
