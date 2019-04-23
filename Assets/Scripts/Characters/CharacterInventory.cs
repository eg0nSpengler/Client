using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Lidgren.Network;
using UnityEngine.UI;

public class CharacterInventory : MonoBehaviour
{
    private int numHands = 0;
    private int numSlots = 0;
    public int ActiveHand { get; private set; } = 0;

    private Container[] hands;
    private Container[] slots;

    private void Awake() {
        SetupHands();
        SetupSlots();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.X))
        {
            ToggleHands();
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            DropInHand();
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            UseItemInActiveHand();
        }
    }

    private void SetupHands() {
        foreach (HandSlot hand in FindObjectsOfType<HandSlot>())
        {
            hand.ID = numHands++;
        }
        foreach (SlotItem slot in FindObjectsOfType<SlotItem>())
        {
            slot.ID = numSlots++;
        }
        hands = new Container[numHands];
        for (int i = 0; i < numHands; i++)
        {
            hands[i] = new Container(1, ContainableSize.Huge, true);
            hands[i].OnContained += OnAddedToHand;
            hands[i].OnRemoved += OnRemovedFromHand;
        }
    }

    private void SetupSlots()
    {
        slots = new Container[numSlots];
        for (int i = 0; i < numSlots; i++)
        {
            slots[i] = new Container(1, ContainableSize.Huge, true);
            slots[i].OnContained += OnAddedToSlot;
            slots[i].OnRemoved += OnRemovedFromSlot;
        }
    }

    public void ToggleHands()
    {
        if (++ActiveHand >= numHands)
        {
            ActiveHand = 0;
        }
    }

    public Container GetActiveHand() {
        return hands.Length > 0 ? hands[ActiveHand] : null;
    }

    public Container GetHandByID(int id)
    {
        return hands.Length > id ? hands[id] : null;
    }

    public Container GetSlotByID(int id)
    {
        return slots.Length > id ? slots[id] : null;
    }

    public Containable GetInHand() {
        var contained = GetActiveHand().GetContained();
        return contained.Count == 0 ? null : contained[0];
    }

    private void OnAddedToHand(Containable containable) {
        if (containable.transform.parent == gameObject.transform)
        {
            return;
        }
        containable.transform.SetParent(gameObject.transform, true);
        containable.transform.localPosition = containable.PickPosition;
        containable.transform.localEulerAngles = containable.PickRotation;
        var containableRb = containable.GetComponent<Rigidbody>();
        if (containableRb) {
            containableRb.isKinematic = true;
        }
        containable.gameObject.SetActive(false);
    }

    private void OnRemovedFromHand(Containable containable) { }

    private void OnDroppedFromHand(Containable containable) {
        containable.transform.parent = null;
        containable.transform.position = gameObject.transform.position; //Move to player position.
        var containableRb = containable.GetComponent<Rigidbody>();
        if (containableRb) {
            containableRb.isKinematic = false;
            containableRb.velocity = Vector3.zero;
        }
        containable.gameObject.SetActive(true);
    }

    private void DropInHand()
    {
        Containable inHand = GetInHand();
        if (!inHand)
        {
            return;
        }
        inHand.Container.Remove(inHand);
        OnDroppedFromHand(inHand);
    }

    private void OnAddedToSlot(Containable containable) { }

    private void OnRemovedFromSlot(Containable containable) { }

    public void UseItemInActiveHand() { }

    public void SwapActiveItem(int id)
    {
        Containable item = GetHandByID(id).GetContained()[0];
        item.Container.Remove(item);
        GetActiveHand().Contain(item);
    }

}