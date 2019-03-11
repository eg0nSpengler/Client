using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ContainableSize
{
    Tiny,
    Small,
    Normal,
    Bulky,
    Huge,
    Gigantic
}

[System.Serializable]
public class Container
{
    [SerializeField] private int numSlots = 0;
    [SerializeField] private ContainableSize maxSize = ContainableSize.Normal;
    [SerializeField] private bool restricted; // Can't grab out of this container
    public bool Restricted { get; set; }

    private readonly List<Containable> contained;

    public delegate void ContainerUpdated(Containable containable);

    public ContainerUpdated OnContained { get; set; } = delegate { };
    public ContainerUpdated OnRemoved { get; set; } = delegate { };

    public Container()
    {
        contained = new List<Containable>();
    }

    public Container(int numSlots, ContainableSize maxWeight, bool restricted)
    {
        this.contained = new List<Containable>();
        this.numSlots = numSlots;
        this.maxSize = maxWeight;
        this.restricted = restricted;
    }

    public bool IsFull()
    {
        return contained.Count >= numSlots;
    }

    public bool Fits(Containable containable)
    {
        return containable.GetWeight() <= maxSize;
    }

    public bool CanContain(Containable containable)
    {
        return !IsFull() && Fits(containable);
    }

    public void Contain(Containable containable)
    {
        if (!CanContain(containable))
        {
            Debug.LogError("Tried to contain an uncontainable object");
            return;
        }

        contained.Add(containable);
        containable.Container = this;
        OnContained(containable);
    }

    public void Remove(Containable containable)
    {
        bool successfullyRemoved = contained.Remove(containable);
        if (!successfullyRemoved)
        {
            Debug.LogError("Tried to remove an object that wasn't in a container");
            return;
        }

        containable.Container = null;
        OnRemoved(containable);
    }

    public bool Contains(Containable containable)
    {
        return contained.Contains(containable);
    }

    public IList<Containable> GetContained()
    {
        return contained.AsReadOnly();
    }
}