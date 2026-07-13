using UnityEngine;

public class TurretPlacer : MonoBehaviour
{
    public static TurretPlacer Instance;

    private GameObject selectedTurretPrefab;
    private int selectedCost;
    
    private float currentPlacementRotation = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (HasSelectedTurret())
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RotateSelection();
            }
        }
    }

    public void SelectTurret(GameObject turretPrefab, int cost)
    {
        selectedTurretPrefab = turretPrefab;
        selectedCost = cost;
        currentPlacementRotation = 0f;
    }

    private void RotateSelection()
    {
        currentPlacementRotation -= 90f;

        if (currentPlacementRotation <= -360f) currentPlacementRotation += 360f;

        if (PreviewManager.Instance != null)
        {
            PreviewManager.Instance.UpdatePreviewRotation(Quaternion.Euler(0f, 0f, currentPlacementRotation));
        }
    }

    public Quaternion GetSelectedRotation()
    {
        return Quaternion.Euler(0f, 0f, currentPlacementRotation);
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
        currentPlacementRotation = 0f;
        PreviewManager.Instance.HidePreview();
    }
}