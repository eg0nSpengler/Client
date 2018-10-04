using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[RequireComponent(typeof(CharacterIK))]
public class CharacterInventory : MonoBehaviour {

    private CharacterIK characterIK;

    [SerializeField] private int numHands = 2;

    private Container[] hands;
    private int activeHand = 0;

    private void Awake() {
        characterIK = GetComponent<CharacterIK>();
        SetupHands();
    }

    private void Update() {
        UpdateHands();
    }

    private void SetupHands() {
        hands = new Container[numHands];
        for (int i = 0; i < numHands; i++) {
            hands[i] = new Container(1, ContainableSize.Huge, true);
            hands[i].OnContained += OnAddedToHand;
            hands[i].OnRemoved += OnRemovedFromHand;
        }
    }

    private void UpdateHands() {
        if (Input.GetKeyDown(KeyCode.X)) {
            // Cycle hand
            activeHand = (activeHand + 1) % numHands;
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

    private void OnAddedToHand(Containable containable) {
        int handIndex = Array.FindIndex(hands, hand => (hand == containable.Container));
        containable.transform.parent = characterIK.GetItemHoldTransform(handIndex);
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
