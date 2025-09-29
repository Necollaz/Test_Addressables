using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropdownCaptionOverlay
{
    private readonly Image prototype;
    private readonly string overlayName;
    
    private Image _instance;

    public DropdownCaptionOverlay(Image prototype, string fallbackName)
    {
        this.prototype = prototype;
        overlayName = string.IsNullOrEmpty(prototype?.name) ? fallbackName : prototype.name;
    }

    public void Build(TMP_Dropdown dropdown)
    {
        if (dropdown == null || prototype == null)
            return;
        
        prototype.enabled = false;
        
        var parent = dropdown.captionText != null ? dropdown.captionText.transform.parent : dropdown.transform;

        if (_instance != null)
        {
            Object.Destroy(_instance.gameObject);
            _instance = null;
        }

        _instance = Object.Instantiate(prototype, parent);
        _instance.name = overlayName + "_Caption";
        _instance.enabled = false;
    }

    public void SetActive(bool active)
    {
        if (_instance != null)
            _instance.enabled = active;
    }
}