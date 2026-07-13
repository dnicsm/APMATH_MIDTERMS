using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EnemyHealthManager : MonoBehaviour
{
    [Header("Health UI")]
    public Image GhostBar;
    public Image HealthBar;

    [Header("Coins")]
    public GameObject coinPrefab;

    [Header("Health Settings")]
    public float MaxHealth;
    [SerializeField] private float CurrentHealth;
    [SerializeField] private int coinDrop = 1;
    
    [Header("References")]
    public GameObject enemy;
    public EnemySFX sfx;

    [HideInInspector] public bool canMove = true;
    [HideInInspector] public float currentSlowFactor = 1f;

    public bool isBlinking = false;
    private Vector3 Health;
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
        if (sfx == null)
        {
            sfx = GetComponent<EnemySFX>();
            if (sfx == null) sfx = GetComponentInChildren<EnemySFX>();
        }
        Health = GhostBar.transform.localScale;
        CurrentHealth = MaxHealth;
        
        if (enemy != null)
        {
            enemySprite = enemy.GetComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        
    }

    public void TakeDamage(float damage)
    {
        if (CurrentHealth <= 0) return;
        if (isBlinking)
        {
            if (sfx != null) sfx.BlinkSFX(); 
            return;
        }

        if (sfx != null) sfx.HitSFX();

        if (isDamageAmplified)
        {
            damage *= damageAmpMultiplier;
        }

        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
        }

        StartCoroutine(HitAnimation());
        if (HealthBar != null) HealthBar.fillAmount = CurrentHealth / MaxHealth;

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

            float currentAngle = Mathf.Lerp(Health.x + 0.5f, Health.x, clampedCounter);
            if (GhostBar != null) GhostBar.transform.localScale = new Vector3(currentAngle, currentAngle, 1);

            if (enemySprite != null)
            {
                enemySprite.color = Color.Lerp(flashColor, GetBaseStatusColor(), clampedCounter);
            }
            
            yield return null;
        }

        if (enemySprite != null) enemySprite.color = GetBaseStatusColor();
        if (GhostBar != null) GhostBar.transform.localScale = Health;
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
        if (HealthBar != null) HealthBar.enabled = false;
        if (GhostBar != null) GhostBar.enabled = false;

        yield return new WaitForSeconds(1f);
        Destroy(enemy);
    }

    private Color GetBaseStatusColor()
    {
        if (!canMove) return Color.gray;
        if (isPoisoned) return Color.green;
        if (isDamageAmplified) return new Color(1f, 0.9f, 0.5f);
        return Color.white;
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
            if (enemy == null) yield break;
            
            enemy.transform.position += pushVector * (Time.deltaTime / duration);
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
        if (enemySprite != null) enemySprite.color = GetBaseStatusColor();
    }

    public void RevealInvisible(bool isRevealed) 
    {
        if (isRevealed)
        {
            if (enemySprite != null) enemySprite.color = new Color(1f, 1f, 1f, 1f);
            Debug.Log("Stealth Broken by Lantern Light Cone!");
        }
    }

    public void SetDamageAmplification(bool active) 
    {
        isDamageAmplified = active;
        if (enemySprite != null)
        {
            enemySprite.color = GetBaseStatusColor();
        }
    }

    public void InterruptSkills() 
    {
        Debug.Log("Ninja active skill loop interrupted by Bell Ring shockwave!");
    }

    public void ApplyStun(float duration) 
    {
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration)
    {
        canMove = false;
        if (enemySprite != null) enemySprite.color = GetBaseStatusColor(); 

        yield return new WaitForSeconds(duration);

        canMove = true;
        if (enemySprite != null) enemySprite.color = GetBaseStatusColor();
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

        if (enemySprite != null) enemySprite.color = GetBaseStatusColor();

        while (elapsed < duration)
        {
            if (CurrentHealth <= 0) break;

            TakeDamage(dps * tickInterval);

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        isPoisoned = false;
        if (enemySprite != null) enemySprite.color = GetBaseStatusColor();
    }
}