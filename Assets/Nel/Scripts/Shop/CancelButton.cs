using UnityEngine;

public class CancelButton : MonoBehaviour
{
    public void ClosePanel()
    {
        UpgradeSell.Instance.Hide();
    }
}