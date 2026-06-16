using UnityEngine;
using System.Collections.Generic;

public class KatanaTurret : MonoBehaviour
{
    public int katanaLevel;
    public float radius;
    public float katanaDamage;

    public AudioSource audioSource;
    public AudioClip swingSound;

    private bool isDamagingActive = false;
    private HashSet<GameObject> damagedEnemies = new HashSet<GameObject>();

    private void Update()
    {
        if (isDamagingActive)
        {
            CheckForEnemies();
        }
    }

    private void CheckForEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || damagedEnemies.Contains(enemy)) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance <= radius)
            {
                enemy.BroadcastMessage(
                    "TakeDamage",
                    katanaDamage,
                    SendMessageOptions.DontRequireReceiver);

                    damagedEnemies.Add(enemy);
            }
        }
    }

    public void PlaySwingSound()
    {
        if (audioSource != null && swingSound != null)
        {
            audioSource.PlayOneShot(swingSound);
        }
    }

    public void StartDamageWindow()
    {
        isDamagingActive = true;
        damagedEnemies.Clear(); 
    }

    public void EndDamageWindow()
    {
        isDamagingActive = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isDamagingActive ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}