using TMPro;
using UnityEngine;

public abstract class ItemInteractable : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] protected TextMeshProUGUI dialogText;
    [SerializeField] protected string defaultMessage = "";

    protected virtual void Start()
    {
        if (dialogText != null) dialogText.text = defaultMessage;
    }

    // Return a short prompt like "Take cold biberon" / "Insert biberon"
    public abstract string GetPrompt(PlayerHands hands);

    // Called when player presses interact key
    public abstract void Interact(PlayerHands hands);

    protected void Say(string msg)
    {
        if (dialogText != null) dialogText.text = msg;
    }
}