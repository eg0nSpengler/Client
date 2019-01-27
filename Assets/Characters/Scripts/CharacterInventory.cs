using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterIK))]
public class CharacterInventory : MonoBehaviour {



    public Sprite LeftDefault;
    public Sprite LeftMain;
    public Sprite RightDefault;
    public Sprite RightMain;
    public GameObject LeftHand;
    public GameObject RightHand;

    private CharacterIK characterIK;

    [SerializeField] private int numHands = 2;

    private Container[] hands;
    public Container[] Pockets = new Container[2];
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

    public void SetHighlightedLeft()
    {

        if (RightHand.GetComponent<Image>().sprite = RightMain)
        {

            RightHand.GetComponent<Image>().sprite = RightDefault;

        }


        RightHand.GetComponent<Image>().sprite = RightDefault;
        LeftHand.GetComponent<Image>().sprite = LeftMain;

    }

    public void SetHighlightedRight()
    {

        if (LeftHand.GetComponent<Image>().sprite = LeftMain)
        {

            LeftHand.GetComponent<Image>().sprite = LeftDefault;

        }

        LeftHand.GetComponent<Image>().sprite = LeftDefault;
        RightHand.GetComponent<Image>().sprite = RightMain;

    }


        private void UpdateHands() {

        if (RightHand.GetComponent<Image>().sprite == RightMain)
        {

            // Cycle hand
            activeHand = 1;

        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            Container temp = hands[0];
            hands[0] = hands[1];
            hands[1] = temp;
            Debug.Log("Items Swapped");
        }

        if (LeftHand.GetComponent<Image>().sprite == LeftMain)
        {

            // Cycle hand
            activeHand = 0;

        }



        if (Input.GetKeyDown(KeyCode.F)) {
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
        containable.transform.localPosition = (containable as Containable).PickPosition;
        containable.transform.localEulerAngles = (containable as Containable).PickRotation;
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
