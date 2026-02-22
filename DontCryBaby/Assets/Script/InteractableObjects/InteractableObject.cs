using TMPro;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField]
    private string objectName;

    [SerializeField]
    private string interactionText;

    [SerializeField]
    private int clicksRequired = 1;

    [SerializeField]
    private float actionCooldown = 0.5f;

    [SerializeField]
    private float eventCooldown = 10f;

    [SerializeField]
    private float triggerBabyCooldown = 10f;
    public bool canInteract = false;
    protected int currentClicks = 0;
    private float angerTimer = 0f;
    private BabyController baby;

    [SerializeField]
    private float triggerMultiplier = 0.1f;

    [SerializeField]
    bool alwaysDisplayed = true;

    [Header("ProgressBar")]
    [SerializeField]
    private Transform progressBarParent; // drag & drop le GameObject ProgressBar

    [SerializeField]
    private SpriteRenderer segmentPrefab; // drag & drop ton sprite carré
    private SpriteRenderer[] segments;

    [Header("ProgressBar Settings")]
    [SerializeField]
    private float progressBarWidth = 1f; // largeur totale de la barre

    [SerializeField]
    private float spacing = 0f; // petit écart entre les segments

    [Header("Dialog")]
    [SerializeField]
    private TextMeshProUGUI dialogText;

    [SerializeField]
    private string defaultMessage = "";

    [Header("Audio")]
    [SerializeField]
    private AudioClip sound;
    private AudioSource audioSource;

    [Header("Shake Settings")]
    [SerializeField]
    private float shakeAmplitude = 0.05f;

    [SerializeField]
    private float shakeSpeed = 25f;

    private Vector3 originalLocalPos;
    private bool isShaking = false;

    [Header("Sprites")]
    [SerializeField]
    private Sprite defaultSprite;

    [SerializeField]
    private Sprite interactableSprite;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && defaultSprite != null)
        {
            spriteRenderer.sprite = defaultSprite;
        }
        CreateProgressBar();
        if (dialogText != null)
            dialogText.text = defaultMessage;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        SetVisualActive(false);
        canInteract = false;

        UpdateSpriteState();

        originalLocalPos = transform.localPosition;
        baby = FindObjectOfType<BabyController>();

        StartCoroutine(InitialSpawnRoutine());
    }

    private System.Collections.IEnumerator InitialSpawnRoutine()
    {
        float delay = Random.Range(10, 40);

        yield return new WaitForSeconds(delay);

        SetVisualActive(true);

        if (sound != null)
            audioSource.PlayOneShot(sound);

        canInteract = true;

        UpdateSpriteState();

        angerTimer = 0f;
    }

    void Update()
    {
        if (canInteract && IsVisible())
        {
            angerTimer += Time.deltaTime;

            if (angerTimer >= triggerBabyCooldown)
            {
                Debug.Log("Baby gets angrier!");
                if (sound != null)
                    audioSource.PlayOneShot(sound);
                angerTimer = 0f;
            }
        }
        HandleShake();
    }

    void HandleShake()
    {
        if (canInteract && IsVisible())
        {
            if (!isShaking)
            {
                originalLocalPos = transform.localPosition;
                isShaking = true;
            }

            float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmplitude;
            float offsetY = Mathf.Cos(Time.time * shakeSpeed) * shakeAmplitude;

            transform.localPosition = originalLocalPos + new Vector3(offsetX, offsetY, 0f);
        }
        else
        {
            if (isShaking)
            {
                transform.localPosition = originalLocalPos;
                isShaking = false;
            }
        }
    }

    void TriggerBaby()
    {
        if (baby != null)
            baby.IncreaseAngerMultiplier(triggerMultiplier);
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

    void UpdateSpriteState()
    {
        if (spriteRenderer == null)
            return;

        if (canInteract)
            spriteRenderer.sprite = interactableSprite;
        else
            spriteRenderer.sprite = defaultSprite;
    }

    private System.Collections.IEnumerator CompleteAndCooldownRoutine()
    {
        canInteract = false;

        UpdateSpriteState();

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

        UpdateSpriteState();
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
        {
            if (alwaysDisplayed)
            {
                sr.enabled = true;
            }
            else
            {
                sr.enabled = state;
            }
        }

        // progress bar
        if (progressBarParent != null)
            progressBarParent.gameObject.SetActive(state);

        // dialog
        if (dialogText != null)
            dialogText.gameObject.SetActive(state);
    }
}
