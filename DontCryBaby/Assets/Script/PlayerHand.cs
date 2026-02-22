using System;
using UnityEngine;

public class PlayerHands : MonoBehaviour
{
    [SerializeField] private ItemSO heldItem;

    public bool HasItem => heldItem != null;
    public ItemSO HeldItem => heldItem;

    public event Action<ItemSO> OnHeldItemChanged;

    private void SetHeld(ItemSO item)
    {
        heldItem = item;
        OnHeldItemChanged?.Invoke(heldItem);
    }

    public bool TryPick(ItemSO item)
    {
        if (item == null || HasItem) return false;
        SetHeld(item);
        return true;
    }

    public ItemSO Drop()
    {
        ItemSO item = heldItem;
        SetHeld(null);
        return item;
    }

    public bool TryConsume(ItemSO expected)
    {
        if (heldItem == null || heldItem != expected) return false;
        SetHeld(null);
        return true;
    }

    public bool TryReplace(ItemSO newItem)
    {
        if (newItem == null) return false;
        SetHeld(newItem);
        return true;
    }
}