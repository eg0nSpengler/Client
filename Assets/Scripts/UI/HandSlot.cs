using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HandSlot : UIElement {

    private int id = -1;
    public int ID
    {
        private get => id;
        set { id = id == -1 ? value : id; }
    }

    private Image itemIcon = null;
    private Image enabledIcon = null;
    [SerializeField] private RenderCamera renderCamera = null;

    private Container container = null;
    private CharacterInventory inventory = null;

    public bool needToRedraw = false;

    private new void Start()
    {
        base.Start();
        enabledIcon = transform.Find("EnabledIcon").GetComponent<Image>();
        enabledIcon.color = iconColor;
        itemIcon = transform.Find("ItemIcon").GetComponent<Image>();
    }

    private void Update()
    {
        if (!inventory)
        {
            inventory = FindObjectOfType<CharacterInventory>();
            if (inventory)
            {
                container = inventory.GetHandByID(id);
                container.OnContained = OnAddedToHand + container.OnContained;
            }
            container = inventory ? inventory.GetHandByID(id) : null;
            return;
        }
        RedrawItem();
        enabledIcon.enabled = id == inventory.ActiveHand;
        itemIcon.enabled = image.enabled && container.GetContained().Count > 0;
    }

    private void UseHand()
    {
        if (container.GetContained().Count != 0)
        {
            inventory.UseItemInActiveHand();
        }
    }

    public void ClickButton()
    {
        if (id == inventory.ActiveHand)
        {
            UseHand();
            return;
        }
        else if (container.GetContained().Count != 0)
        {
            if (inventory.GetActiveHand().GetContained().Count == 0)
            {
                inventory.SwapActiveItem(id);
            }
        }
        else
        {
            inventory.ToggleHands();
            return;
        }
    }

    public new void Show()
    {
        base.Show();
        enabledIcon.enabled = id == inventory.ActiveHand;
        itemIcon.enabled = container != null && container.GetContained().Count > 0;
    }

    public new void Hide()
    {
        base.Hide();
        enabledIcon.enabled = false;
        itemIcon.enabled = false;
    }

    private void RedrawItem()
    {
        if (!needToRedraw || container == null || container.GetContained().Count <= 0)
        {
            return;
        }
        Containable obj = container.GetContained()[0];
        if (!obj.generatedIcon)
        {
            Texture2D tex = renderCamera.CamCapture(Instantiate(obj.gameObject));
            obj.generatedIcon = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
        }
        itemIcon.sprite = Instantiate(obj.generatedIcon);
        itemIcon.transform.SetAsLastSibling();
        needToRedraw = false;
    }

    public void OnAddedToHand(Containable containable)
    {
        needToRedraw = true;
    }
}