using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TurretSpotManager : MonoBehaviour
{
    public static TurretSpotManager Instance;

    public List<TurretSpot> spots = new List<TurretSpot>();
    public float hoverDistance = 1f;

    TurretSpot currentSpot;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        spots.Clear();
        TurretSpot[] foundSpots = Object.FindObjectsByType<TurretSpot>();
        spots.AddRange(foundSpots);
    }

    void Update()
    {
        HandleHover();
        HandleInput();

        if (TurretPlacer.Instance.HasSelectedTurret())
        {
            GameObject selectedPrefab = TurretPlacer.Instance.GetSelectedTurret();
            
            if (selectedPrefab != null && selectedPrefab.GetComponent<LaserTurret>() != null)
            {
                float scroll = Input.mouseScrollDelta.y;
                if (scroll != 0)
                {
                    PreviewManager.Instance.RotatePreview(scroll * 90f);
                }
            }
        }
    }

    void HandleHover()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0;

        TurretSpot closest = null;
        float bestDistance = hoverDistance;

        foreach (TurretSpot spot in spots)
        {
            float distance = Vector3.Distance(mouse, spot.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                closest = spot;
            }
        }

        currentSpot = closest;

        if (currentSpot == null || currentSpot.Occupied || !TurretPlacer.Instance.HasSelectedTurret())
        {
            PreviewManager.Instance.HidePreview();
            return;
        }

        PreviewManager.Instance.ShowPreview(
            TurretPlacer.Instance.GetSelectedTurret(),
            currentSpot.transform.position);
    }

    void HandleInput()
    {
        TurretSpot spot = GetHoveredSpot();
        if (spot == null) return;

        if (Input.GetMouseButtonDown(1))
        {
            TurretPlacer.Instance.ClearSelection();
            UpgradeSell.Instance.Hide();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (spot.Occupied)
            {
                if (!TurretPlacer.Instance.HasSelectedTurret())
                {
                    UpgradeSell.Instance.Show(spot);
                }
                return;
            }

            if (!TurretPlacer.Instance.HasSelectedTurret()) return;

            GameObject prefab = TurretPlacer.Instance.GetSelectedTurret();
            if (prefab == null) return;

            Quaternion rot = PreviewManager.Instance.GetRotation();
            GameObject placedTurret = spot.PlaceTurret(prefab, rot);

            if (placedTurret != null)
            {
                if (placedTurret.GetComponent<LaserTurret>() != null)
                {
                    StartCoroutine(SetLaserDirection(placedTurret, rot));
                }
                else if (placedTurret.GetComponent<ShurikenTurret>() != null)
                {
                    StartCoroutine(SetShurikenAnimation(placedTurret));
                }
                else if (placedTurret.GetComponent<KatanaTurret>() != null)
                {
                    StartCoroutine(SetKatanaAnimation(placedTurret));
                }
            }

            TurretPlacer.Instance.ClearSelection();
        }
    }

    private IEnumerator SetLaserDirection(GameObject turret, Quaternion rot)
    {
        yield return null; 

        if (turret == null) yield break;

        Animator animator = turret.GetComponent<Animator>();
        LaserTurret laserScript = turret.GetComponent<LaserTurret>();

        if (animator != null)
        {
            turret.transform.rotation = Quaternion.identity;

            float zAngle = rot.eulerAngles.z;
            zAngle = Mathf.Round((zAngle % 360f + 360f) % 360f);

            int lvl = laserScript.laserLevel;
            string animBaseName = "";

            if (zAngle == 0f)        { animBaseName = "LaserFront"; }
            else if (zAngle == 90f)  { animBaseName = "LaserRight"; }
            else if (zAngle == 180f) { animBaseName = "LaserBack"; }
            else if (zAngle == 270f) { animBaseName = "LaserLeft"; }

            string finalAnimName = $"{animBaseName}{lvl}";
            animator.Play(finalAnimName);
        }
    }

    private IEnumerator SetShurikenAnimation(GameObject turret)
    {
        yield return null;
        if (turret == null) yield break;

        Animator animator = turret.GetComponent<Animator>();
        ShurikenTurret shurikenScript = turret.GetComponent<ShurikenTurret>();

        if (animator != null && shurikenScript != null)
        {
            string finalAnimName = $"ShurikenIdle{shurikenScript.shurikenLevel}";
            animator.Play(finalAnimName);
        }
    }

   private IEnumerator SetKatanaAnimation(GameObject turret)
    {
        yield return null;
        if (turret == null) yield break;

        Animator animator = turret.GetComponent<Animator>();
        KatanaTurret katanaScript = turret.GetComponent<KatanaTurret>();

        if (animator != null && katanaScript != null)
        {
            animator.SetInteger("Katanalvl", katanaScript.katanaLevel);
        }
    }

    public TurretSpot GetHoveredSpot()
    {
        return currentSpot;
    }
}