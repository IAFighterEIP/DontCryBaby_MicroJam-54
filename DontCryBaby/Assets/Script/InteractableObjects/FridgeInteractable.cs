using UnityEngine;

public class FridgeInteractable : ItemInteractable
{
    [Header("Fridge Output")]
    [SerializeField] private ItemSO coldBiberon;
    [SerializeField] private string takeMessage = "You got a cold Baby bottle.";

    public override string GetPrompt(PlayerHands hands)
    {
        if (hands == null) return "";
        return hands.HasItem ? "Hands full" : "Take cold Baby bottle";
    }

    public override void Interact(PlayerHands hands)
    {
        if (hands == null) return;

        if (hands.HasItem)
        {
            Say("Your hands are full.");
            return;
        }

        if (hands.TryPick(coldBiberon))
            Say(takeMessage);
    }
}