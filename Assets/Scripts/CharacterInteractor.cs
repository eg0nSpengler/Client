using System.Collections;
using System.Collections.Generic;
using Brisk.Entities;
using UnityEngine;

public class CharacterInteractor : NetBehaviour 
{
    [SerializeField] private LayerMask raycastMask;

    private new Camera camera;
    private GameObject hovered;

    private void Start()
    {
        camera = Camera.main;
    }

    private void Update()
    {
        if (Peer == null || !Entity.Owner) return;
        
        UpdateHoverObject();
        UpdateInteracts();
    }

    // Updates the object the player is hovering over
    private void UpdateHoverObject() {
        var newHovered = FindHovered();

        if (newHovered == hovered)
        {
            return;
        }

        if (hovered != null) {
            foreach (var hoverWatcher in hovered.GetComponentsInChildren<IHoverWatcher>()) {
                hoverWatcher.OnUnhovered();
            }
        }
        if (newHovered != null) {
            foreach (var hoverWatcher in newHovered.GetComponentsInChildren<IHoverWatcher>()) {
                hoverWatcher.OnHovered();
            }
        }

        hovered = newHovered;
    }

    // Updates to interact with the object the player is hovering over with on click
    private void UpdateInteracts() {
        if (hovered == null)
        {
            return; // Cursor isn't over an object
        }

        var interactable = hovered.GetComponent<IInteractable>();
        if (interactable == null)
        {
            return; // Object isn't interactable
        }

        if (!interactable.IsInteractable(gameObject))
        {
            return; // Not interactable
        }

        if (Input.GetMouseButtonDown(0)) {
            interactable.Interact(this.gameObject);
        }
    }

    // Raycast from the cursor, returning the first collided with interactable object
    private GameObject FindHovered() {
        if (camera == null) return null;
        
        var ray = camera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, Mathf.Infinity, raycastMask))
        {
            return null;
        }

        // Call from the RigidBody if it exists,
        // In the case that this was a nested collider so we can still reach top level components
        return hit.rigidbody ? hit.rigidbody.gameObject : hit.collider.gameObject;

    }
}
