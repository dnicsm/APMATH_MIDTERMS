using System.Collections; 
using UnityEngine;

public class ShurikenTurret : MonoBehaviour
{
    public int shurikenLevel; 
    public GameObject shurikenPrefab;
    public Transform firePoint;

    public float range;
    public float fireRate = 1f; 
    public float shurikenDamage;
    public int burstCount;
    public float burstDelay = 0.1f; 
    public float spreadAngle = 15f;

    public Animator animator; 

    public AudioSource audioSource;
    public AudioClip throwSound;
    [Range(0f, 1f)] public float volume = 0.5f;

    private GameObject target;
    private float timer;

    void Update()
    {
        FindTarget();

        if (target == null)
        {
            if (animator != null)
            {
                animator.speed = 1f;
                animator.SetBool("IsAttacking", false);
            }
            return;
        }

        Vector3 direction = target.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        if (firePoint != null)
        {
            firePoint.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        float cleanAngle = (angle % 360f + 360f) % 360f;
        UpdateDirectionalAnimation(cleanAngle);

        if (animator != null)
        {
            if (fireRate > 0) animator.speed = 1f / fireRate;
            animator.SetBool("IsAttacking", true);
        }

        timer += Time.deltaTime;
        if (timer >= fireRate)
        {
            timer = 0f;
            StartCoroutine(ShootBurst());
        }
    }

    private void UpdateDirectionalAnimation(float angle)
    {
        if (animator == null) return;

        int directionIndex = 0;

        if (angle >= 60f && angle < 120f)       directionIndex = 3; 
        else if (angle >= 120f && angle < 180f) directionIndex = 4; 
        else if (angle >= 180f && angle < 240f) directionIndex = 5; 
        else if (angle >= 240f && angle < 300f) directionIndex = 0; 
        else if (angle >= 300f && angle < 360f) directionIndex = 1; 
        else                                    directionIndex = 2; 

        animator.SetInteger("Direction", directionIndex);
    }

    IEnumerator ShootBurst()
    {
        if (firePoint == null || shurikenPrefab == null) yield break;

        float startAngle = -(spreadAngle / 2f);
        float angleStep = burstCount > 1 ? spreadAngle / (burstCount - 1) : 0f;

        for (int i = 0; i < burstCount; i++)
        {
            if (target == null) yield break;

            float currentSpreadAngle = startAngle + (angleStep * i);
            Quaternion spreadRotation = firePoint.rotation * Quaternion.Euler(0, 0, currentSpreadAngle);

            GameObject shurikenGO = Instantiate(shurikenPrefab, firePoint.position, spreadRotation);
            Shuriken shuriken = shurikenGO.GetComponent<Shuriken>();

            if (shuriken != null)
            {
                shuriken.target = target;
                shuriken.damage = shurikenDamage;
            }

            if (audioSource != null && throwSound != null)
            {
                audioSource.PlayOneShot(throwSound, volume);
            }

            yield return new WaitForSeconds(burstDelay);
        }
    }

    void FindTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = range;
        target = null;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                target = enemy;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);

        if (target != null && firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, target.transform.position);
        }
    }
}