using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Containable : MonoBehaviour, IInteractable {
    public Sprite generatedIcon = null;

    [SerializeField] private ContainableSize size = ContainableSize.Normal;
    [SerializeField] private Vector3 pickPosition = Vector3.zero;
    [SerializeField] private Vector3 pickRotation = Vector3.zero;
    [SerializeField] private Vector3 iconRotation = Vector3.zero;

    public Vector3 IconRotation => iconRotation;
    public Vector3 PickPosition => pickPosition;
    public Vector3 PickRotation => pickPosition;

    private Container container;

    public Container Container {
        get => container;
        set {
            if (value != null && container != null) {
                Debug.LogError("Tried to contain an already contained object");
                return;
            }
            container = value;
        }
    }

    public bool IsInteractable(GameObject source) {
        // Check to see if we can remove it from the current container
        if (container != null && container.Restricted) {
            return false;
        }

        var inventory = source.GetComponent<CharacterInventory>();
        return inventory != null && inventory.GetActiveHand().CanContain(this);
    }

    public void Interact(GameObject source) {
        if (!IsInteractable(source)) {
            return;
        }
        var inventory = source.GetComponent<CharacterInventory>();

        container?.Remove(this);

        inventory.GetActiveHand().Contain(this);
    }

    public ContainableSize GetWeight() {
        return size;
    }
}
