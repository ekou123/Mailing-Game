// Something a held object can be released onto (sorting bins, the van, delivery points).
// Needs a collider on the Bin layer so PackageDragger's drop raycast finds it.
public interface IDropTarget
{
    void Receive(IDraggable obj);
}
