using UnityEngine;

public enum ItemId
{
    None = 0,
    ColdFeedingBottle = 10,
    WarmFeedingBottle = 11
}

[CreateAssetMenu(menuName = "Game/Item")]
public class ItemSO : ScriptableObject
{
    public ItemId id;
    public string displayName;
    public Sprite icon;
}