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
    public float CurrentHealth;

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

    [Header("Hit Feedback")]
    public Color flashColor = new Color(1f, 0.6f, 0.6f, 1f);

    private bool isDamageAmplified = false;
    private float damageAmpMultiplier = 1.35f;

    private Coroutine poisonCoroutine;
    private Coroutine stunCoroutine;
    private Coroutine pushbackCoroutine;

    void Start()
    {
        sfx = GetComponent<EnemySFX>();
        if (sfx == null) sfx = GetComponentInChildren<EnemySFX>();

        enemySprite = GetComponent<SpriteRenderer>();

        if (GhostBar != null) 
        {
            ghostBarInitialScale = GhostBar.transform.localScale;
            if (ghostBarInitialScale.x <= 0) ghostBarInitialScale = Vector3.one;
        }

        if (HealthBar != null) 
        {
            healthBarInitialScale = HealthBar.transform.localScale;
            if (healthBarInitialScale.x <= 0) healthBarInitialScale = Vector3.one;
        }

        if (MaxHealth <= 0) MaxHealth = 100f;
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
        bool invisible = isInvisible || (enemyControllerScript != null && enemyControllerScript.isInvisible);
        bool revealed = isLanternRevealed || (enemyControllerScript != null && enemyControllerScript.isLanternRevealed);
        return !invisible || revealed;
    }

    #region Health & Damage Management

    public void TakeDamage(float damage)
    {
        if (CurrentHealth <= 0 || enemyDead) return;

        bool invisible = isInvisible || (enemyControllerScript != null && enemyControllerScript.isInvisible);
        bool revealed = isLanternRevealed || (enemyControllerScript != null && enemyControllerScript.isLanternRevealed);

        // Invisible targets block incoming direct hits unless revealed by a Lantern
        if (invisible && !revealed)
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

        if (HealthBar != null && MaxHealth > 0)
        {
            float healthPercent = Mathf.Clamp01(CurrentHealth / MaxHealth);
            HealthBar.transform.localScale = new Vector3(
                healthBarInitialScale.x * healthPercent, 
                healthBarInitialScale.y, 
                healthBarInitialScale.z
            );
        }

        if (enemyControllerScript != null)
        {
            enemyControllerScript.currentHealth = CurrentHealth;
            if (enemyControllerScript.enemyType == EnemyType.SmokeBomber)
            {
                enemyControllerScript.CheckSmokeBombTrigger();
            }
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
        bool invisible = isInvisible || (enemyControllerScript != null && enemyControllerScript.isInvisible);
        bool revealed = isLanternRevealed || (enemyControllerScript != null && enemyControllerScript.isLanternRevealed);
        bool isClone = enemyControllerScript != null && enemyControllerScript.isClone;

        float alpha = 1f;

        if (invisible && !revealed)
        {
            alpha = 0.3f;
        }
        else if (isClone)
        {
            alpha = 0.6f;
        }

        Color baseColor = Color.white;
        if (bossScript != null)
        {
            baseColor = bossScript.GetPhaseColor();
        }

        return new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
    }

    #endregion

    #region Tower Interaction Messages

    public void RevealInvisible(bool isRevealed)
    {
        isLanternRevealed = isRevealed;

        if (enemyControllerScript != null)
        {
            enemyControllerScript.isLanternRevealed = isRevealed;
            enemyControllerScript.UpdateVisualOverlay();
        }

        UpdateSpriteColor();
    }

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

    public void SetDamageAmplification(bool active)
    {
        isDamageAmplified = active;
        UpdateSpriteColor();
    }

    public void InterruptSkills()
    {
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
        float elapsed = 0f;
        float tickInterval = 0.5f;

        while (elapsed < duration)
        {
            if (CurrentHealth <= 0) break;

            TakeDamage(dps * tickInterval);

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        UpdateSpriteColor();
    }

    #endregion
}