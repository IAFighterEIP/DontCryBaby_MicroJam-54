using UnityEngine;

public class PlayerHands : MonoBehaviour
{
    [SerializeField] private ItemSO heldItem;

    public bool HasItem => heldItem != null;
    public ItemSO HeldItem => heldItem;

    public bool TryPick(ItemSO item)
    {
        if (item == null || HasItem) return false;
        heldItem = item;
        // TODO: update UI icon
        return true;
    }

    public ItemSO Drop()
    {
        ItemSO item = heldItem;
        heldItem = null;
        // TODO: update UI icon
        return item;
    }

    public bool TryConsume(ItemSO expected)
    {
        if (heldItem == null || heldItem != expected) return false;
        heldItem = null;
        // TODO: update UI icon
        return true;
    }

    public bool TryReplace(ItemSO newItem)
    {
        if (newItem == null) return false;
        heldItem = newItem;
        // TODO: update UI icon
        return true;
    }
}