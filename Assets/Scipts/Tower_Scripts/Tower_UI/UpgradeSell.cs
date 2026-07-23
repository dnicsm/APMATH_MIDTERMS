using UnityEngine;

public class UpgradeSell : MonoBehaviour
{
    public static UpgradeSell Instance;

    public GameObject panel;
    public Vector3 panelOffset = new Vector3(0f, 1.5f, 0f);

    private TurretSpot selectedSpot;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Hide();
    }

    private void Update()
    {
        if (selectedSpot == null)
            return;

        panel.transform.position =
            selectedSpot.transform.position +
            panelOffset;
    }

    public void Show(TurretSpot spot)
    {
        selectedSpot = spot;

        panel.SetActive(true);

        panel.transform.position =
            spot.transform.position +
            panelOffset;

    }

    public void Hide()
    {
        panel.SetActive(false);

        selectedSpot = null;
    }

    public TurretSpot GetSelectedSpot()
    {
        return selectedSpot;
    }
}