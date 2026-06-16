using UnityEngine;

public class TurretPlacer : MonoBehaviour
{
    public static TurretPlacer Instance;

    private GameObject selectedTurretPrefab;
    private int selectedCost;

    private void Awake()
    {
        Instance = this;
    }

    public void SelectTurret(GameObject turretPrefab, int cost)
    {
        selectedTurretPrefab = turretPrefab;
        selectedCost = cost;
    }

    public GameObject GetSelectedTurret()
    {
        return selectedTurretPrefab;
    }

    public int GetSelectedCost()
    {
        return selectedCost;
    }

    public bool HasSelectedTurret()
    {
        return selectedTurretPrefab != null;
    }

    public void ClearSelection()
    {
        selectedTurretPrefab = null;
        selectedCost = 0;
        PreviewManager.Instance.HidePreview();
    }
}