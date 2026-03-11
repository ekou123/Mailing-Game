using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleEntryTrigger : MonoBehaviour
{
    public Vehicle vehicle;

    private Character characterInRange;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Character>(out var character)) return;
        characterInRange = character;
        character.movementSM.currentState.interactAction.Enable();
        character.movementSM.currentState.interactAction.performed += OnInteractPressed;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<Character>(out var character)) return;
        character.movementSM.currentState.interactAction.performed -= OnInteractPressed;
        characterInRange = null;
    }

    private void OnInteractPressed(InputAction.CallbackContext ctx)
    {
        Debug.Log("Interact Pressed on car");
        if (characterInRange == null || vehicle.isOccupied) return;
        characterInRange.driving.SetVehicle(vehicle);
        characterInRange.movementSM.ChangeState(characterInRange.driving);
    }
}