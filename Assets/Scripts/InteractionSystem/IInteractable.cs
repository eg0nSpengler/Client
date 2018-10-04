using UnityEngine;

public interface IInteractable
{
    bool IsInteractable(GameObject source);

    void Interact(GameObject source); 
}