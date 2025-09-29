using UnityEngine;

public class GameObjectHierarchyLayerSetter
{
    public void SetLayerRecursively(GameObject root, int layer)
    {
        if (root == null)
            return;

        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        
        for (int i = 0; i < transforms.Length; i++)
            transforms[i].gameObject.layer = layer;
    }
}