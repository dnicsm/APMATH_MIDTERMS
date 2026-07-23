using UnityEngine;
using System.Collections.Generic;

public class TowerBehaviors : MonoBehaviour
{
    [Header("Tower Information")]
    public TowerType towerType;
    public int towerLevel;
    public int towerCost;
    public int towerUpgradeCost;
    public int towerSellValue;
    public float towerRange;
    public float towerDamage;
    public float towerAttackSpeed;

    [Header("Setup / References")]
    public LayerMask enemyLayer; 
    public Vector2 facingDirection;

    [Header("Visual Prefabs")]
    public GameObject spikePrefab;
    public float spikeDuration = 0.5f;

    public GameObject windPrefab;  
    public float windSpriteDuration = 0.4f;

    public GameObject lanternConePrefab;   
    public GameObject bellRingPrefab;  
    public float bellRingDuration = 0.3f;    
    public GameObject shurikenPrefab;      
    public GameObject ballistaBoltPrefab; 
    public float ballistaBoltDuration = 0.2f;
    public GameObject poisonDartPrefab;
    public float poisonDartDuration = 0.2f;

    [Header("Visual Scaling")]
    [Tooltip("How much the tower scales up per level (e.g., 0.2 means +20% size per level)")]
    public float scaleIncreasePerLevel = 0.15f; 
    
    // Store the initial scale so we can calculate exact growth reliably
    private Vector3 initialScale;

    private float attackCooldownTimer;
    private HashSet<GameObject> buffedEnemies = new HashSet<GameObject>();
    private GameObject activeLanternCone;
    private float saboteurDebuffTimer = 0f;

    void Start()
    {
        attackCooldownTimer = towerAttackSpeed;
        towerLevel = 1;
        UpdateSellValue();

        // Capture the starting scale of the GameObject (or you can target a specific child SpriteRenderer)
        initialScale = transform.localScale;
    }

    #region Upgrade Management
    [ContextMenu("Upgrade Tower")] 
    public void UpgradeTower()
    {
        if (towerLevel >= 3)
        {
            Debug.LogWarning("Tower is already at maximum level (Level 3)!");
            return;
        }

        towerLevel++;
        Debug.Log($"Upgrading to level: {towerLevel}");

        towerDamage += towerDamage / 2f;
        towerRange += towerRange / 2f;
        
        towerAttackSpeed -= towerAttackSpeed / 4f; 

        towerCost += towerUpgradeCost; 
        towerUpgradeCost += towerUpgradeCost / 2;

        UpdateSellValue();
        UpdateTowerVisualSize(); // <-- Call the new scaling function here

        Debug.Log($"{gameObject.name} upgraded to Level {towerLevel}!");
    }

    public void UpdateSellValue()
    {
        towerSellValue = (int)(towerCost - (towerCost / 1.5f));
    }

    // --- NEW FUNCTION: Scale the sprite ---
    private void UpdateTowerVisualSize()
    {
        // Calculate the new scale multiplier based on current level
        // Level 1 = 1.0 multiplier
        // Level 2 = 1.0 + (0.15 * 1) = 1.15
        // Level 3 = 1.0 + (0.15 * 2) = 1.30
        float scaleMultiplier = 1f + (scaleIncreasePerLevel * (towerLevel - 1));
        
        // Apply the new scale relative to the original starting scale
        transform.localScale = initialScale * scaleMultiplier;
        
        // Optional: If you only want to scale a child object containing the sprite, 
        // you would replace 'transform.localScale' with 'spriteRendererTransform.localScale'
    }
    #endregion

    void Update()
    {
        attackCooldownTimer -= Time.deltaTime;

        if (towerType == TowerType.Lantern_Tower)
        {
            ExecuteLanternLogic();
        }
        else if (attackCooldownTimer <= 0f)
        {
            ExecuteAttack();
            attackCooldownTimer = towerAttackSpeed;
        }
    }

    private void ExecuteAttack()
    {
        switch (towerType)
        {
            case TowerType.Bamboo_Spiker:
                ExecuteBambooSpiker();
                break;
            case TowerType.Wind_Fan_Pagoda:
                ExecuteWindFan();
                break;
            case TowerType.Bell_Tower:
                ExecuteBellTower();
                break;
            case TowerType.Shuriken_Launcher:
                ExecuteShurikenLauncher();
                break;
            case TowerType.Balista:
                ExecuteBallista();
                break;
            case TowerType.Poison_Dart_Blowpipe:
                ExecutePoisonBlowpipe();
                break;
        }
    }

    public void ApplySaboteurDebuff(float reductionFactor)
    {
        if (saboteurDebuffTimer <= 0f)
        {
            towerAttackSpeed += towerAttackSpeed * reductionFactor;
        }
        saboteurDebuffTimer = 0.5f; 
    }

    private GameObject[] GetAllEnemies()
    {
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        List<GameObject> validTargets = new List<GameObject>();

        foreach (GameObject enemy in allEnemies)
        {
            EnemyController advEnemy = enemy.GetComponent<EnemyController>();
            if (advEnemy != null)
            {
                if (advEnemy.IsTargetable()) validTargets.Add(enemy);
            }
            else
            {
                validTargets.Add(enemy); 
            }
        }
        return validTargets.ToArray();
    }

    #region 1. Bamboo Spiker
    private void ExecuteBambooSpiker()
    {
        Vector2 checkCenter = (Vector2)transform.position + (facingDirection * 1.5f);
        Vector2 boxSize = new Vector2(3f, 3f);

        if (spikePrefab != null)
        {
            GameObject spawnedSpike = Instantiate(spikePrefab, checkCenter, Quaternion.identity);
            Destroy(spawnedSpike, spikeDuration);
        }

        GameObject[] enemies = GetAllEnemies();
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            Vector2 enemyPos = enemy.transform.position;

            if (Mathf.Abs(enemyPos.x - checkCenter.x) <= boxSize.x / 2 &&
                Mathf.Abs(enemyPos.y - checkCenter.y) <= boxSize.y / 2)
            {
                enemy.BroadcastMessage("TakeDamage", towerDamage, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
    #endregion

    #region 2. Wind Fan Pagoda
    private void ExecuteWindFan()
    {
        if (windPrefab != null)
        {
            Vector3 spawnPosition = transform.position + (Vector3)(facingDirection * 2.5f);
            float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            GameObject spawnedWind = Instantiate(windPrefab, spawnPosition, rotation);
            Destroy(spawnedWind, windSpriteDuration);
        }

        GameObject[] enemies = GetAllEnemies();
        Vector2 origin = transform.position;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            Vector2 enemyPos = enemy.transform.position;
            float distance = Vector2.Distance(origin, enemyPos);

            if (distance >= 2f && distance <= towerRange)
            {
                Vector2 dirToEnemy = (enemyPos - origin).normalized;
                if (Vector2.Dot(dirToEnemy, facingDirection) > 0.7f)
                {
                    enemy.BroadcastMessage("ApplyPushback", facingDirection * 2f, SendMessageOptions.DontRequireReceiver);
                    enemy.BroadcastMessage("ApplySlow", 0.5f, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
    #endregion

    #region 3. Lantern Tower
    private void ExecuteLanternLogic()
    {
        if (lanternConePrefab != null && activeLanternCone == null)
        {
            float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
            activeLanternCone = Instantiate(lanternConePrefab, transform.position, Quaternion.Euler(0, 0, angle), transform);
        }

        GameObject[] enemies = GetAllEnemies();
        Vector2 origin = transform.position;
        HashSet<GameObject> currentEnemiesInLight = new HashSet<GameObject>();

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            Vector2 enemyPos = enemy.transform.position;

            if (Vector2.Distance(origin, enemyPos) <= towerRange)
            {
                Vector2 dirToEnemy = (enemyPos - origin).normalized;
                if (Vector2.Dot(dirToEnemy, facingDirection) > 0.5f)
                {
                    currentEnemiesInLight.Add(enemy);
                    enemy.BroadcastMessage("RevealInvisible", true, SendMessageOptions.DontRequireReceiver);

                    if (!buffedEnemies.Contains(enemy))
                    {
                        enemy.BroadcastMessage("SetDamageAmplification", true, SendMessageOptions.DontRequireReceiver);
                        buffedEnemies.Add(enemy);
                    }
                }
            }
        }

        List<GameObject> toRemove = new List<GameObject>();
        foreach (GameObject enemy in buffedEnemies)
        {
            if (!currentEnemiesInLight.Contains(enemy) || enemy == null)
            {
                if (enemy != null)
                {
                    enemy.BroadcastMessage("RevealInvisible", false, SendMessageOptions.DontRequireReceiver);
                    enemy.BroadcastMessage("SetDamageAmplification", false, SendMessageOptions.DontRequireReceiver);
                }
                toRemove.Add(enemy);
            }
        }
        foreach (GameObject r in toRemove) buffedEnemies.Remove(r);
    }
    #endregion

    #region 4. Bell Tower
    private void ExecuteBellTower()
    {
        if (bellRingPrefab != null)
        {
            GameObject wave = Instantiate(bellRingPrefab, transform.position, Quaternion.identity);
            wave.transform.localScale = new Vector3(towerRange * 2, towerRange * 2, 1);
            Destroy(wave, bellRingDuration);
        }

        GameObject[] enemies = GetAllEnemies();
        Vector2 origin = transform.position;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            if (Vector2.Distance(origin, enemy.transform.position) <= towerRange)
            {
                enemy.BroadcastMessage("InterruptSkills", SendMessageOptions.DontRequireReceiver);
                enemy.BroadcastMessage("ApplyStun", 1.5f, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
    #endregion

    #region 5. Shuriken Launcher
    private void ExecuteShurikenLauncher()
    {
        GameObject target = GetInitialShurikenTarget();
        if (target != null)
        {
            StartCoroutine(FireShurikenBurst(target));
        }
    }

    private GameObject GetInitialShurikenTarget()
    {
        GameObject[] enemies = GetAllEnemies();
        GameObject bestTarget = null;
        float closestDist = float.MaxValue;
        Vector2 origin = transform.position;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            float dist = Vector2.Distance(origin, enemy.transform.position);
            if (dist <= towerRange && dist < closestDist)
            {
                closestDist = dist;
                bestTarget = enemy;
            }
        }
        return bestTarget;
    }

    private System.Collections.IEnumerator FireShurikenBurst(GameObject initialTarget)
    {
        int totalShurikens = 3;
        float delayBetweenShurikens = 0.15f;

        for (int i = 0; i < totalShurikens; i++)
        {
            if (initialTarget == null) yield break; 

            if (shurikenPrefab != null)
            {
                GameObject shurikenGo = Instantiate(shurikenPrefab, transform.position, Quaternion.identity);
                RicochetShuriken shurikenScript = shurikenGo.GetComponent<RicochetShuriken>();

                if (shurikenScript != null)
                {
                    shurikenScript.Setup(initialTarget, towerDamage, 3);
                }
            }
            yield return new WaitForSeconds(delayBetweenShurikens);
        }
    }
    #endregion

    #region 6. Ballista
    private void ExecuteBallista()
    {
        if (ballistaBoltPrefab != null)
        {
            Vector3 spawnPos = transform.position + (Vector3)(facingDirection * (towerRange / 2f));
            float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
            
            GameObject bolt = Instantiate(ballistaBoltPrefab, spawnPos, Quaternion.Euler(0, 0, angle));
            bolt.transform.localScale = new Vector3(towerRange, 1f, 1f); 
            Destroy(bolt, ballistaBoltDuration);
        }

        GameObject[] enemies = GetAllEnemies();
        Vector2 origin = transform.position;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            Vector2 enemyPos = enemy.transform.position;
            Vector2 targetVector = enemyPos - origin;

            float projection = Vector2.Dot(targetVector, facingDirection);

            if (projection >= 0 && projection <= towerRange)
            {
                Vector2 closestPoint = origin + (facingDirection * projection);
                if (Vector2.Distance(enemyPos, closestPoint) <= 0.5f)
                {
                    enemy.BroadcastMessage("TakeDamage", towerDamage, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
    #endregion

    #region 7. Poison Dart Blowpipe
    private void ExecutePoisonBlowpipe()
    {
        GameObject[] enemies = GetAllEnemies();
        Vector2 origin = transform.position;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            if (Vector2.Distance(origin, enemy.transform.position) <= towerRange)
            {
                if (poisonDartPrefab != null)
                {
                    Vector3 targetDir = (enemy.transform.position - transform.position).normalized;
                    float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
                    
                    Vector3 spawnPos = (transform.position + enemy.transform.position) / 2f;
                    GameObject dart = Instantiate(poisonDartPrefab, spawnPos, Quaternion.Euler(0, 0, angle));
                    Destroy(dart, poisonDartDuration);
                }

                object[] poisonPayload = new object[] { towerDamage, 4.0f };
                enemy.BroadcastMessage("ApplyPoisonDebuff", poisonPayload, SendMessageOptions.DontRequireReceiver);
                break;
            }
        }
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, towerRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, (Vector3)facingDirection * 2f);
    }
}

public enum TowerType
{
    Bamboo_Spiker,
    Wind_Fan_Pagoda,
    Lantern_Tower,
    Bell_Tower,
    Shuriken_Launcher,
    Balista,
    Poison_Dart_Blowpipe
}