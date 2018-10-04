using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterCameraReference))]
public class CharacterInteractor : MonoBehaviour {

    private CharacterCameraReference characterCameraReference;

    private IInteractable hovered;

    [SerializeField] private LayerMask raycastMask;

    private void Awake() {
        characterCameraReference = GetComponent<CharacterCameraReference>();
    }

    private void FixedUpdate() {
        UpdateHoverObject();
    }

    void Update() {
        UpdateInteracts();
    }

    // Updates the object the player is hovering over
    private void UpdateHoverObject() {
        IInteractable newHovered = GetInteractableFromCursor();

        if (newHovered == hovered) return; // Same object

        if (hovered != null) {
            MonoBehaviour hoveredMono = (MonoBehaviour)hovered;
            foreach (IHoverWatcher hoverWatcher in hoveredMono.GetComponentsInChildren<IHoverWatcher>()) {
                hoverWatcher.OnUnhovered();
            }
        }
        if (newHovered != null) {
            MonoBehaviour newHoveredMono = (MonoBehaviour)newHovered;
            foreach (IHoverWatcher hoverWatcher in newHoveredMono.GetComponentsInChildren<IHoverWatcher>()) {
                hoverWatcher.OnHovered();
            }
        }

        hovered = newHovered;
    }

    // Updates to interact with the object the player is hovering over with on click
    private void UpdateInteracts() {
        if (hovered == null) return; // Cursor isn't over an object
        if (!hovered.IsInteractable(this.gameObject)) return; // No longer interactable

        if (Input.GetMouseButtonDown(0)) {
            hovered.Interact(this.gameObject);
        }
    }

    // Raycast from the cursor, returning the first collided with interactable object
    private IInteractable GetInteractableFromCursor() {
        Camera characterCamera = characterCameraReference.GetCamera();
        if (characterCamera == null) return null;
        Ray ray = characterCamera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastMask)) {
            GameObject objectRoot = hit.collider.gameObject;

            // Call from the RigidBody if it exists,
            // In the case that this was a nested collider so we can still reach top level components
            if (hit.rigidbody) {
                objectRoot = hit.rigidbody.gameObject;
            }
            foreach (IInteractable interactable in objectRoot.GetComponents<IInteractable>()) {
                if (interactable.IsInteractable(this.gameObject)) {
                    return interactable;
                }
            }
        }
        return null;
    }
}
