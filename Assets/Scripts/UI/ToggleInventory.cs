using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class ToggleInventory : UIElement
{
    private bool toggleableHidden = true;

    [SerializeField] private Sprite expand = null;
    [SerializeField] private Sprite contract = null;

    [SerializeField] private GameObject bottomBar = null;
    [SerializeField] private GameObject toggleable = null;

    public void ToggleToggleable()
    {
        if (toggleableHidden)
        {
            EnableToggleable();
        }
        else
        {
            DisableToggleable();
        }
    }

    public void DisableToggleable()
    {
        toggleableHidden = true;
        icon.sprite = expand;
        foreach (UIElement slot in toggleable.GetComponentsInChildren<UIElement>())
        {
            slot.Hide();
        }
    }

    public void EnableToggleable()
    {
        toggleableHidden = false;
        icon.sprite = contract;
        foreach (UIElement slot in toggleable.GetComponentsInChildren<UIElement>())
        {
            slot.Show();
        }
    }

    public void EnableNonToggleable()
    {
        foreach (UIElement slot in bottomBar.GetComponentsInChildren<UIElement>())
        {
            slot.Show();
        }
    }
}
