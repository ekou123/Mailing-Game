public interface IDraggable
{
    void OnPickUp();
    void OnDrop();
    UnityEngine.Transform Transform { get; }
}