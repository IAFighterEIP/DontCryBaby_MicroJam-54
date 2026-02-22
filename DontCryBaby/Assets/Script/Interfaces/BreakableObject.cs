using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BreakableObject : MonoBehaviour, IBreakable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 30f;
    private float currentHealth;
    private bool isBroken;

    [Header("Hit Shake")]
    [SerializeField] private bool shakeOnHit = true;
    [SerializeField] private float shakeDuration = 0.08f;
    [SerializeField] private float shakeIntensity = 0.06f;

    [Header("Juice - Flash (Optional)")]
    [SerializeField] private bool flashOnHit = true;
    [SerializeField] private SpriteRenderer spriteRenderer; // if null, auto-find
    [SerializeField] private float flashDuration = 0.06f;

    [Header("Juice - Scale Punch (Optional)")]
    [SerializeField] private bool punchScaleOnHit = true;
    [SerializeField] private float punchUpScale = 1.08f;
    [SerializeField] private float punchDuration = 0.08f;

    [Header("Freeze Frame (Optional)")]
    [SerializeField] private bool freezeFrameOnBreak = true;
    [SerializeField] private float freezeDuration = 0.04f;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip breakSound;

    [Header("Particles (Optional)")]
    [SerializeField] private ParticleSystem hitParticlesPrefab;
    [SerializeField] private ParticleSystem breakParticlesPrefab;

    [Header("Break Result")]
    [SerializeField] private bool destroyOnBreak = true;
    [SerializeField] private GameObject brokenPrefab;

    private Vector3 originalLocalPos;
    private Vector3 originalLocalScale;

    private Coroutine shakeRoutine;
    private Coroutine flashRoutine;
    private Coroutine punchRoutine;
    
    public float CurrentLife => currentHealth;
    public float MaxLife => maxHealth;

    private AudioSource audioSource;
    private Color originalColor;

    private void Awake()
    {
        currentHealth = maxHealth;

        originalLocalPos = transform.localPosition;
        originalLocalScale = transform.localScale;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(float damage)
    {
        if (isBroken) return;

        damage = Mathf.Max(0f, damage);
        if (damage <= 0f) return;

        currentHealth -= damage;

        // Optional hit particles
        if (hitParticlesPrefab != null)
            Instantiate(hitParticlesPrefab, transform.position, Quaternion.identity);

        // Optional hit sound
        if (hitSound != null)
            audioSource.PlayOneShot(hitSound);

        // Shake based on damage strength
        if (shakeOnHit)
        {
            var damageRatio = Mathf.Clamp01(damage / Mathf.Max(maxHealth, 0.0001f));
            var dynamicIntensity = shakeIntensity * (1f + damageRatio);

            if (shakeRoutine != null) StopCoroutine(shakeRoutine);
            shakeRoutine = StartCoroutine(HitShake(dynamicIntensity));
        }

        // White flash
        if (flashOnHit && spriteRenderer != null)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashWhite());
        }

        // Scale punch
        if (punchScaleOnHit)
        {
            if (punchRoutine != null) StopCoroutine(punchRoutine);
            punchRoutine = StartCoroutine(PunchScale());
        }

        if (currentHealth <= 0f)
            Break();
    }

    private IEnumerator HitShake(float intensity)
    {
        var elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            var offset = (Vector3)Random.insideUnitCircle * intensity;
            transform.localPosition = originalLocalPos + offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalLocalPos;
        shakeRoutine = null;
    }

    private IEnumerator FlashWhite()
    {
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
        flashRoutine = null;
    }

    private IEnumerator PunchScale()
    {
        // quick up
        transform.localScale = originalLocalScale * punchUpScale;

        // small time (uses scaled time, which is fine for hit feedback)
        yield return new WaitForSeconds(punchDuration);

        // return
        transform.localScale = originalLocalScale;
        punchRoutine = null;
    }

    private void Break()
    {
        if (isBroken) return;
        isBroken = true;

        // Run the whole break flow safely (object stays alive until timescale restored)
        StartCoroutine(BreakSequence());
    }

    private IEnumerator BreakSequence()
    {
        // Optional freeze frame (hit-stop)
        if (freezeFrameOnBreak)
        {
            // Prevent stacking freezes from multiple objects breaking same frame
            if (!_isFreezing)
            {
                _isFreezing = true;

                var previousScale = Time.timeScale;
                Time.timeScale = 0f;

                yield return new WaitForSecondsRealtime(freezeDuration);

                Time.timeScale = previousScale <= 0f ? 1f : previousScale;
                _isFreezing = false;
            }
        }

        // Optional break particles
        if (breakParticlesPrefab != null)
            Instantiate(breakParticlesPrefab, transform.position, Quaternion.identity);

        // Optional break sound
        if (breakSound != null)
            audioSource.PlayOneShot(breakSound);

        // Optional broken version spawn
        if (brokenPrefab != null)
            Instantiate(brokenPrefab, transform.position, transform.rotation);

        if (destroyOnBreak)
            Destroy(gameObject);
    }

// Static guard so multiple breakables don’t perma-freeze / spam freeze
    private static bool _isFreezing = false;
}