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
    public float triggerBabyCooldown = 10f;
    public bool canInteract = true;
    protected int currentClicks = 0;
    private float angerTimer = 0f;

    [Header("ProgressBar")]
    public Transform progressBarParent; // drag & drop le GameObject ProgressBar
    public SpriteRenderer segmentPrefab; // drag & drop ton sprite carré
    private SpriteRenderer[] segments;

    [Header("ProgressBar Settings")]
    public float progressBarWidth = 1f; // largeur totale de la barre
    public float spacing = 0f; // petit écart entre les segments

    [Header("Dialog")]
    public TextMeshProUGUI dialogText;
    public string defaultMessage = "";

    [Header("Audio")]
    public AudioClip sound;
    private AudioSource audioSource;

    void Start()
    {
        CreateProgressBar();
        if (dialogText != null)
            dialogText.text = defaultMessage;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (!canInteract || !IsVisible())
            return;

        angerTimer += Time.deltaTime;

        if (angerTimer >= triggerBabyCooldown)
        {
            Debug.Log("Baby gets angrier!");
            if (sound != null)
                audioSource.PlayOneShot(sound);
            angerTimer = 0f;
        }
    }

    private bool IsVisible()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            return sr.enabled;

        return false;
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
        angerTimer = 0f;
        UpdateProgressUI();

        if (currentClicks >= clicksRequired)
        {
            StartCoroutine(CompleteAndCooldownRoutine());
        }
        else
        {
            StartCoroutine(ActionCooldownRoutine());
        }
    }

    private System.Collections.IEnumerator CompleteAndCooldownRoutine()
    {
        canInteract = false;

        CompleteInteraction();

        // attendre 2 secondes pour lire le message
        yield return new WaitForSeconds(2f);

        // cacher visuellement l'objet
        SetVisualActive(false);

        // attendre le event cooldown
        yield return new WaitForSeconds(eventCooldown);

        // réafficher
        SetVisualActive(true);

        if (sound != null)
            audioSource.PlayOneShot(sound);

        // reset
        currentClicks = 0;
        UpdateProgressUI();

        if (dialogText != null)
            dialogText.text = defaultMessage;

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

    private void SetVisualActive(bool state)
    {
        // sprite principal
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = state;

        // progress bar
        if (progressBarParent != null)
            progressBarParent.gameObject.SetActive(state);

        // dialog
        if (dialogText != null)
            dialogText.gameObject.SetActive(state);
    }
}
