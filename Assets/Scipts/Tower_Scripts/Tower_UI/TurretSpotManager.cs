using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TurretSpotManager : MonoBehaviour
{
    public static TurretSpotManager Instance;

    public List<TurretSpot> spots = new List<TurretSpot>();
    public float hoverDistance = 1f;

    private TurretSpot currentSpot;

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

            Quaternion rot = TurretPlacer.Instance.GetSelectedRotation();
            
            GameObject placedTurret = spot.PlaceTurret(prefab, rot);

            TurretPlacer.Instance.ClearSelection();
        }
    }

    public TurretSpot GetHoveredSpot()
    {
        return currentSpot;
    }
}