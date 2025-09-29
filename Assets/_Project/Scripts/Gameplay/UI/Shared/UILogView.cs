using UnityEngine;
using TMPro;

public class UILogView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;

    public void SetMessage(string message)
    {
        if (_text == null)
            return;

        _text.enableWordWrapping = true;
        _text.richText = true;
        _text.text = message;
    }
}