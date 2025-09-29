using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class SceneSelectionButtonGate
{
    public void UpdateInteractable(Button loadSceneButton, TMP_Dropdown sceneDropdown, IList<string> sceneKeys)
    {
        if (loadSceneButton == null)
            return;

        bool enabled = sceneDropdown != null && sceneKeys != null && sceneKeys.Count > 0 && sceneDropdown.value >= 0 &&
                       sceneDropdown.value < sceneKeys.Count && !string.IsNullOrWhiteSpace(sceneKeys[sceneDropdown.value]);

        loadSceneButton.interactable = enabled;
    }
}