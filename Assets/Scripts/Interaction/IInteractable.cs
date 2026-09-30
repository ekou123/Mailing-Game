

public interface IInteractable
{
    public string InteractionPrompt {get;}
    public bool Interact(Interactor interactor);

    void ShowPrompt() { }
    void HidePrompt() { }

}