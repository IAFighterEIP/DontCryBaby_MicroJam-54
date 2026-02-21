using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    [Header("Base Settings")]
    public string objectName;
    public string interactionText;
    public int clicksRequired = 1;
    public float cooldown = 0.5f;

    protected int currentClicks = 0;

    public void Interact()
    {
        currentClicks++;

        if (currentClicks >= clicksRequired)
        {
            CompleteInteraction();
            currentClicks = 0;
        }
    }

    protected void CompleteInteraction(){
        Debug.Log(interactionText);
    }
}
