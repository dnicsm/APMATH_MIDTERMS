using UnityEngine;

public class TurretData : MonoBehaviour
{
    public int cost = 100;
    public GameObject nextLevelPrefab;

    public bool CanUpgrade()
    {
        return nextLevelPrefab != null;
    }
}
