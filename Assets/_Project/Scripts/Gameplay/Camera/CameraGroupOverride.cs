using System;
using UnityEngine;

[Serializable]
public struct CameraGroupOverride
{
    public GroupNameKeyType GroupNameKey;
    public Vector3 CameraPosition;
    public Vector3 CameraEulerAngles;
    
    public CameraGroupOverride(GroupNameKeyType groupNameKey, Vector3 cameraPosition, Vector3 cameraEulerAngles)
    {
        GroupNameKey = groupNameKey;
        CameraPosition = cameraPosition;
        CameraEulerAngles = cameraEulerAngles;
    }

    public bool IsMatch(string key)
    {
        string prefix = GroupNameKeys.GetPrefix(GroupNameKey);
        
        return !string.IsNullOrWhiteSpace(prefix) && !string.IsNullOrWhiteSpace(key)
                                                  && key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }
}