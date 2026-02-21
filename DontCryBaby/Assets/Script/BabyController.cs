using UnityEngine;

public class BabyController : MonoBehaviour
{
    [Header("Anger")]
    [SerializeField] private float anger = 0f;
    [SerializeField] private float maxAnger = 100f;
    [SerializeField] private float baseAngerRate = 3f;   // anger/sec at multiplier=1
    [SerializeField] private float multiplier = 1f;
    [SerializeField] private float maxMultiplier = 3f;

    [Header("Movement")]
    [SerializeField] private float baseSpeed = 2f;
    [SerializeField] private float maxSpeed = 10f;

    [Header("Bounds")]
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;

    [Header("Visuals")]
    [Tooltip("Optional. If empty, the script auto-creates a child named 'Visual' and moves the sprite there.")]
    [SerializeField] private Transform visual;
    [SerializeField] private float minScale = 0.9f;
    [SerializeField] private float maxScale = 1.8f;
    [SerializeField] private Color calmColor = Color.white;
    [SerializeField] private Color angryColor = Color.red;

    [Header("Shake (Visual only)")]
    [SerializeField] private float shakeIntensity = 0.05f;
    [SerializeField] private float shakeSpeed = 25f;

    [Header("Wander")]
    [SerializeField] private float wanderChangeInterval = 1.0f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private Vector2 wanderDir;
    private float wanderTimer;

    private bool isShaking = false;
    private Vector3 visualOriginalLocalPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("[BabyController] Missing Rigidbody2D on Baby.");
            enabled = false;
            return;
        }

        EnsureVisualChild();

        sr = visual.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("[BabyController] Visual has no SpriteRenderer.");
            enabled = false;
            return;
        }

        visualOriginalLocalPos = visual.localPosition;
        PickNewWanderDir();
    }

    private void EnsureVisualChild()
    {
        if (visual != null) return;

        Transform found = transform.Find("Visual");
        if (found != null)
        {
            visual = found;
            return;
        }

        GameObject v = new GameObject("Visual");
        v.transform.SetParent(transform);
        v.transform.localPosition = Vector3.zero;
        v.transform.localRotation = Quaternion.identity;
        v.transform.localScale = Vector3.one;
        visual = v.transform;

        // Copy sprite renderer from parent if any, then disable parent renderer
        SpriteRenderer parentSR = GetComponent<SpriteRenderer>();
        SpriteRenderer childSR = v.AddComponent<SpriteRenderer>();

        if (parentSR != null)
        {
            childSR.sprite = parentSR.sprite;
            childSR.color = parentSR.color;
            childSR.flipX = parentSR.flipX;
            childSR.flipY = parentSR.flipY;
            childSR.sortingLayerID = parentSR.sortingLayerID;
            childSR.sortingOrder = parentSR.sortingOrder;
            childSR.material = parentSR.sharedMaterial;

            parentSR.enabled = false;
        }
        else
        {
            // If you had no SpriteRenderer on the parent, at least show something
            childSR.color = Color.white;
        }
    }

    private void Update()
    {
        // Anger increases over time; external events ONLY affect multiplier (and Calm can reduce anger too)
        anger += baseAngerRate * multiplier * Time.deltaTime;
        anger = Mathf.Clamp(anger, 0f, maxAnger);

        float t = anger / maxAnger;

        // Size + color based on anger
        float scale = Mathf.Lerp(minScale, maxScale, t);
        visual.localScale = new Vector3(scale, scale, 1f);
        sr.color = Color.Lerp(calmColor, angryColor, t);

        // Wander direction refresh
        wanderTimer += Time.deltaTime;
        if (wanderTimer >= wanderChangeInterval)
            PickNewWanderDir();

        // Shake (funny jitter) on Visual ONLY, intensity scales with anger
        if (isShaking)
        {
            float dynamicIntensity = shakeIntensity * Mathf.Lerp(0.2f, 1.0f, t);

            float offsetX = Mathf.Sin(Time.time * shakeSpeed) * dynamicIntensity;
            float offsetY = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * dynamicIntensity; // goofy uneven wobble

            visual.localPosition = visualOriginalLocalPos + new Vector3(offsetX, offsetY, 0f);
        }
        else
        {
            visual.localPosition = visualOriginalLocalPos;
        }

        // Micro-polish: cartoony bounce when very angry
        if (anger > 70f)
        {
            float bounce = 1f + Mathf.Sin(Time.time * 18f) * 0.03f;
            visual.localScale *= bounce;
        }
    }

    private void FixedUpdate()
    {
        float t = anger / maxAnger;

        // Speed ramp: slow early, fast late
        float speedT = t * t;
        float speed = Mathf.Lerp(baseSpeed, maxSpeed, speedT);

        // ============================================================
        // TODO (LATER): Pathfinding to breakable objects
        //
        // When you have breakable objects + a pathfinding system:
        // 1) Find a target breakable (closest, highest priority, etc.)
        //    Example: Transform target = breakableProvider.GetTarget();
        //
        // 2) Compute a path to target (NavMesh, A*, grid, etc.)
        //    Example: Vector2 desiredDir = pathfinder.GetNextDirection(transform.position, target.position);
        //
        // 3) Replace wanderDir with the path direction:
        //    wanderDir = desiredDir.normalized;
        //
        // For now: wander randomly.
        // ============================================================

        rb.linearVelocity = wanderDir * speed;

        // Keep inside bounds; if we hit a wall, bounce by choosing a new direction
        Vector2 p = rb.position;
        bool hitWall = false;

        if (p.x < minX) { p.x = minX; hitWall = true; }
        else if (p.x > maxX) { p.x = maxX; hitWall = true; }

        if (p.y < minY) { p.y = minY; hitWall = true; }
        else if (p.y > maxY) { p.y = maxY; hitWall = true; }

        if (hitWall)
        {
            rb.position = p;
            PickNewWanderDir();
        }
    }

    private void PickNewWanderDir()
    {
        wanderTimer = 0f;
        wanderDir = Random.insideUnitCircle.normalized;
        if (wanderDir.sqrMagnitude < 0.01f)
            wanderDir = Vector2.right;
    }

    // Objects / events call this to make baby escalate faster (adds to multiplier)
    public void AddFlatAnger(float amount)
    {
        multiplier = Mathf.Clamp(multiplier + amount, 1f, maxMultiplier);
    }

    // Calm lowers multiplier
    public void CalmBaby(float amount)
    {
        multiplier = Mathf.Clamp(multiplier - amount, 1f, maxMultiplier);
    }

    // Calm zone can also lower the anger value directly
    public void ReduceAnger(float amount)
    {
        anger = Mathf.Clamp(anger - amount, 0f, maxAnger);
    }

    public void SetShaking(bool value) => isShaking = value;

    // Debug helpers
    public float Anger01 => anger / maxAnger;
    public float Multiplier => multiplier;
}