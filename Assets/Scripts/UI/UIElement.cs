using UnityEngine;
using UnityEngine.UI;

public abstract class UIElement: MonoBehaviour
{
    protected Image image = null;
    protected Image icon = null;
    protected Color buttonColor = new Color(0.16f, 0.17f, 0.2f, 0.8f);
    protected Color iconColor = new Color(0.38f, 0.49f, 0.52f, 0.8f);

    protected void Start()
    {
        icon = transform.Find("IconUI").GetComponent<Image>();
        image = GetComponent<Image>();
        image.color = buttonColor;
        icon.color = iconColor;
    }

    public void Hide()
    {
        image.enabled = false;
        icon.enabled = false;
    }

    public void Show()
    {
        image.enabled = true;
        icon.enabled = true;
    }
}
