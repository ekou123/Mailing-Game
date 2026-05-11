using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [SerializeField] private Transform _interactionPoint;
    [SerializeField] private float _interactionPointRadius = 0.5f;
    [SerializeField] private LayerMask _interactableMask;

    private readonly Collider[] _colliders = new Collider[3];
    [SerializeField] private int _numFound;

    private InputAction interactAction;
    private Character playerCharacter;

    private void Start()
    {
        playerCharacter = GetComponent<Character>();
        if (playerCharacter == null)
        {
            Debug.LogError("Character component not found on Interactor");
            return;
        }

        interactAction = playerCharacter.playerInput.actions["Interact"];


        if (interactAction != null)
        {
            interactAction.Enable();
            interactAction.performed += OnInteractPerformed;
            interactAction.canceled  += OnInteractPerformed;
        }
    }

    void Update()
    {
        

        
    }

    private void OnInteractPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        _numFound = Physics.OverlapSphereNonAlloc(_interactionPoint.position, _interactionPointRadius, _colliders, _interactableMask);

        if (_numFound > 0)
        {
            var interactable = _colliders[0].GetComponent<IInteractable>();

            if (interactable != null )
            {
                interactable.Interact(this);
            }
        }
        
        Debug.Log("Interact Performed");
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_interactionPoint.position, _interactionPointRadius);
    }
}
