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