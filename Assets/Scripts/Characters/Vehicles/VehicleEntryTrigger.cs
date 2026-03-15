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

        // Guard — state may have already changed during Enter()
        if (characterInRange == null) return;

        character.movementSM.currentState.interactAction.performed -= OnInteractPressed;
        characterInRange = null;
    }

    private void OnInteractPressed(InputAction.CallbackContext ctx)
    {
        Debug.Log("Interact Pressed on car");
        if (characterInRange == null || vehicle.isOccupied) return;

        // Unsubscribe BEFORE we change state / disable colliders
        characterInRange.movementSM.currentState.interactAction.performed -= OnInteractPressed;

        characterInRange.GetComponent<CapsuleCollider>().enabled = false;
        characterInRange.driving.SetVehicle(vehicle);
        characterInRange.movementSM.ChangeState(characterInRange.driving);

        // Clear ref so OnTriggerExit (which will fire when colliders disable) is a no-op
        characterInRange = null;
    }
}