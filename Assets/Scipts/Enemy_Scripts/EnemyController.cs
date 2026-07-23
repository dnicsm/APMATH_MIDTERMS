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
    public bool isInvisible = true;
    public bool isLanternRevealed = false;
    private float lanternRevealTimer = 0f;

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
    private bool hasTriggeredSmoke = false;
    private float baseSpeed;

    public bool canMove = true;
    public float currentSlowFactor = 1f;

    private EnemyHealthManager healthManager;
    private EnemySFX sfx;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null && spriteRenderer.sortingOrder <= 0)
        {
            spriteRenderer.sortingOrder = 2;
        }

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
                break;

            case EnemyType.EchoShinobi:
                if (!isClone)
                {
                    StartCoroutine(EchoCloneRoutine());
                }
                else
                {
                    currentHealth = maxHealth * 0.4f;
                }
                break;

            case EnemyType.Saboteur:
                CreateRedAura();
                break;
        }

        UpdateVisualOverlay();
    }

    public void PingLanternLight()
    {
        lanternRevealTimer = 0.25f; 
        isLanternRevealed = true;

        if (healthManager != null)
        {
            healthManager.isLanternRevealed = true;
        }
    }

    void Update()
    {
        // --- 1. Path Movement Loop ---
        if (canMove && pathPoints != null && pathPoints.Length >= 4)
        {
            t += Time.deltaTime * (speed * currentSlowFactor);
            transform.position = CubicFast(p0, p1, p2, p3, t);

            if (t >= 1f)
            {
                AdvancePath();
            }
        }

        // --- 2. Stealth & Light Detection ---
        if (lanternRevealTimer > 0f)
        {
            lanternRevealTimer -= Time.deltaTime;
            isLanternRevealed = true;
            if (healthManager != null) healthManager.isLanternRevealed = true;
        }
        else
        {
            isLanternRevealed = false;
            if (healthManager != null) healthManager.isLanternRevealed = false;
        }

        UpdateVisualOverlay();

        // --- 3. Active Enemy Ability Ticks ---
        if (enemyType == EnemyType.Saboteur)
        {
            ExecuteSaboteurAura();
        }
        else if (enemyType == EnemyType.SmokeBomber)
        {
            CheckSmokeBombTrigger();
        }
    }

    public void UpdateVisualOverlay()
    {
        if (spriteRenderer == null) return;

        float alpha = 1f;

        // Ninja remains 100% opaque as long as revealed by Lantern
        if (isInvisible && !isLanternRevealed)
        {
            alpha = 0.3f; // Invisible stealth state
        }
        else if (isClone)
        {
            alpha = 0.6f; // Echo clone state
        }

        spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
    }

    #region Enemy Behaviors

    private void CheckSmokeBombTrigger()
    {
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
        if (healthManager != null) healthManager.isInvisible = true; 

        UpdateVisualOverlay();
        if (healthManager != null) healthManager.UpdateSpriteColor();

        speed = baseSpeed * 1.8f; 
        if (sfx != null) sfx.SmokeSFX(); 

        yield return new WaitForSeconds(3.0f); 

        isInvisible = false;
        if (healthManager != null) healthManager.isInvisible = false;

        UpdateVisualOverlay();
        if (healthManager != null) healthManager.UpdateSpriteColor();

        speed = baseSpeed;
    }

    private void CreateRedAura()
    {
        GameObject auraObj = new GameObject("SaboteurRedAura");
        auraObj.transform.SetParent(transform);
        auraObj.transform.localPosition = Vector3.zero;

        SpriteRenderer auraSR = auraObj.AddComponent<SpriteRenderer>();
        auraSR.sprite = GenerateCircleSprite();
        auraSR.color = new Color(1f, 0f, 0f, 0.35f); 

        if (spriteRenderer != null)
        {
            auraSR.sortingLayerID = spriteRenderer.sortingLayerID;
            auraSR.sortingOrder = spriteRenderer.sortingOrder - 1;
        }
        else
        {
            auraSR.sortingOrder = 1;
        }

        float diameter = saboteurRadius * 2f;
        auraObj.transform.localScale = new Vector3(diameter, diameter, 1f);
    }

    private Sprite GenerateCircleSprite()
    {
        int resolution = 128;
        Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        Color[] colors = new Color[resolution * resolution];
        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float radius = resolution / 2f;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                colors[y * resolution + x] = (dist <= radius) ? Color.white : Color.clear;
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        return Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);
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
        bool invisible = isInvisible || (healthManager != null && healthManager.isInvisible);
        bool revealed = isLanternRevealed || (healthManager != null && healthManager.isLanternRevealed);
        return !invisible || revealed;
    }

    public void RevealInvisible(bool status)
    {
        isLanternRevealed = status;
        if (healthManager != null) healthManager.isLanternRevealed = status;
        UpdateVisualOverlay();
    }

    public void ApplyPushback(Vector2 pushForce)
    {
        float pushbackMagnitude = pushForce.magnitude * 0.05f; 
        t = Mathf.Max(0f, t - pushbackMagnitude);
    }

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
        currentSlowFactor = 1f; 
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