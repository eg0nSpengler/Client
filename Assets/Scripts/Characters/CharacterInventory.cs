using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Lidgren.Network;
using UnityEngine.UI;

public class CharacterInventory : MonoBehaviour 
{
    [SerializeField] private Sprite leftMain = null;
    [SerializeField] private Sprite rightMain = null;
    [SerializeField] private GameObject leftHand = null;
    [SerializeField] private GameObject rightHand = null;

    [SerializeField] private int numHands = 2;

    private Container[] hands;
    private int activeHand = 0;

    private void Awake() {
        SetupHands();
    }

    private void Update() {
        UpdateHands();
    }

    private void SetupHands() {
        hands = new Container[numHands];
        for (var i = 0; i < numHands; i++) {
            hands[i] = new Container(1, ContainableSize.Huge, true);
            hands[i].OnContained += OnAddedToHand;
            hands[i].OnRemoved += OnRemovedFromHand;
        }
    }

    private void UpdateHands() 
    {
        if (leftHand == null || rightHand == null)
        {
            return;
        }

        if (rightHand.GetComponent<Image>().sprite == rightMain)
        {
            activeHand = 1;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            var temp = hands[0];
            hands[0] = hands[1];
            hands[1] = temp;
            Debug.Log("Items Swapped");
        }

        if (leftHand.GetComponent<Image>().sprite == leftMain)
        {
            // Cycle hand
            activeHand = 0;
        }


        if (Input.GetKeyDown(KeyCode.F)) {
            var inHand = GetInHand();
            if (inHand) {
                GetActiveHand().Remove(inHand);
            }
        }
    }

    public Container GetActiveHand() {
        return hands[activeHand];
    }

    public Containable GetInHand() {
        var contained = GetActiveHand().GetContained();
        return contained.Count == 0 ? null : contained[0];
    }

    private void OnAddedToHand(Containable containable) {
        containable.transform.localPosition = containable.PickPosition;
        containable.transform.localEulerAngles = containable.PickRotation;
        var containableRb = containable.GetComponent<Rigidbody>();
        if (containableRb) {
            containableRb.isKinematic = true;
        }

    }
    private void OnRemovedFromHand(Containable containable) {
        containable.transform.parent = null;
        var containableRb = containable.GetComponent<Rigidbody>();
        if (containableRb) {
            containableRb.isKinematic = false;
            containableRb.velocity = Vector3.zero;
        }
    }

}
