using UnityEngine;
using System.Collections.Generic;

public class LaserTurret : MonoBehaviour
{
    public int laserLevel;

    public float laserRange;
    public float laserDamage;
    public float laserWidth;

    public AudioSource audioSource;
    public AudioClip laserSound;

    private bool isLaserActive = false;
    private Vector2 currentLaserDirection = Vector2.up;
    private HashSet<GameObject> damagedEnemies = new HashSet<GameObject>();

    void Update()
    {

        if (isLaserActive)
        {
            LaserCollision();
        }
    }

    private void LaserCollision()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Vector2 origin = (Vector2)transform.position;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || damagedEnemies.Contains(enemy)) continue;

            Vector2 enemyPos = (Vector2)enemy.transform.position;
            Vector2 v = enemyPos - origin;

            float projection = Vector2.Dot(v, currentLaserDirection);

            if (projection >= 0 && projection <= laserRange)
            {
                Vector2 closestPointOnLine = origin + (currentLaserDirection * projection);
                float distanceToBeam = Vector2.Distance(enemyPos, closestPointOnLine);

                if (distanceToBeam <= laserWidth)
                {
                    enemy.BroadcastMessage("TakeDamage", laserDamage, SendMessageOptions.DontRequireReceiver);
                    damagedEnemies.Add(enemy); 
                }
            }
        }
    }


    public void StartLaserDamage(string direction)
    {
        isLaserActive = true;
        damagedEnemies.Clear(); 


        string lowerDir = direction.ToLower().Trim();
        if (lowerDir == "front")      currentLaserDirection = Vector2.down; 
        else if (lowerDir == "back")  currentLaserDirection = Vector2.up;   
        else if (lowerDir == "left")  currentLaserDirection = Vector2.left;
        else if (lowerDir == "right") currentLaserDirection = Vector2.right;
        else                          currentLaserDirection = Vector2.up;   

        if (audioSource != null && laserSound != null)
        {
            audioSource.PlayOneShot(laserSound);
        }
    }

    public void EndLaserDamage()
    {
        isLaserActive = false;
    }

    private void OnDrawGizmos()
    {
        if (isLaserActive)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(currentLaserDirection * laserRange));
        }
    }
}