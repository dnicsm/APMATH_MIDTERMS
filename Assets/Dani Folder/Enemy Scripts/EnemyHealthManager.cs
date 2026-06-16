using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealthManager : MonoBehaviour
{
    [Header("Health UI")]
    public Image GhostBar;
    public Image HealthBar;

    [Header("Coins")]
    public GameObject coinPrefab;

    public float MaxHealth;
    [SerializeField]
    private float CurrentHealth;
    [SerializeField]
    private int coinDrop = 1;
    private Vector3 Health;
    public bool isBlinking = false;
    public GameObject enemy;
    private SpriteRenderer enemySprite;
    private bool enemyDead = false;

    public EnemySFX sfx;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (sfx == null)
        {
            sfx = GetComponent<EnemySFX>();
            if (sfx == null) sfx = GetComponentInChildren<EnemySFX>();
        }
        Health = GhostBar.transform.localScale;
        CurrentHealth = MaxHealth;
        enemySprite = enemy.GetComponent<SpriteRenderer>();
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void TakeDamage(float damage)
    {
        if (CurrentHealth <= 0) return;
        if (isBlinking)
        {
            if (sfx != null) sfx.BlinkSFX(); 
            return;
        }

        sfx.HitSFX();

        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
        }
        StopAllCoroutines();
        enemySprite.color = Color.white;
        StartCoroutine(HitAnimation());
        HealthBar.fillAmount = CurrentHealth / MaxHealth;

        if(CurrentHealth == 0)
        {
            enemyDead = true;
            if (enemyDead)
            {
                StartCoroutine(EnemyDeath());
            }
            
        }
    }

    IEnumerator HitAnimation()
    {
        float counter = 0;

        while (counter < 1f)
        {
            counter += Time.deltaTime * 10f;
            
            float clampedCounter = Mathf.Clamp01(counter);

            float currentAngle = Mathf.Lerp(Health.x + 0.5f, Health.x, clampedCounter);
            GhostBar.transform.localScale = new Vector3(currentAngle, currentAngle, 1);

            if (enemySprite != null)
            {
                enemySprite.color = Color.Lerp(Color.red, Color.white, clampedCounter);
            }
            
            yield return null;
        }

        if (enemySprite != null) enemySprite.color = Color.white;
        GhostBar.transform.localScale = Health;
    }

    IEnumerator EnemyDeath()
    {
        Debug.Log("DEATH TRIGGERED");
        enemyDead = false;
        sfx.DeathSFX();

        for (int i = 0; i < coinDrop; i++)
        {
           Instantiate(coinPrefab, transform.position, Quaternion.identity); 
        }
        
        if (enemySprite != null) enemySprite.enabled = false;
        if (HealthBar != null) HealthBar.enabled = false;
        if (GhostBar != null) GhostBar.enabled = false;

        yield return new WaitForSeconds(1f);
        Destroy(enemy);
    }

}