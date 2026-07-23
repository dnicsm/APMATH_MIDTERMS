using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EnemyType
{
    SmokeBomber,
    Saboteur,
    ShadowNinja,
    EchoShinobi
}

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Classification")]
    public EnemyType enemyType;

    [Header("Base Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float speed = 0.5f;
    public float damageToPlayer = 3f;

    [Header("Stealth & Visibility Settings")]
    public bool isInvisible = false;
    public bool isLanternRevealed = false;

    [Header("Saboteur Aura Setup")]
    public float saboteurRadius = 4.0f;
    public LayerMask towerLayer;

    [Header("Echo Shinobi Setup")]
    public float cloneInterval = 5.0f;
    public bool isClone = false;

    private GameObject[] pathPoints;
    private Vector3 p0, p1, p2, p3;
    private float t = 0f;
    private int nextControlIndex = 3;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private bool hasTriggeredSmoke = false;
    private float baseSpeed;

    public bool canMove = true;
    public float currentSlowFactor = 1f;

    // References
    private EnemyHealthManager healthManager;
    private EnemySFX sfx;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        // Automatically hook into Health and SFX scripts
        healthManager = GetComponent<EnemyHealthManager>();
        sfx = GetComponent<EnemySFX>();

        currentHealth = maxHealth;
        baseSpeed = speed;

        InitializePath();
        InitializeEnemyType();
    }

    private void InitializeEnemyType()
    {
        switch (enemyType)
        {
            case EnemyType.ShadowNinja:
                isInvisible = true;
                if (healthManager != null) healthManager.isInvisible = true;
                UpdateVisualOverlay();
                break;

            case EnemyType.EchoShinobi:
                if (!isClone)
                {
                    StartCoroutine(EchoCloneRoutine());
                }
                else
                {
                    currentHealth = maxHealth * 0.4f;
                    if (spriteRenderer) spriteRenderer.color = new Color(0.4f, 0.7f, 1f, 0.6f);
                }
                break;

            case EnemyType.Saboteur:
                if (spriteRenderer) spriteRenderer.color = new Color(0.8f, 0.3f, 1f, 1f); // Purple tint aura
                CreateRedAura();
                break;

            case EnemyType.SmokeBomber:
                break;
        }
    }

    #region Aura
    private void CreateRedAura()
    {
        GameObject auraObj = new GameObject("SaboteurRedAura");
        auraObj.transform.SetParent(transform);
        auraObj.transform.localPosition = Vector3.zero;

        SpriteRenderer auraSR = auraObj.AddComponent<SpriteRenderer>();

        auraSR.sprite = GenerateCircleSprite();

        auraSR.color = new Color(1f, 0f, 0f, 0.3f);

        auraSR.sortingOrder = -1;

        float diameter = saboteurRadius * 2f;
        auraObj.transform.localScale = new Vector3(diameter, diameter, 1f);
    }

    private Sprite GenerateCircleSprite()
    {
        int resolution = 128;
        Texture2D tex = new Texture2D(resolution, resolution);
        Color[] colors = new Color[resolution * resolution];
        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float radius = resolution / 2f;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                {

                    colors[y * resolution + x] = Color.white;
                }
                else
                {

                    colors[y * resolution + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(colors);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);
    }

    #endregion

    void Update()
    {
        if (!canMove) return;

        t += Time.deltaTime * speed * currentSlowFactor;
        t = Mathf.Clamp01(t);
        transform.position = CubicFast(p0, p1, p2, p3, t);

        if (t >= 1f)
        {
            AdvancePath();
        }

        switch (enemyType)
        {
            case EnemyType.SmokeBomber:
                CheckSmokeBombTrigger();
                UpdateVisualOverlay();
                break;

            case EnemyType.Saboteur:
                ExecuteSaboteurAura();
                break;

            case EnemyType.ShadowNinja:
                UpdateVisualOverlay();
                break;
        }
    }

    #region Enemy Behaviors

    private void CheckSmokeBombTrigger()
    {
        // Read actual health from HealthManager if attached, otherwise use local health
        float currentHp = (healthManager != null) ? healthManager.MaxHealth - (healthManager.MaxHealth - GetCurrentHealth()) : currentHealth;
        float maxHp = (healthManager != null) ? healthManager.MaxHealth : maxHealth;

        if (!hasTriggeredSmoke && (currentHp / maxHp) < 0.5f)
        {
            hasTriggeredSmoke = true;
            StartCoroutine(ActivateSmokeBomb());
        }
    }

    private float GetCurrentHealth()
    {
        // Access HealthManager's health safely
        if (healthManager != null)
        {
            return (float)typeof(EnemyHealthManager)
                .GetField("CurrentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(healthManager);
        }
        return currentHealth;
    }

    private IEnumerator ActivateSmokeBomb()
    {
        isInvisible = true;
        if (healthManager != null) healthManager.isInvisible = true; // Sync with HealthManager

        speed = baseSpeed * 1.8f; // Speed boost while stealthed

        if (sfx != null) sfx.SmokeSFX(); // Play smoke bomb sound effect

        yield return new WaitForSeconds(3.0f); // Stealth duration

        isInvisible = false;
        if (healthManager != null) healthManager.isInvisible = false;

        speed = baseSpeed;
    }

    private void ExecuteSaboteurAura()
    {
        Collider2D[] towersInRange = Physics2D.OverlapCircleAll(transform.position, saboteurRadius, towerLayer);
        
        foreach (Collider2D col in towersInRange)
        {
            TowerBehaviors tower = col.GetComponent<TowerBehaviors>();
            if (tower != null)
            {
                tower.SendMessage("ApplySaboteurDebuff", 0.10f, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private void UpdateVisualOverlay()
    {
        if (spriteRenderer == null) return;

        // Sync lantern reveal status from HealthManager if active
        if (healthManager != null)
        {
            isLanternRevealed = healthManager.isLanternRevealed;
        }

        if (isInvisible && !isLanternRevealed)
        {
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.7f); // Semi-transparent smoke overlay
        }
        else if (!isInvisible)
        {
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }
    }

    private IEnumerator EchoCloneRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(cloneInterval);

            GameObject clone = Instantiate(gameObject, transform.position, Quaternion.identity);
            EnemyController cloneScript = clone.GetComponent<EnemyController>();
            
            if (cloneScript != null)
            {
                cloneScript.isClone = true;
                cloneScript.t = this.t;
                cloneScript.nextControlIndex = this.nextControlIndex;
                cloneScript.p0 = this.p0;
                cloneScript.p1 = this.p1;
                cloneScript.p2 = this.p2;
                cloneScript.p3 = this.p3;
            }
        }
    }

    #endregion

#region For towers

public bool IsTargetable()
{
    return !isInvisible || isLanternRevealed;
}

public void RevealInvisible(bool status)
{
    isLanternRevealed = status;
    if (healthManager != null) healthManager.isLanternRevealed = status;
    UpdateVisualOverlay();
}

// --- WIND FAN HANDLERS ---

/// <summary>
/// Pushes back the enemy along the path (reduces t).
/// </summary>
public void ApplyPushback(Vector2 pushForce)
{
    // Push back 't' along the Bezier curve
    float pushbackMagnitude = pushForce.magnitude * 0.05f; 
    t = Mathf.Max(0f, t - pushbackMagnitude);
}

/// <summary>
/// Applies a temporary slow that reverts back after a duration.
/// </summary>
public void ApplySlowWithDuration(object[] args)
{
    float slowFactor = (float)args[0];
    float duration = (float)args[1];
    StartCoroutine(SlowRoutine(slowFactor, duration));
}

private IEnumerator SlowRoutine(float slowFactor, float duration)
{
    currentSlowFactor = slowFactor;
    yield return new WaitForSeconds(duration);
    currentSlowFactor = 1f; // Reset to normal speed
}

public void ApplySlow(float slowFactor)
{
    currentSlowFactor = slowFactor;
}

public void ApplyStun(float duration)
{
    StartCoroutine(StunRoutine(duration));
}

private IEnumerator StunRoutine(float duration)
{
    canMove = false;
    yield return new WaitForSeconds(duration);
    canMove = true;
}

#endregion

    #region Path Logic
    private void InitializePath()
    {
        GameObject pathParent = GameObject.Find("Path Control");
        if (pathParent != null)
        {
            pathPoints = new GameObject[pathParent.transform.childCount];
            for (int i = 0; i < pathPoints.Length; i++)
            {
                Transform child = pathParent.transform.Find($"p{i}");
                if (child != null) pathPoints[i] = child.gameObject;
            }
        }

        if (pathPoints != null && pathPoints.Length >= 4)
        {
            p0 = pathPoints[0].transform.position;
            p1 = pathPoints[1].transform.position;
            p2 = pathPoints[2].transform.position;
            p3 = pathPoints[3].transform.position;
            transform.position = p0;
        }
    }

    private void AdvancePath()
    {
        if (nextControlIndex + 3 >= pathPoints.Length)
        {
            GameObject playerHM = GameObject.FindWithTag("PlayerHealth");
            if (playerHM != null)
            {
                PlayerHealth ph = playerHM.GetComponent<PlayerHealth>();
                if (ph != null) ph.PlayerDamaged(damageToPlayer);
            }
            Destroy(gameObject);
        }
        else
        {
            t = 0f;
            p0 = p3;
            p1 = pathPoints[nextControlIndex + 1].transform.position;
            p2 = pathPoints[nextControlIndex + 2].transform.position;
            p3 = pathPoints[nextControlIndex + 3].transform.position;
            nextControlIndex += 3;
        }
    }

    public static Vector3 CubicFast(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        return u * u * u * p0 + 3f * u * u * t * p1 + 3f * u * t * t * p2 + t * t * t * p3;
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        if (enemyType == EnemyType.Saboteur)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, saboteurRadius);
        }
    }
}