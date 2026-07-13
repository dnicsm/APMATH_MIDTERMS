using UnityEngine;

public class PreviewManager : MonoBehaviour
{
    public static PreviewManager Instance;

    [Header("Visual References")]
    public GameObject rangeCirclePrefab;

    private GameObject previewTower;
    private GameObject rangeCircle;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowPreview(GameObject turretPrefab, Vector3 position)
    {
        if (turretPrefab == null)
            return;

        if (previewTower == null || previewTower.name.Replace("(Clone)", "") != turretPrefab.name)
        {
            HidePreview();

            Quaternion currentPlacerRot = TurretPlacer.Instance.GetSelectedRotation();
            previewTower = Instantiate(turretPrefab, position, currentPlacerRot);

            DisableTurretBehaviour(previewTower);

            rangeCircle = Instantiate(rangeCirclePrefab, position, Quaternion.identity);
        }

        previewTower.transform.position = position;
        rangeCircle.transform.position = position;

        UpdateRange();
    }

    public void HidePreview()
    {
        if (previewTower != null)
            Destroy(previewTower);

        if (rangeCircle != null)
            Destroy(rangeCircle);
    }

    private void DisableTurretBehaviour(GameObject tower)
    {
        MonoBehaviour[] scripts = tower.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            script.enabled = false;
        }
    }

    private void UpdateRange()
    {
        if (previewTower == null || rangeCircle == null)
            return;

        float range = 1f;

        TowerBehaviors towerScript = previewTower.GetComponent<TowerBehaviors>();
        if (towerScript != null)
        {
            range = towerScript.towerRange;
        }

        rangeCircle.transform.localScale = Vector3.one * range * 2f;
    }
    public void UpdatePreviewRotation(Quaternion newRotation)
    {
        if (previewTower != null)
        {
            previewTower.transform.rotation = newRotation;
        }
    }
}