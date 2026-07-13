using TMPro;
using UnityEngine;

public class ShopButton : MonoBehaviour
{
    [Header("Assign the Tower Prefab")]
    public GameObject turretPrefab;
    
    [Header("Optional Text Display")]
    public TMP_Text costText;

    private int cost;

    private void Start()
    {
        if (turretPrefab != null)
        {
            TowerBehaviors towerScript = turretPrefab.GetComponent<TowerBehaviors>();
            if (towerScript != null)
            {
                cost = towerScript.towerCost;
                
                if (costText != null)
                {
                    costText.text = $"{cost} Gold";
                }
            }
            else
            {
                Debug.LogError($"The prefab assigned to {gameObject.name} is missing the TowerBehaviors script!");
            }
        }
    }

    public void BuyTurret()
    {
        if (turretPrefab != null)
        {
            TurretPlacer.Instance.SelectTurret(turretPrefab, cost);
        }
    }
}