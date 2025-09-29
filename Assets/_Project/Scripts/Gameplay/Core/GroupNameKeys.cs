public static class GroupNameKeys
{
    public const string KEY_GROUP_CHARACTERS = "characters/";
    public const string KEY_GROUP_CHARACTER = "character/";
    public const string KEY_GROUP_UI = "ui/";
    public const string KEY_GROUP_BUILDINGS = "buildings/";
    public const string KEY_GROUP_EFFECTS = "effects/";
    public const string KEY_GROUP_SCENES = "scenes/";

    public static string GetPrefix(GroupNameKeyType groupType)
    {
        switch (groupType)
        {
            case GroupNameKeyType.Characters:
                return KEY_GROUP_CHARACTERS;
            
            case GroupNameKeyType.Character:
                return KEY_GROUP_CHARACTER;
            
            case GroupNameKeyType.UI:
                return KEY_GROUP_UI;
            
            case GroupNameKeyType.Buildings:
                return KEY_GROUP_BUILDINGS;
            
            case GroupNameKeyType.Effects:
                return KEY_GROUP_EFFECTS;
            
            case GroupNameKeyType.Scenes:
                return KEY_GROUP_SCENES;
            
            default:
                return string.Empty;
        }
    }
}