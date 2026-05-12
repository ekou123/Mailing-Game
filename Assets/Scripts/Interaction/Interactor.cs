using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [SerializeField] private Transform _interactionPoint;
    [SerializeField] private float _interactionPointRadius = 0.5f;
    [SerializeField] private LayerMask _interactableMask = ~0;

    private readonly Collider[] _colliders = new Collider[8];
    [SerializeField] private int _numFound;

    private InputAction interactAction;
    private Character playerCharacter;

    private void OnEnable()
    {
        if (_interactionPoint == null)
            _interactionPoint = transform;
    }

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
        }
        else
        {
            Debug.LogWarning("Interact action not found on player input. Check the InputAction asset and action name.");
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.performed -= OnInteractPerformed;
        }
    }

    void Update()
    {
    }

    private void OnInteractPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (_interactionPoint == null)
            _interactionPoint = transform;

        LayerMask mask = _interactableMask.value == 0 ? ~0 : _interactableMask;
        _numFound = Physics.OverlapSphereNonAlloc(_interactionPoint.position, _interactionPointRadius, _colliders, mask);

        if (_numFound <= 0)
        {
            Debug.Log("Interact Performed: no interactables found.");
            return;
        }

        for (int i = 0; i < _numFound; i++)
        {
            var interactable = _colliders[i].GetComponent<IInteractable>();
            if (interactable != null)
            {
                Debug.Log("Interacting with " + _colliders[i].name);
                interactable.Interact(this);
                return;
            }
        }

        Debug.Log("Interact Performed: hit colliders but no IInteractable present.");
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_interactionPoint.position, _interactionPointRadius);
    }
}
