using UnityEngine;

public class Shuriken : MonoBehaviour
{
    public float speed = 10f;
    public float spinSpeed = 720f; 
    public float damage; 
    public float hitRadius = 0.35f; 

    public float homingStrength = 15f; 

    public GameObject target; 
    
    private Vector2 moveDirection;

    void Start()
    {
        moveDirection = (Vector2)transform.up;
        Destroy(gameObject, 1f); 
    }

    void Update()
    {
        if (target != null)
        {
            Vector2 targetPos = (Vector2)target.transform.position;
            Vector2 currentPos = (Vector2)transform.position;
            Vector2 directionToTarget = (targetPos - currentPos).normalized;
            
            float distanceToTarget = Vector2.Distance(currentPos, targetPos);

            if (distanceToTarget < 0.8f)
            {
                moveDirection = directionToTarget;
            }
            else
            {
                moveDirection = Vector2.Lerp(moveDirection, directionToTarget, homingStrength * Time.deltaTime).normalized;
            }
        }

        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

        CheckCollisions();
    }

    private void CheckCollisions()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            float distance = Vector2.Distance((Vector2)transform.position, (Vector2)enemy.transform.position);
            
            if (distance <= hitRadius)
            {
                enemy.BroadcastMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                Destroy(gameObject);
                return; 
            }
        }
    }
}