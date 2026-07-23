using UnityEngine;
using System.Collections;

public class EnemyHealthManager : MonoBehaviour
{
    [Header("Health UI")]
    public GameObject GhostBar;
    public GameObject HealthBar;

    [Header("Coins & Rewards")]
    public GameObject coinPrefab;
    [SerializeField] private int coinDrop = 1;

    [Header("Health Settings")]
    public float MaxHealth = 100f;
    [SerializeField] private float CurrentHealth;

    [Header("Movement & Status Factors")]
    public bool canMove = true;
    public float currentSlowFactor = 1f;

    [Header("Stealth & Visibility State")]
    public bool isInvisible = false;
    public bool isLanternRevealed = false;

    private EnemySFX sfx;
    private EnemyController enemyControllerScript;
    private ArashiBoss bossScript;

    private Vector3 ghostBarInitialScale;
    private Vector3 healthBarInitialScale;
    private SpriteRenderer enemySprite;
    private bool enemyDead = false;

    private bool isDamageAmplified = false;
    private float damageAmpMultiplier = 1.35f;
    private bool isPoisoned = false;

    private Coroutine poisonCoroutine;
    private Coroutine stunCoroutine;
    private Coroutine pushbackCoroutine;

    void Start()
    {
        sfx = GetComponent<EnemySFX>();
        if (sfx == null) sfx = GetComponentInChildren<EnemySFX>();

        enemySprite = GetComponent<SpriteRenderer>();

        if (GhostBar != null) ghostBarInitialScale = GhostBar.transform.localScale;
        if (HealthBar != null) healthBarInitialScale = HealthBar.transform.localScale;

        CurrentHealth = MaxHealth;

        enemyControllerScript = GetComponent<EnemyController>();
        bossScript = GetComponent<ArashiBoss>();

        if (enemyControllerScript != null && enemyControllerScript.enemyType == EnemyType.ShadowNinja)
        {
            isInvisible = true;
        }

        UpdateSpriteColor();
    }

    public bool IsTargetable()
    {
        if (enemyDead) return false;
        return !isInvisible || isLanternRevealed;
    }

    #region Health & Damage Management

    public void TakeDamage(float damage)
    {
        if (CurrentHealth <= 0 || enemyDead) return;

        if (isInvisible)
        {
            if (sfx != null) sfx.ShadowSFX();
            return;
        }

        if (sfx != null) sfx.HitSFX();

        if (isDamageAmplified)
        {
            damage *= damageAmpMultiplier;
        }

        if (bossScript != null)
        {
            bossScript.TakeDamage(damage);
            CurrentHealth = bossScript.currentPhaseHealth;
            MaxHealth = bossScript.phaseMaxHealth[bossScript.currentPhaseIndex];
        }
        else
        {
            CurrentHealth -= damage;
        }

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
        }

        StartCoroutine(HitAnimation());

        // Shrink the 2D HealthBar on the X-axis based on remaining HP percentage
        if (HealthBar != null)
        {
            float healthPercent = CurrentHealth / MaxHealth;
            HealthBar.transform.localScale = new Vector3(
                healthBarInitialScale.x * healthPercent, 
                healthBarInitialScale.y, 
                healthBarInitialScale.z
            );
        }

        if (enemyControllerScript != null)
        {
            enemyControllerScript.currentHealth = CurrentHealth;
        }

        if (CurrentHealth == 0 && !enemyDead)
        {
            enemyDead = true;
            StartCoroutine(EnemyDeath());
        }
    }

    IEnumerator HitAnimation()
    {
        float counter = 0;
        Color flashColor = isPoisoned ? new Color(0.6f, 0f, 0.6f) : Color.red;

        while (counter < 1f)
        {
            counter += Time.deltaTime * 10f;
            float clampedCounter = Mathf.Clamp01(counter);

            float currentAngle = Mathf.Lerp(ghostBarInitialScale.x + 0.5f, ghostBarInitialScale.x, clampedCounter);
            if (GhostBar != null) GhostBar.transform.localScale = new Vector3(currentAngle, currentAngle, 1);

            if (enemySprite != null)
            {
                enemySprite.color = Color.Lerp(flashColor, GetBaseStatusColor(), clampedCounter);
            }

            yield return null;
        }

        UpdateSpriteColor();
        if (GhostBar != null) GhostBar.transform.localScale = ghostBarInitialScale;
    }

    IEnumerator EnemyDeath()
    {
        Debug.Log("DEATH TRIGGERED");

        if (poisonCoroutine != null) StopCoroutine(poisonCoroutine);
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        if (pushbackCoroutine != null) StopCoroutine(pushbackCoroutine);

        if (sfx != null) sfx.DeathSFX();

        for (int i = 0; i < coinDrop; i++)
        {
            if (coinPrefab != null)
            {
                Instantiate(coinPrefab, transform.position, Quaternion.identity);
            }
        }

        if (enemySprite != null) enemySprite.enabled = false;
        
        // Hide UI GameObjects on death
        if (HealthBar != null) HealthBar.SetActive(false);
        if (GhostBar != null) GhostBar.SetActive(false);

        yield return new WaitForSeconds(1f);
        
        Destroy(gameObject); 
    }

    #endregion

    #region Color & Visual Status Overlays

    public void UpdateSpriteColor()
    {
        if (enemySprite != null)
        {
            enemySprite.color = GetBaseStatusColor();
        }
    }

    private Color GetBaseStatusColor()
    {
        if (!canMove) return Color.gray; 
        if (isPoisoned) return new Color(0.3f, 0.8f, 0.3f);
        if (isDamageAmplified) return new Color(1f, 0.9f, 0.5f);

        if (bossScript != null)
        {
            switch (bossScript.currentPhase)
            {
                case BossPhase.Phase2_Enraged: return new Color(1f, 0.3f, 0.3f, 1f); 
                case BossPhase.Phase3_Overload: return new Color(1f, 0.9f, 0.2f, 1f); 
                default: return Color.white;
            }
        }

        if (enemyControllerScript != null)
        {
            switch (enemyControllerScript.enemyType)
            {
                case EnemyType.ShadowNinja:
                case EnemyType.SmokeBomber:
                    if (isInvisible && !isLanternRevealed)
                    {
                        return new Color(0.7f, 0.7f, 0.7f, 0.7f); 
                    }
                    break;

                case EnemyType.Saboteur:
                    return new Color(0.8f, 0.3f, 1f, 1f); 

                case EnemyType.EchoShinobi:
                    if (enemyControllerScript.isClone)
                    {
                        return new Color(0.4f, 0.7f, 1f, 0.6f); 
                    }
                    break;
            }
        }

        return Color.white;
    }

    #endregion

    #region Tower Interaction Messages (BroadcastMessage Targets)

    public void ApplyPushback(Vector2 force)
    {
        if (pushbackCoroutine != null) StopCoroutine(pushbackCoroutine);
        pushbackCoroutine = StartCoroutine(SmoothPushbackRoutine(force, 0.2f));
    }

    IEnumerator SmoothPushbackRoutine(Vector2 force, float duration)
    {
        float elapsed = 0f;
        Vector3 pushVector = (Vector3)force;

        while (elapsed < duration)
        {
            transform.position += pushVector * (Time.deltaTime / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public void ApplySlow(float factor)
    {
        currentSlowFactor = factor;

        CancelInvoke(nameof(ResetSlowFactor));
        Invoke(nameof(ResetSlowFactor), 0.5f);
    }

    private void ResetSlowFactor()
    {
        currentSlowFactor = 1f;
        UpdateSpriteColor();
    }

    public void RevealInvisible(bool isRevealed)
    {
        isLanternRevealed = isRevealed;

        if (enemyControllerScript != null)
        {
            enemyControllerScript.isLanternRevealed = isRevealed;
        }

        UpdateSpriteColor();

        if (isRevealed)
        {
            Debug.Log("Stealth Broken by Lantern Light Cone!");
        }
    }

    public void SetDamageAmplification(bool active)
    {
        isDamageAmplified = active;
        UpdateSpriteColor();
    }

    public void InterruptSkills()
    {
        Debug.Log("Skill Interrupted by Bell Tower shockwave!");

        if (sfx != null) sfx.DebuffSFX();

        if (isInvisible && enemyControllerScript != null && enemyControllerScript.enemyType == EnemyType.SmokeBomber)
        {
            isInvisible = false;
            enemyControllerScript.isInvisible = false;
            UpdateSpriteColor();
        }
    }

    public void ApplyStun(float duration)
    {
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration)
    {
        canMove = false;
        UpdateSpriteColor();

        yield return new WaitForSeconds(duration);

        canMove = true;
        UpdateSpriteColor();
    }

    public void ApplyPoisonDebuff(object[] payload)
    {
        float dps = (float)payload[0];
        float duration = (float)payload[1];

        if (poisonCoroutine != null) StopCoroutine(poisonCoroutine);
        poisonCoroutine = StartCoroutine(PoisonTickRoutine(dps, duration));
    }

    IEnumerator PoisonTickRoutine(float dps, float duration)
    {
        isPoisoned = true;
        float elapsed = 0f;
        float tickInterval = 0.5f;

        UpdateSpriteColor();

        while (elapsed < duration)
        {
            if (CurrentHealth <= 0) break;

            TakeDamage(dps * tickInterval);

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        isPoisoned = false;
        UpdateSpriteColor();
    }

    #endregion
}