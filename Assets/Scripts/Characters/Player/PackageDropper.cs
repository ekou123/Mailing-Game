using UnityEngine;

public class PackageDragger : MonoBehaviour
{
    public float holdDistance = 1.5f;
    public LayerMask packageLayer;
    public LayerMask binLayer;

    private IDraggable heldObject;
    private Camera cam;

    void Awake() => cam = GetComponentInChildren<Camera>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TryPickUp();
        if (Input.GetMouseButtonUp(0)) TryDrop();
        if (heldObject != null) DragObject(); // checking heldObject not heldPackage
    }

    void TryPickUp()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f, packageLayer))
        {
            heldObject = hit.collider.GetComponent<IDraggable>();
            heldObject?.OnPickUp();
        }
    }

    void DragObject()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        heldObject.Transform.position = ray.GetPoint(holdDistance);
    }

    void TryDrop()
    {
        if (heldObject == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f, binLayer))
            hit.collider.GetComponent<SortingBin>()?.Receive(heldObject);

        heldObject.OnDrop();
        heldObject = null;
    }
}