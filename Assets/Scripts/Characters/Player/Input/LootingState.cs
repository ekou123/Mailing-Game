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
        base.Enter();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        character.playerInput.actions.Disable();
        inventoryAction?.Enable();

        
    }


    private void OnEnable() 
    {
        //switchItemAction
    }

    public override void HandleInput()
    {
        if (inventoryAction != null && inventoryAction.WasPressedThisFrame())
        {
            stateMachine.ChangeState(character.standing);
        }
    }

    public override void LogicUpdate()
    {

    }

    public override void Exit()
    {
        if (character.inventoryUI != null)
            character.inventoryUI.gameObject.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        character.playerInput.actions.Enable();
    }

}
