using UnityEngine;

public class PreviewManager : MonoBehaviour
{
    public static PreviewManager Instance;

    [Header("Visual Indicator Prefabs")]
    public GameObject rangeCirclePrefab;       // Full 360-degree circle
    public GameObject directionalConePrefab;   // Arc/Cone indicator (Wind Fan)
    public GameObject directionalBoxPrefab;    // Linear/Box indicator (Bamboo Spiker, Ballista)

    private GameObject previewTower;
    private GameObject activeIndicator;
    private TowerType currentTowerType;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowPreview(GameObject turretPrefab, Vector3 position)
    {
        if (turretPrefab == null)
            return;

        // Rebuild preview if switching tower types
        if (previewTower == null || previewTower.name.Replace("(Clone)", "") != turretPrefab.name)
        {
            HidePreview();

            // Tower always spawns at default 0-degree rotation
            previewTower = Instantiate(turretPrefab, position, Quaternion.identity);

            DisableTurretBehaviour(previewTower);

            TowerBehaviors towerScript = previewTower.GetComponent<TowerBehaviors>();
            if (towerScript != null)
            {
                currentTowerType = towerScript.towerType;
                SetupIndicatorPrefab(currentTowerType, position, TurretPlacer.Instance.GetSelectedRotation());
            }
            else
            {
                activeIndicator = Instantiate(rangeCirclePrefab, position, Quaternion.identity);
            }
        }

        previewTower.transform.position = position;
        
        // Match indicator rotation to placement angle
        UpdatePreviewRotation(TurretPlacer.Instance.GetSelectedRotation());
    }

    public void HidePreview()
    {
        if (previewTower != null)
            Destroy(previewTower);

        if (activeIndicator != null)
            Destroy(activeIndicator);
    }

    private void DisableTurretBehaviour(GameObject tower)
    {
        MonoBehaviour[] scripts = tower.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            script.enabled = false;
        }
    }

    private void SetupIndicatorPrefab(TowerType towerType, Vector3 position, Quaternion rotation)
    {
        GameObject prefabToInstantiate = rangeCirclePrefab;

        switch (towerType)
        {
            case TowerType.Bamboo_Spiker:
            case TowerType.Balista:
                prefabToInstantiate = directionalBoxPrefab != null ? directionalBoxPrefab : rangeCirclePrefab;
                break;

            case TowerType.Wind_Fan_Pagoda:
                prefabToInstantiate = directionalConePrefab != null ? directionalConePrefab : rangeCirclePrefab;
                break;

            case TowerType.Lantern_Tower:
            case TowerType.Bell_Tower:
            case TowerType.Shuriken_Launcher:
            case TowerType.Poison_Dart_Blowpipe:
            default:
                prefabToInstantiate = rangeCirclePrefab;
                break;
        }

        if (prefabToInstantiate != null)
        {
            activeIndicator = Instantiate(prefabToInstantiate, position, rotation);
        }
    }

    private void UpdateIndicatorTransform()
    {
        if (previewTower == null || activeIndicator == null)
            return;

        TowerBehaviors towerScript = previewTower.GetComponent<TowerBehaviors>();
        float range = towerScript != null ? towerScript.towerRange : 1f;

        Vector3 towerPos = previewTower.transform.position;

        // Default position center for ALL towers
        activeIndicator.transform.position = towerPos;

        switch (currentTowerType)
        {
            case TowerType.Bamboo_Spiker:
                // Only Bamboo Spiker overrides position with a forward offset
                Vector3 facingDir = activeIndicator.transform.up;
                activeIndicator.transform.position = towerPos + (facingDir * 1.5f);
                activeIndicator.transform.localScale = new Vector3(3f, 3f, 1f);
                break;

            case TowerType.Balista:
                activeIndicator.transform.localScale = new Vector3(range, 1f, 1f);
                break;

            case TowerType.Wind_Fan_Pagoda:
                activeIndicator.transform.localScale = Vector3.one * range;
                break;

            case TowerType.Lantern_Tower:
            case TowerType.Bell_Tower:
            case TowerType.Shuriken_Launcher:
            case TowerType.Poison_Dart_Blowpipe:
            default:
                activeIndicator.transform.localScale = Vector3.one * (range * 2f);
                break;
        }
    }

    public void UpdatePreviewRotation(Quaternion newRotation)
    {
        // Keep tower fixed, only rotate the indicator
        if (activeIndicator != null)
        {
            activeIndicator.transform.rotation = newRotation;
        }

        UpdateIndicatorTransform();
    }
}