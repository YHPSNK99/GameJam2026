using UnityEngine;

public enum ItemType
{
    None,
    MaskWater,
    MaskFire
}

[CreateAssetMenu(menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    public ItemType type;
    public Sprite icon;
}
