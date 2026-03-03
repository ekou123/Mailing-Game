using UnityEngine;
using TMPro;

public class SortingBin : MonoBehaviour
{
    public string district;
    public TextMeshPro binLabel;

    void Start()
    {
        if (binLabel != null) binLabel.text = district;
    }

    public void ReceivePackage(Package package)
    {
        bool correct = package.district == district;
        ShiftScoreManager.Instance.RegisterSort(correct);
        Destroy(package.gameObject); // swap for animation later
    }

    public void Receive(IDraggable obj)
    {
        Package package = obj as Package;
        if (package != null)
        {
            bool correct = package.district == district;
            ShiftScoreManager.Instance.RegisterSort(correct);
        }

        UnityEngine.Object.Destroy(obj.Transform.gameObject);
    }
}