using TMPro;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    [Header("Base Settings")]
    public string objectName;
    public string interactionText;
    public int clicksRequired = 1;
    public float actionCooldown = 0.5f;
    public float eventCooldown = 10f;

    public bool canInteract = true;

    protected int currentClicks = 0;

    [Header("ProgressBar")]
    public Transform progressBarParent; // drag & drop le GameObject ProgressBar
    public SpriteRenderer segmentPrefab; // drag & drop ton sprite carré
    private SpriteRenderer[] segments;

    [Header("ProgressBar Settings")]
    public float progressBarWidth = 1f; // largeur totale de la barre
    public float spacing = 0f; // petit écart entre les segments

    [Header("Dialog")]
    public TextMeshProUGUI dialogText; // ou Text si tu utilises l’UI classique
    public string defaultMessage = "";

    void Start()
    {
        CreateProgressBar();
        if (dialogText != null)
            dialogText.text = defaultMessage;
    }

    void CreateProgressBar()
    {
        segments = new SpriteRenderer[clicksRequired];

        float segmentWidth = (progressBarWidth - spacing * (clicksRequired - 1)) / clicksRequired;
        float startX = -progressBarWidth / 2f + segmentWidth / 2f;

        for (int i = 0; i < clicksRequired; i++)
        {
            SpriteRenderer seg = Instantiate(segmentPrefab, progressBarParent);

            // position horizontale
            seg.transform.localPosition = new Vector3(startX + i * (segmentWidth + spacing), 0, 0);

            // scale X pour que le segment prenne sa largeur
            seg.transform.localScale = new Vector3(segmentWidth, 0.2f, 1f);

            seg.color = Color.gray; // couleur vide au départ
            segments[i] = seg;
        }
    }

    public void UpdateProgressUI()
    {
        for (int i = 0; i < segments.Length; i++)
        {
            if (i < currentClicks)
                segments[i].color = Color.green;
            else
                segments[i].color = Color.gray;
        }
    }

    public void Interact()
    {
        if (canInteract == false)
            return;

        currentClicks++;
        UpdateProgressUI();

        if (currentClicks >= clicksRequired)
        {
            CompleteInteraction();
            currentClicks = 0;
            StartCoroutine(EventCooldownRoutine());
            UpdateProgressUI();
        }
        else
        {
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
        if (dialogText != null)
            dialogText.text = interactionText;
    }
}
