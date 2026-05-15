using UnityEngine;
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

    private void Start()
    {
        if (_interactionPoint == null)
            _interactionPoint = transform;

        playerCharacter = GetComponentInParent<Character>();
        if (playerCharacter == null)
        {
            Debug.LogError("Character component not found on Interactor or any parent");
            return;
        }

        interactAction = playerCharacter.playerInput.actions["Interact"];

        if (interactAction == null)
        {
            Debug.LogWarning("Interact action not found. Check the InputAction asset and action name.");
            return;
        }

        Subscribe();
    }

    private void OnEnable()
    {
        if (_interactionPoint == null)
            _interactionPoint = transform;
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (interactAction == null) return;
        interactAction.performed -= OnInteractPerformed; // prevent double-subscribe
        interactAction.performed += OnInteractPerformed;
        interactAction.Enable();
    }

    private void Unsubscribe()
    {
        if (interactAction != null)
            interactAction.performed -= OnInteractPerformed;
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        LayerMask mask = _interactableMask.value == 0 ? ~0 : _interactableMask;
        _numFound = Physics.OverlapSphereNonAlloc(_interactionPoint.position, _interactionPointRadius, _colliders, mask);

        if (_numFound <= 0)
        {
            Debug.Log("Interact Performed: no interactables found.");
            return;
        }

        for (int i = 0; i < _numFound; i++)
        {
            var interactable = _colliders[i].GetComponentInParent<IInteractable>();
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
        if (_interactionPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_interactionPoint.position, _interactionPointRadius);
    }
}
