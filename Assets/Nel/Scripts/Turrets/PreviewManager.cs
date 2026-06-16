using UnityEngine;

public class PreviewManager : MonoBehaviour
{
    public static PreviewManager Instance;

    public GameObject rangeCirclePrefab;

    GameObject previewTower;
    GameObject rangeCircle;

    float currentRotation;

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

            currentRotation = 0f;

            previewTower = Instantiate(turretPrefab, position, Quaternion.identity);

            DisableTurretBehaviour(previewTower);

            rangeCircle = Instantiate(rangeCirclePrefab, position, Quaternion.identity);
        }

        previewTower.transform.position = position;
        rangeCircle.transform.position = position;

        UpdateRange();
        UpdatePreviewVisuals();
    }

    public void HidePreview()
    {
        if (previewTower != null)
            Destroy(previewTower);

        if (rangeCircle != null)
            Destroy(rangeCircle);
    }

    void DisableTurretBehaviour(GameObject tower)
    {
        MonoBehaviour[] scripts = tower.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            script.enabled = false;
        }
    }

    void UpdateRange()
    {
        if (previewTower == null || rangeCircle == null)
            return;

        float range = 1f;


        LaserTurret laser = previewTower.GetComponent<LaserTurret>();
        if (laser != null)
            range = laser.laserRange;

        KatanaTurret katana = previewTower.GetComponent<KatanaTurret>();
        if (katana != null)
            range = katana.radius;

        ShurikenTurret shuriken = previewTower.GetComponent<ShurikenTurret>();
        if (shuriken != null)
            range = shuriken.range;

        rangeCircle.transform.localScale = Vector3.one * range * 2f;
    }

    public void RotatePreview(float amount)
    {
        if (previewTower == null)
            return;

        currentRotation += amount;

        UpdatePreviewVisuals();
    }

    public Quaternion GetRotation()
    {
        return Quaternion.Euler(0, 0, currentRotation);
    }

    private void UpdatePreviewVisuals()
    {
        if (previewTower == null) return;

        LaserTurret laserScript = previewTower.GetComponent<LaserTurret>();
        if (laserScript == null) return; 

        Animator previewAnim = previewTower.GetComponent<Animator>();
        if (previewAnim == null) return;

        previewTower.transform.rotation = Quaternion.identity;

        float zAngle = Mathf.Round((currentRotation % 360f + 360f) % 360f);

        string animBaseName = "";

        if (zAngle == 0f || zAngle == 360f)      { animBaseName = "LaserFront"; }
        else if (zAngle == 90f)                 { animBaseName = "LaserRight"; }
        else if (zAngle == 180f)                { animBaseName = "LaserBack"; }
        else if (zAngle == 270f)                { animBaseName = "LaserLeft"; }

        int lvl = laserScript.laserLevel; 
        string finalAnimName = $"{animBaseName}{lvl}";
        
        previewAnim.Play(finalAnimName);
    }
}