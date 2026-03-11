using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
public class FirstPersonPlayer : MonoBehaviourPun
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform moveReference;

    private Rigidbody rb;
    private PlayerInput pi;
    [SerializeField] private InputAction walkAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pi = GetComponent<PlayerInput>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        if (!photonView.IsMine)
        {
            pi.enabled = false;   // prevent remote stealing input
            enabled = false;
            return;
        }

        walkAction = pi.actions["Walk"]; // MUST match action name
    }

    private void FixedUpdate()
    {
        Vector2 moveInput = walkAction.ReadValue<Vector2>();

        Transform basis = moveReference != null ? moveReference : transform;
        Vector3 forward = basis.forward; forward.y = 0f; forward.Normalize();
        Vector3 right = basis.right; right.y = 0f; right.Normalize();

        Vector3 desired = (right * moveInput.x + forward * moveInput.y) * moveSpeed;

        // keep current vertical velocity (gravity/jumps)
        Vector3 v = rb.velocity;
        rb.velocity = new Vector3(desired.x, v.y, desired.z);
    }
}