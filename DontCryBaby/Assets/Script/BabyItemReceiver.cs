using UnityEngine;

public class BabyItemReceiver : ItemInteractable
{
    [Header("Accept")]
    [SerializeField] private ItemSO harmBiberon;

    [Header("Baby")]
    [SerializeField] private BabyController baby;
    [SerializeField] private float angerMultiplierDecrease = 2.0f;
    [SerializeField] private float angerCalm = 50.0f;

    public override string GetPrompt(PlayerHands hands)
    {
        if (hands == null) return "";
        if (!hands.HasItem) return "Give something";
        return hands.HeldItem == harmBiberon ? "Give harmful biberon" : "Baby doesn't want that";
    }

    public override void Interact(PlayerHands hands)
    {
        if (hands == null) return;

        if (!hands.HasItem || hands.HeldItem != harmBiberon)
        {
            Say("Bring me the harmful biberon!");
            return;
        }

        hands.TryConsume(harmBiberon);

        if (baby == null) baby = FindFirstObjectByType<BabyController>();
        if (baby != null)
            baby.DecreaseAngerMultiplier(angerMultiplierDecrease);
            baby.CalmBaby(angerCalm);

        Say("Baby got the feeding bottle...");
    }
}