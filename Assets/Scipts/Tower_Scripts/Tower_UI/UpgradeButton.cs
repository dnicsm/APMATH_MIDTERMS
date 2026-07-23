using UnityEngine;

public class UpgradeButton : MonoBehaviour
{
    public void UpgradeTurret()
    {
        TurretSpot spot = UpgradeSell.Instance.GetSelectedSpot();

        if (spot == null)
            return;

        if (spot.currentTurret == null)
            return;

        TowerBehaviors tower = spot.currentTurret.GetComponent<TowerBehaviors>();

        if (tower == null)
            return;

        if (tower.towerLevel >= 3)
        {
            Debug.LogWarning("Tower is already max level!");
            return;
        }

        int costToUpgrade = tower.towerUpgradeCost;

        if (!CoinManager.Instance.SpendCoins(costToUpgrade))
        {
            Debug.LogWarning("Not enough coins to upgrade!");
            return;
        }

        tower.UpgradeTower();

        UpgradeSell.Instance.Hide();
    }
}