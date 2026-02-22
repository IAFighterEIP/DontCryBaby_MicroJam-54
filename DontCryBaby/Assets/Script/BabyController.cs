using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BabyController : MonoBehaviour
{
    [Header("Anger (time-based)")]
    [SerializeField] private float anger = 0f;
    [SerializeField] private float maxAnger = 100f;
    [SerializeField] private float baseAngerRate = 8f;

    [Header("Anger Multiplier (modified by triggers)")]
    [SerializeField] private float angerMultiplier = 1f;
    [SerializeField] private float minMultiplier = 1f;
    [SerializeField] private float maxMultiplier = 6f;

    [Header("Movement")]
    [SerializeField] private float baseSpeed = 1.5f;
    [SerializeField] private float maxSpeed = 4.5f;

    [Header("Wander")]
    [SerializeField] private float wanderChangeMin = 0.6f;
    [SerializeField] private float wanderChangeMax = 1.4f;
    private Vector2 wanderDir = Vector2.right;
    private float wanderTimer = 0f;

    [Header("Breakables Hunt")]
    [SerializeField] private float calmThreshold = 20f;         // below: wander unless breakable close
    [SerializeField] private float rageThreshold = 50f;         // above: hunt aggressively
    [SerializeField] private float calmSearchRadius = 2.5f;
    [SerializeField] private float rageSearchRadius = 10f;
    [SerializeField] private float retargetCooldown = 0.25f;
    [SerializeField] private float stopDistance = 0.25f;        // how close to “stick” to target
    [SerializeField] private float giveUpDistanceMultiplier = 1.5f; // if target too far away, drop it
    [SerializeField] private LayerMask breakableMask;

    [Header("Damage (anger -> damage)")]
    [SerializeField] private float minDamage = 5f;
    [SerializeField] private float maxDamageAtFullAnger = 20f;
    [SerializeField] private float hitCooldown = 0.12f;

    [Header("Obstacle Avoidance (walls/furniture ONLY)")]
    [SerializeField] private LayerMask obstacleMask;  // DO NOT include breakables here
    [SerializeField] private float avoidDistance = 0.6f;
    [SerializeField] private float avoidStrength = 2.0f;
    [SerializeField] private float sideRayAngle = 35f;
    [SerializeField] private float sideRayScale = 0.9f;

    [Header("Anti-Stuck")]
    [SerializeField] private float stuckSpeedThreshold = 0.15f;
    [SerializeField] private float stuckTimeToRecover = 0.6f;

    [Header("Visuals (ROOT)")]
    [SerializeField] private SpriteRenderer sprite; // auto if null
    [SerializeField] private bool enableAngerTint = true;
    [SerializeField] private Color angryColor = Color.red;

    [SerializeField] private bool enableAngerScale = true;
    [SerializeField] private float maxScaleBonus = 0.25f;

    [Header("Shake (ROOT, no movement disturbance)")]
    [SerializeField] private bool enableShaking = true;
    [SerializeField] private float shakeThreshold = 60f;
    [SerializeField] private float shakeSpeed = 25f;
    [SerializeField] private float maxShakeScaleJitter = 0.04f;

    [Header("Clean scaling (always shrink when anger drops)")]
    [SerializeField] private bool captureCalmScaleOnStart = true;
    [SerializeField] private Vector3 calmScale = Vector3.one;
    [SerializeField] private float scaleSmooth = 18f; // set 0 for instant
    
    
    [Header("Audio - Baby Vocal")]
    [SerializeField] private AudioSource vocalSource; // assign or auto-get

    [Tooltip("Played when anger < happyToCryAnger")]
    [SerializeField] private AudioClip[] happyClips;

    [Tooltip("Played when anger >= happyToCryAnger")]
    [SerializeField] private AudioClip[] cryClips;

    [SerializeField] private float happyToCryAnger = 40f;

    [Header("Audio - Timing")]
    [Tooltip("Happy sounds interval range (seconds).")]
    [SerializeField] private Vector2 happyInterval = new Vector2(2.0f, 4.0f);

    [Tooltip("Crying interval range at LOW cry anger (just above threshold).")]
    [SerializeField] private Vector2 cryIntervalLow = new Vector2(0.8f, 1.2f);

    [Tooltip("Crying interval range at MAX anger (most annoying).")]
    [SerializeField] private Vector2 cryIntervalHigh = new Vector2(0.2f, 0.35f);

    [Tooltip("Random pitch range for variety.")]
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    [Tooltip("Random volume multiplier range for variety.")]
    [SerializeField] private Vector2 volumeRange = new Vector2(0.9f, 1.0f);

    private float nextVocalTime = 0f;
    private int lastHappyIndex = -1;
    private int lastCryIndex = -1;
    private Rigidbody2D rb;

    private Transform breakTarget;
    private float retargetTimer = 0f;
    private float stuckTimer = 0f;

    private float lastDamageTime = -999f;

    private Color baseColor = Color.white;

    // smoothed scale output
    private Vector3 currentScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        if (sprite == null) sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) baseColor = sprite.color;
        else enableAngerTint = false;

        if (captureCalmScaleOnStart)
            calmScale = transform.localScale;

        currentScale = transform.localScale;

        PickNewWanderDir();
        wanderTimer = Random.Range(wanderChangeMin, wanderChangeMax);
        
        if (vocalSource == null)
            vocalSource = GetComponent<AudioSource>();

        // If still null, auto-add one so it "just works"
        if (vocalSource == null)
            vocalSource = gameObject.AddComponent<AudioSource>();

        vocalSource.playOnAwake = false;
        vocalSource.loop = false;

        // Schedule first vocal
        nextVocalTime = Time.time + Random.Range(0.1f, 0.4f);
    }

    private void FixedUpdate()
    {
        // Anger increases only with time
        anger = Mathf.Clamp(anger + baseAngerRate * angerMultiplier * Time.fixedDeltaTime, 0f, maxAnger);
        float anger01 = Mathf.Clamp01(anger / Mathf.Max(maxAnger, 0.0001f));

        // Speed ramp
        float speed = Mathf.Lerp(baseSpeed, maxSpeed, anger01 * anger01);

        // Update wander direction timer
        wanderTimer -= Time.fixedDeltaTime;
        if (wanderTimer <= 0f)
        {
            PickNewWanderDir();
            wanderTimer = Random.Range(wanderChangeMin, wanderChangeMax);
        }

        // Decide search radius based on anger
        float searchRadius =
            (anger >= rageThreshold) ? rageSearchRadius :
            (anger >= calmThreshold)
                ? Mathf.Lerp(calmSearchRadius, rageSearchRadius, Mathf.InverseLerp(calmThreshold, rageThreshold, anger))
                : calmSearchRadius;

        // Give up target if it drifted too far from our “current mood radius”
        if (breakTarget != null)
        {
            float dist = Vector2.Distance(rb.position, breakTarget.position);
            if (dist > searchRadius * giveUpDistanceMultiplier)
                breakTarget = null;
        }

        // Retarget periodically (or if no target)
        retargetTimer -= Time.fixedDeltaTime;
        if (retargetTimer <= 0f)
        {
            retargetTimer = retargetCooldown;

            bool canAcquire =
                (anger >= calmThreshold/*) || (anger < calmThreshold && searchRadius <= calmSearchRadius + 0.001f*/);

            if (breakTarget == null && canAcquire)
                breakTarget = FindClosestBreakable(searchRadius);
        }

        // Movement direction: commit to target if we have one
        Vector2 desiredDir;

        if (breakTarget != null)
        {
            Vector2 toTarget = (Vector2)breakTarget.position - rb.position;
            float dist = toTarget.magnitude;

            desiredDir = dist > 0.001f ? (toTarget / dist) : wanderDir;

            // “Stick” near it so we keep colliding
            if (dist <= stopDistance)
                desiredDir = dist > 0.001f ? (toTarget / dist) : desiredDir;
        }
        else
        {
            desiredDir = wanderDir;
        }

        // Avoid walls/furniture (NOT breakables)
        Vector2 finalDir = AvoidObstacles(desiredDir);

        rb.linearVelocity = finalDir * speed;

        // Anti-stuck
        if (rb.linearVelocity.magnitude < stuckSpeedThreshold)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckTimeToRecover)
            {
                PickNewWanderDir();
                breakTarget = null;
                retargetTimer = 0f;
                stuckTimer = 0f;
            }
        }
        else stuckTimer = 0f;
    }

    private void LateUpdate()
    {
        float anger01 = Mathf.Clamp01(anger / Mathf.Max(maxAnger, 0.0001f));

        if (enableAngerTint && sprite != null)
            sprite.color = Color.Lerp(baseColor, angryColor, anger01);

        float sizeMul = enableAngerScale ? (1f + maxScaleBonus * anger01) : 1f;

        float jitter = 0f;
        if (enableShaking && anger >= shakeThreshold)
            jitter = Mathf.Sin(Time.time * shakeSpeed) * (maxShakeScaleJitter * anger01);

        Vector3 targetScale = calmScale * (sizeMul * (1f + jitter));

        if (scaleSmooth <= 0f)
        {
            currentScale = targetScale;
        }
        else
        {
            currentScale = Vector3.Lerp(currentScale, targetScale, scaleSmooth * Time.deltaTime);
        }

        transform.localScale = currentScale;
        
        UpdateVocalSounds();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Keep damaging while pushing it
        if (Time.time - lastDamageTime < hitCooldown) return;

        BreakableObject breakable = collision.collider.GetComponent<BreakableObject>();
        if (breakable == null) return;

        float angerRatio = Mathf.Clamp01(anger / Mathf.Max(maxAnger, 0.0001f));
        angerRatio *= angerRatio; // strong curve
        float damage = Mathf.Lerp(minDamage, maxDamageAtFullAnger, angerRatio);

        breakable.TakeDamage(damage);
        lastDamageTime = Time.time;

        // Commit: if we hit a breakable, make it our target
        if (breakTarget == null)
            breakTarget = breakable.transform;
    }

    // Multiplier control
    public void IncreaseAngerMultiplier(float amount)
        => angerMultiplier = Mathf.Clamp(angerMultiplier + Mathf.Abs(amount), minMultiplier, maxMultiplier);

    public void DecreaseAngerMultiplier(float amount)
        => angerMultiplier = Mathf.Clamp(angerMultiplier - Mathf.Abs(amount), minMultiplier, maxMultiplier);

    public void SetAngerMultiplier(float value)
        => angerMultiplier = Mathf.Clamp(value, minMultiplier, maxMultiplier);

    // ONLY method allowed to reduce anger directly
    public void CalmBaby(float amount)
    {
        Debug.Log("---- CalmBaby called ----");

        Debug.Log("Anger BEFORE: " + anger);
        Debug.Log("Amount received: " + amount);

        float absoluteAmount = Mathf.Abs(amount);
        Debug.Log("Absolute amount: " + absoluteAmount);

        float newAnger = anger - absoluteAmount;
        Debug.Log("After subtraction (before clamp): " + newAnger);

        float clampedAnger = Mathf.Clamp(newAnger, 0f, maxAnger);
        Debug.Log("After clamp: " + clampedAnger);

        anger = clampedAnger;
        
        breakTarget = null;

        Debug.Log("Anger AFTER: " + anger);
        Debug.Log("--------------------------");
    }

    // -------- Helpers --------
    
    private void UpdateVocalSounds()
    {
        if (vocalSource == null) return;
        if (Time.time < nextVocalTime) return;

        bool isCrying = anger >= happyToCryAnger;

        if (isCrying)
        {
            if (cryClips == null || cryClips.Length == 0)
            {
                nextVocalTime = Time.time + 0.5f;
                return;
            }

            // 0 at threshold, 1 at max anger
            float t = Mathf.InverseLerp(happyToCryAnger, maxAnger, anger);

            // Crying gets faster with anger by blending interval ranges
            float minI = Mathf.Lerp(cryIntervalLow.x, cryIntervalHigh.x, t);
            float maxI = Mathf.Lerp(cryIntervalLow.y, cryIntervalHigh.y, t);

            PlayRandomFrom(cryClips, ref lastCryIndex);

            nextVocalTime = Time.time + Random.Range(minI, maxI);
        }
        else
        {
            if (happyClips == null || happyClips.Length == 0)
            {
                nextVocalTime = Time.time + 1.0f;
                return;
            }

            // Happy can also speed up slightly as anger rises (optional)
            float t = Mathf.InverseLerp(0f, happyToCryAnger, anger);
            float minI = Mathf.Lerp(happyInterval.y, happyInterval.x, t); // slightly faster near threshold
            float maxI = Mathf.Lerp(happyInterval.y, happyInterval.x, t);

            PlayRandomFrom(happyClips, ref lastHappyIndex);

            nextVocalTime = Time.time + Random.Range(minI, maxI);
        }
    }

    private void PlayRandomFrom(AudioClip[] clips, ref int lastIndex)
    {
        if (clips == null || clips.Length == 0) return;

        int index = Random.Range(0, clips.Length);

        // Avoid immediate repeat when possible
        if (clips.Length > 1 && index == lastIndex)
            index = (index + 1 + Random.Range(0, clips.Length - 1)) % clips.Length;

        lastIndex = index;

        vocalSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        vocalSource.volume = Mathf.Clamp01(Random.Range(volumeRange.x, volumeRange.y));

        vocalSource.PlayOneShot(clips[index]);
    }

    private Transform FindClosestBreakable(float radius)
    {
        var hits = Physics2D.OverlapCircleAll(rb.position, radius, breakableMask);

        Transform best = null;
        float bestDistSqr = float.PositiveInfinity;

        for (int i = 0; i < hits.Length; i++)
        {
            var tr = hits[i].transform;
            if (tr.GetComponent<BreakableObject>() == null) continue;

            float d = ((Vector2)tr.position - rb.position).sqrMagnitude;
            if (d < bestDistSqr)
            {
                bestDistSqr = d;
                best = tr;
            }
        }

        return best;
    }

    private Vector2 AvoidObstacles(Vector2 desiredDir)
    {
        if (desiredDir.sqrMagnitude < 0.0001f)
            return desiredDir;

        desiredDir.Normalize();
        Vector2 origin = rb.position;

        var forwardHit = Physics2D.Raycast(origin, desiredDir, avoidDistance, obstacleMask);

        Vector2 leftDir = Rotate(desiredDir, sideRayAngle);
        Vector2 rightDir = Rotate(desiredDir, -sideRayAngle);

        var leftHit = Physics2D.Raycast(origin, leftDir, avoidDistance * sideRayScale, obstacleMask);
        var rightHit = Physics2D.Raycast(origin, rightDir, avoidDistance * sideRayScale, obstacleMask);

        if (!forwardHit && !leftHit && !rightHit)
            return desiredDir;

        Vector2 avoid = Vector2.zero;
        if (forwardHit) avoid += forwardHit.normal;
        if (leftHit) avoid += leftHit.normal * 0.8f;
        if (rightHit) avoid += rightHit.normal * 0.8f;

        if (avoid.sqrMagnitude < 0.0001f)
            avoid = new Vector2(-desiredDir.y, desiredDir.x);

        return (desiredDir + avoid.normalized * avoidStrength).normalized;
    }

    private void PickNewWanderDir()
    {
        wanderDir = Random.insideUnitCircle.normalized;
        if (wanderDir.sqrMagnitude < 0.0001f)
            wanderDir = Vector2.right;
    }

    private static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }
}