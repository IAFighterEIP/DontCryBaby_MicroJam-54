using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    [Header("Base Settings")]
    public string objectName;
    public string interactionText;
    public int clicksRequired = 1;
    public float actionCooldown = 0.5f;
    public float eventCooldown = 0.5f;

    public bool canInteract = true;

    protected int currentClicks = 0;

    public void Interact()
    {
        Debug.Log("start interact");
        if (canInteract == false)
            return;

        currentClicks++;

        if (currentClicks >= clicksRequired)
        {
            Debug.Log("interaction finished");
            CompleteInteraction();
            currentClicks = 0;
            StartCoroutine(EventCooldownRoutine());
        }
        else
        {
            Debug.Log("interaction +1");
            StartCoroutine(ActionCooldownRoutine());
        }
    }

    private System.Collections.IEnumerator EventCooldownRoutine()
    {
        canInteract = false;
        yield return new WaitForSeconds(eventCooldown);
        canInteract = true;
    }

    private System.Collections.IEnumerator ActionCooldownRoutine()
    {
        canInteract = false;
        yield return new WaitForSeconds(actionCooldown);
        canInteract = true;
    }

    protected void CompleteInteraction()
    {
        Debug.Log(interactionText);
    }
}
