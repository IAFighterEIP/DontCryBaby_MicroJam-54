using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Hands UI")]
    [SerializeField] private GameObject handsPanel;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    
    [Header("Default")]
    [SerializeField] private Sprite emptyHandSprite;

    [Header("Prompt UI")]
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Prompt Style")]
    [SerializeField] private string keyLabel = "E"; // change if you want controller icons later

    public void SetHeldItem(ItemSO item)
    {
        if (handsPanel != null)
            handsPanel.SetActive(true); // always visible now

        if (item == null)
        {
            // Show default hand icon
            if (itemIcon != null)
            {
                itemIcon.sprite = emptyHandSprite;
                itemIcon.enabled = emptyHandSprite != null;
            }

            if (itemName != null)
                itemName.text = "Empty";

            return;
        }

        // Show item
        if (itemIcon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = item.icon != null;
        }

        if (itemName != null)
            itemName.text = item.displayName;
    }

    public void SetPrompt(string text)
    {
        if (promptText == null) return;

        if (string.IsNullOrWhiteSpace(text))
        {
            promptText.text = "";
            promptText.gameObject.SetActive(false);
            return;
        }

        promptText.gameObject.SetActive(true);
        promptText.text = $"<color=#FFD54A>[{keyLabel}]</color> {text}";
    }
}