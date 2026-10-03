public interface IDraggable
{
    // Returns false if someone else is already holding it
    bool TryPickUp();
    void OnDrop();

    // Goes false if another player takes it, so the dragger lets go
    bool IsHeldByLocalPlayer { get; }
    UnityEngine.Transform Transform { get; }
}
