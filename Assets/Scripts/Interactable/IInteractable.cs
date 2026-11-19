using UnityEngine;

public interface IInteractable
{
    bool CanInteract { get; }
    float InteractRange { get; }
    Vector3 GetInteractPoint();
    void Interact();
}
