using System.Collections;
using UnityEngine;

public class MicrowaveInteractable : ItemInteractable
{
    [Header("Recipe")]
    [SerializeField] private ItemSO inputCold;
    [SerializeField] private ItemSO outputHarm;

    [Header("Timing")]
    [SerializeField] private float cookTime = 2.0f;

    [Header("Messages")]
    [SerializeField] private string insertMsg = "Microwave started...";
    [SerializeField] private string doneMsg = "Biberon is now harmful!";
    [SerializeField] private string busyMsg = "Microwave is busy.";

    private bool isCooking = false;

    public override string GetPrompt(PlayerHands hands)
    {
        if (isCooking) return "Microwave (busy)";
        if (hands == null) return "";
        if (!hands.HasItem) return "Insert cold biberon";
        return hands.HeldItem == inputCold ? "Put cold biberon" : "Doesn't fit";
    }

    public override void Interact(PlayerHands hands)
    {
        if (hands == null) return;
        if (isCooking)
        {
            Say(busyMsg);
            return;
        }

        if (!hands.HasItem || hands.HeldItem != inputCold)
        {
            Say("You need a cold biberon.");
            return;
        }

        // consume input
        hands.TryConsume(inputCold);

        Say(insertMsg);
        StartCoroutine(CookRoutine(hands));
    }

    private IEnumerator CookRoutine(PlayerHands hands)
    {
        isCooking = true;

        yield return new WaitForSeconds(cookTime);

        // output goes directly to hands (simple + funny), or you can store inside microwave
        if (!hands.HasItem)
        {
            hands.TryPick(outputHarm);
            Say(doneMsg);
        }
        else
        {
            // fallback if hands got filled somehow
            Say("Done, but your hands are full!");
            // You could store output internally and let player take it later.
        }

        isCooking = false;
    }
}