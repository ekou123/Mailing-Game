using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PackageDragger : MonoBehaviourPun
{
    public float holdDistance = 1.5f;
    public float reach = 3f;
    public LayerMask packageLayer;
    public LayerMask binLayer;

    private IDraggable heldObject;
    private Camera cam;
    private InputAction grabAction;

    void Awake() => cam = GetComponentInChildren<Camera>();

    void Start()
    {
        // Remote copies of other players must not read this machine's mouse
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            enabled = false;
            return;
        }

        // "Grab" is a Button action in the Walking map, bound to the left mouse button
        grabAction = GetComponentInParent<PlayerInput>().actions.FindAction("Grab");
        if (grabAction == null)
            Debug.LogWarning("Grab action not found. Add it to the Walking map in FirstPersonPlayerControls.");
    }

    void Update()
    {
        if (grabAction == null) return;

        // Let go if it was destroyed or another player took it
        if (heldObject != null && ((heldObject is Object obj && obj == null) || !heldObject.IsHeldByLocalPlayer))
            heldObject = null;

        // The Walking map gets switched off when you get in the van, so the release would never arrive
        if (heldObject != null && !grabAction.enabled)
            TryDrop();

        if (grabAction.WasPressedThisFrame()) TryPickUp();
        if (grabAction.WasReleasedThisFrame()) TryDrop();
        if (heldObject != null) DragObject();
    }

    // The cursor is locked, so aim from the middle of the screen
    Ray AimRay() => cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

    void TryPickUp()
    {
        if (!Physics.Raycast(AimRay(), out RaycastHit hit, reach, packageLayer | binLayer)) return;

        IDraggable draggable = hit.collider.GetComponentInParent<IDraggable>();
        if (draggable != null)
        {
            if (draggable.TryPickUp()) heldObject = draggable;
            return;
        }

        // Clicked a bin or the van with empty hands
        hit.collider.GetComponentInParent<IDragSource>()?.TakeItem();
    }

    void DragObject()
    {
        heldObject.Transform.position = AimRay().GetPoint(holdDistance);
    }

    void TryDrop()
    {
        if (heldObject == null) return;

        // Release before handing it over, since the target may destroy it
        IDraggable dropped = heldObject;
        heldObject = null;
        dropped.OnDrop();

        if (Physics.Raycast(AimRay(), out RaycastHit hit, reach, binLayer))
            hit.collider.GetComponentInParent<IDropTarget>()?.Receive(dropped);
    }
}
