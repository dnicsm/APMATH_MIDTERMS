using TMPro;
using UnityEngine;

public class ShopButton : MonoBehaviour
{
    public GameObject turretPrefab;
    public int cost;

   public void BuyTurret()
    {
        TurretPlacer.Instance.SelectTurret(turretPrefab, cost);
    }
}
