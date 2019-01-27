using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class CharacterInventory : MonoBehaviour {

    [SerializeField] private Transform[] handTransforms;

    private Container[] hands;
    private int activeHand = 0;

    private void Awake() {
        SetupHands();
    }

    private void Update() {
        UpdateHands();
    }

    private void SetupHands() {
        hands = new Container[handTransforms.Length];
        for (int i = 0; i < handTransforms.Length; i++) {
            hands[i] = new Container(1, ContainableSize.Huge, true);
            hands[i].OnContained += OnAddedToHand;
            hands[i].OnRemoved += OnRemovedFromHand;
        }
    }

    private void UpdateHands() {
        if (Input.GetKeyDown(KeyCode.X)) {
            // Cycle hand
            activeHand = (activeHand + 1) % handTransforms.Length;
        }
        if (Input.GetKeyDown(KeyCode.Q)) {
            Containable inhand = GetInhand();
            if (inhand) {
                GetActiveHand().Remove(inhand);
            }
        }
    }

    public Container GetActiveHand() {
        return hands[activeHand];
    }

    public Container[] GetHands() {
        return hands;
    }

    public Containable GetInhand() {
        IList<Containable> contained = GetActiveHand().GetContained();
        if (contained.Count == 0)
            return null;
        return contained[0];
    }

    private void OnAddedToHand(Containable containable)
    {
        var hand = handTransforms[Array.FindIndex(hands, 0, h => h == containable.Container)];
        containable.transform.parent = hand;
        containable.transform.localPosition = Vector3.zero;
        containable.transform.localRotation = Quaternion.identity;
        Rigidbody containableRB = containable.GetComponent<Rigidbody>();
        if (containableRB) {
            containableRB.isKinematic = true;
        }
    }
    
    private void OnRemovedFromHand(Containable containable) {
        containable.transform.parent = null;
        Rigidbody containableRB = containable.GetComponent<Rigidbody>();
        if (containableRB) {
            containableRB.isKinematic = false;
            containableRB.velocity = Vector3.zero;
        }
    }

}
