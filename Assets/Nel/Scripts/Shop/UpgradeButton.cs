using UnityEngine;

public class UpgradeButton : MonoBehaviour
{
    public void UpgradeTurret()
    {
        TurretSpot spot =
            UpgradeSell.Instance.GetSelectedSpot();

        if (spot == null)
            return;

        if (spot.currentTurret == null)
            return;

        TurretData data =
            spot.currentTurret.GetComponent<TurretData>();

        if (data == null)
            return;

        if (data.nextLevelPrefab == null)
            return;

        TurretData nextData =
            data.nextLevelPrefab.GetComponent<TurretData>();

        if (nextData == null)
            return;

        if (!CoinManager.Instance.SpendCoins(nextData.cost))
            return;

        Quaternion rot =
            spot.currentTurret.transform.rotation;

        Destroy(spot.currentTurret);

        spot.currentTurret =
            Instantiate(
                data.nextLevelPrefab,
                spot.transform.position,
                rot);

        UpgradeSell.Instance.Hide();
    }
}