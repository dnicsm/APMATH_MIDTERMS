using UnityEngine;

public class SellButton : MonoBehaviour
{
    public void SellTurret()
    {
        TurretSpot spot = UpgradeSell.Instance.GetSelectedSpot();

        if (spot == null)
            return;

        if (spot.currentTurret == null)
            return;

        TowerBehaviors tower = spot.currentTurret.GetComponent<TowerBehaviors>();

        if (tower != null)
        {
            int refund = tower.towerSellValue;

            CoinManager.Instance.AddCoins(refund);
        }

        Destroy(spot.currentTurret);
        spot.currentTurret = null;

        UpgradeSell.Instance.Hide();
    }
}