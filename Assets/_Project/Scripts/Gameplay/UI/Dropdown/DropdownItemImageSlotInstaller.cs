using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropdownItemImageSlotInstaller
{
    private const string TEMPLATE_ITEM_IMAGE_NAME = "Item Image";
    
    private readonly DropdownItemImageLayout layout;

    public DropdownItemImageSlotInstaller(DropdownItemImageLayout layout)
    {
        this.layout = layout;
    }

    public void EnsureItemImageSlot(TMP_Dropdown dropdown, Image prototype)
    {
        if (dropdown == null || dropdown.template == null || prototype == null)
            return;
        
        if (dropdown.itemImage != null)
            return;

        Toggle itemToggle = dropdown.template.GetComponentInChildren<Toggle>(true);
        
        if (itemToggle == null)
            return;
        
        var created = Object.Instantiate(prototype, itemToggle.transform);
        created.name = TEMPLATE_ITEM_IMAGE_NAME;
        created.enabled = false;
        created.sprite = null;

        RectTransform createdRectTransform = created.rectTransform;
        createdRectTransform.anchorMin = layout.AnchorMin;
        createdRectTransform.anchorMax = layout.AnchorMax;
        createdRectTransform.pivot = layout.Pivot;
        createdRectTransform.anchoredPosition = new Vector2(-layout.RightPadding, 0f);
        createdRectTransform.sizeDelta = prototype.rectTransform.sizeDelta;

        dropdown.itemImage = created;
    }
}