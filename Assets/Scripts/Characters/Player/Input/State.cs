using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Photon.Pun.Demo.PunBasics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class State
{
    public Character character;
    public StateMachine stateMachine;
    bool inventoryOpen;
    bool lootMenuOpen;

    protected Vector3 gravityVelocity;
    protected Vector3 velocity;
    protected Vector2 input;

    public InputAction interactAction;
    public InputAction interactUIAction;
    public InputAction moveAction;
    public InputAction lookAction;
    public InputAction jumpAction;
    public InputAction sprintAction;
    public InputAction crouchAction;
    public InputAction drawWeaponAction;
    public InputAction attackAction;
    public InputAction inventoryAction;
    public InputAction switchItemAction;
    public InputAction dropItemAction;
    public InputAction selectUIAction;
    public InputAction openClassSkillsUIAction;
    public InputAction exitUIAction;

    

    public State(Character _character, StateMachine _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;

        moveAction = character.playerInput.actions["Move"];
        lookAction = character.playerInput.actions["Look"];
        jumpAction = character.playerInput.actions["Jump"];
        sprintAction = character.playerInput.actions["Sprint"];
        // crouchAction = character.playerInput.actions["Crouch"];
        // drawWeaponAction = character.playerInput.actions["DrawWeapon"];
        // attackAction = character.playerInput.actions["Attack1"];
        // inventoryAction = character.playerInput.actions["Inventory"];
        interactAction = character.playerInput.actions["Interact"];
        // interactUIAction = character.playerInput.actions["InteractUI"];
        // dropItemAction = character.playerInput.actions["DropItem"];
        // selectUIAction = character.playerInput.actions["SelectUI"];
        // exitUIAction = character.playerInput.actions["ExitUI"];
        // openClassSkillsUIAction = character.playerInput.actions["OpenClassSkills"];
        //SetupSlotBinds();
    }

    public virtual void Enter()
    {
        Debug.Log("Enter state: " + this.ToString());
    }

    private void OnEnable() 
    {
        //switchItemAction
    }

    protected void ApplyLook()
    {
        if (lookAction == null) return;

        Vector2 look = lookAction.ReadValue<Vector2>();

        character.yaw   += look.x * character.lookSensitivity;
        character.pitch -= look.y * character.lookSensitivity;
        character.pitch = Mathf.Clamp(character.pitch, character.pitchMin, character.pitchMax);

        character.transform.rotation = Quaternion.Euler(0f, character.yaw, 0f);

        if (character.cameraPivot != null)
            character.cameraPivot.localRotation = Quaternion.Euler(character.pitch, 0f, 0f);
    }

    public virtual void HandleInput()
    {
        ApplyLook();
    }

    public virtual void LogicUpdate()
    {

    }

    public virtual void PhysicsUpdate()
    {
        // Vector2 lookDelta = lookAction.ReadValue<Vector2>();

        // Debug.Log("Look delta: " + lookDelta);

        // float currentX = character.cinemachineFreeLook.m_XAxis.Value;
        
        // currentX += lookDelta.x * 120f * Time.deltaTime;
        // character.cinemachineFreeLook.m_XAxis.Value = currentX;

        // float currentY = character.cinemachineFreeLook.m_YAxis.Value;
        // currentY = lookDelta.y * 1f * Time.deltaTime;

        // currentY = Mathf.Clamp01(currentY);
        // character.cinemachineFreeLook.m_YAxis.Value = currentY;
        
        
    }

    public virtual void Exit()
    {

    }
}
