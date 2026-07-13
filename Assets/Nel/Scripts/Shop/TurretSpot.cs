using UnityEngine;

public class TurretSpot : MonoBehaviour
{
    public GameObject currentTurret;

    public bool Occupied => currentTurret != null;

    public GameObject PlaceTurret(GameObject prefab, Quaternion rotation)
    {
        int costToPay = TurretPlacer.Instance.GetSelectedCost();

        if (!CoinManager.Instance.SpendCoins(costToPay))
        {
            return null; 
        }

        currentTurret = Instantiate(
            prefab,
            transform.position,
            rotation
        );

        TowerBehaviors towerScript = currentTurret.GetComponent<TowerBehaviors>();
        if (towerScript != null)
        {
            towerScript.towerLevel = 1;
            towerScript.UpdateSellValue();

            towerScript.facingDirection = (Vector2)currentTurret.transform.up;
        }

        return currentTurret;
    }

    public void RemoveTurret()
    {
        if (currentTurret == null)
            return;

        Destroy(currentTurret);
        currentTurret = null;
    }
}