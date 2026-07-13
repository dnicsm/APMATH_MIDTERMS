using UnityEngine;
using System.Collections.Generic;

public class RicochetShuriken : MonoBehaviour
{
    public float speed = 12f;
    public float rotationSpeed = 720f;
    public float bounceRange = 4.0f;

    private GameObject currentTarget;
    private float damage;
    private int remainingBounces;
    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

    public void Setup(GameObject initialTarget, float dmg, int maxBounces)
    {
        currentTarget = initialTarget;
        damage = dmg;
        remainingBounces = maxBounces;
    }

    void Update()
    {
        if (currentTarget == null)
        {
            FindNextTarget();
            if (currentTarget == null)
            {
                Destroy(gameObject);
                return;
            }
        }

        Vector3 targetPos = currentTarget.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.BroadcastMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            hitEnemies.Add(currentTarget); 
        }

        remainingBounces--;

        if (remainingBounces > 0)
        {
            FindNextTarget();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void FindNextTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject bestTarget = null;
        float closestDist = float.MaxValue;
        Vector2 origin = transform.position;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || hitEnemies.Contains(enemy)) continue;

            float dist = Vector2.Distance(origin, enemy.transform.position);
            if (dist <= bounceRange && dist < closestDist)
            {
                closestDist = dist;
                bestTarget = enemy;
            }
        }

        currentTarget = bestTarget;

        if (currentTarget == null)
        {
            Destroy(gameObject);
        }
    }
}