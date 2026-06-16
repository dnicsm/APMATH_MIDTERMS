using UnityEngine;

public class SellButton : MonoBehaviour
{
    public void SellTurret()
    {
        TurretSpot spot =
            UpgradeSell.Instance.GetSelectedSpot();

        if (spot == null)
            return;

        if (spot.currentTurret == null)
            return;

        TurretData data =
            spot.currentTurret.GetComponent<TurretData>();

        if (data != null)
        {
            int refund =
                Mathf.RoundToInt(
                    data.cost * 0.5f);

            CoinManager.Instance.AddCoins(refund);
        }

        Destroy(spot.currentTurret);

        spot.currentTurret = null;

        UpgradeSell.Instance.Hide();
    }
}
