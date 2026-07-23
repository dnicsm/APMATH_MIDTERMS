using UnityEngine;
using System.Collections;

public enum BossPhase
{
    Phase1_Form,
    Phase2_Enraged,
    Phase3_Overload
}

public class ArashiBoss : MonoBehaviour
{
    [Header("Boss Multi-Layer Health System")]
    public BossPhase currentPhase = BossPhase.Phase1_Form;
    
    public float[] phaseMaxHealth = new float[3] { 500f, 800f, 1200f };
    public float currentPhaseHealth;
    public int currentPhaseIndex = 0;

    [Header("Movement Stats")]
    public float[] phaseSpeeds = new float[3] { 0.3f, 0.5f, 0.8f };
    public float damageToPlayer = 20f;

    [Header("Blast Effect Settings")]
    public float blastRadius = 6.0f;
    public LayerMask trapLayer;

    private GameObject[] pathPoints;
    private Vector3 p0, p1, p2, p3;
    private float t = 0f;
    private int nextControlIndex = 3;

    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private Color originalCamColor;
    private EnemyHealthManager healthManager;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        healthManager = GetComponent<EnemyHealthManager>();
        mainCamera = Camera.main;
        if (mainCamera != null) originalCamColor = mainCamera.backgroundColor;

        currentPhaseIndex = 0;
        currentPhaseHealth = phaseMaxHealth[currentPhaseIndex];

        InitializePath();
        ApplyPhaseState();
    }

    void Update()
    {

        bool canMove = (healthManager != null) ? healthManager.canMove : true;
        float slowFactor = (healthManager != null) ? healthManager.currentSlowFactor : 1f;

        if (canMove && pathPoints != null && pathPoints.Length >= 4)
        {
            float currentSpeed = (currentPhaseIndex < phaseSpeeds.Length) ? phaseSpeeds[currentPhaseIndex] : phaseSpeeds[0];
            t += Time.deltaTime * (currentSpeed * slowFactor);
            t = Mathf.Clamp01(t);
            transform.position = EnemyController.CubicFast(p0, p1, p2, p3, t);

            if (t >= 1f)
            {
                AdvancePath();
            }
        }
    }

    public void TakeDamage(float amount)
    {
        currentPhaseHealth -= amount;

        if (currentPhaseHealth <= 0f)
        {
            AdvanceBossPhase();
        }
    }

    private void AdvanceBossPhase()
    {
        currentPhaseIndex++;

        if (currentPhaseIndex >= phaseMaxHealth.Length)
        {
            currentPhaseHealth = 0f;
            return;
        }

        currentPhaseHealth = phaseMaxHealth[currentPhaseIndex];
        currentPhase = (BossPhase)currentPhaseIndex;

        StartCoroutine(TransformationBlastRoutine());
        ApplyPhaseState();

        if (healthManager != null)
        {
            healthManager.MaxHealth = phaseMaxHealth[currentPhaseIndex];
            healthManager.CurrentHealth = currentPhaseHealth;
            healthManager.UpdateSpriteColor();
        }
    }

    public Color GetPhaseColor()
    {
        switch (currentPhase)
        {
            case BossPhase.Phase2_Enraged:
                return new Color(1f, 0.3f, 0.3f, 1f);
            case BossPhase.Phase3_Overload:
                return new Color(1f, 0.9f, 0.2f, 1f);
            case BossPhase.Phase1_Form:
            default:
                return Color.white;
        }
    }

    private void ApplyPhaseState()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = GetPhaseColor();
        }
    }

    private IEnumerator TransformationBlastRoutine()
    {
        Collider2D[] trappedObjects = Physics2D.OverlapCircleAll(transform.position, blastRadius, trapLayer);
        foreach (Collider2D trap in trappedObjects)
        {
            Destroy(trap.gameObject);
        }

        if (mainCamera != null)
        {
            mainCamera.backgroundColor = Color.white;
            yield return new WaitForSeconds(0.15f);
            mainCamera.backgroundColor = Color.red;
            yield return new WaitForSeconds(0.15f);
            mainCamera.backgroundColor = originalCamColor;
        }
    }

    #region Path Logic
    private void InitializePath()
    {
        GameObject pathParent = GameObject.Find("Path Control");
        if (pathParent != null)
        {
            pathPoints = new GameObject[pathParent.transform.childCount];
            for (int i = 0; i < pathPoints.Length; i++)
            {
                Transform child = pathParent.transform.Find($"p{i}");
                if (child != null) pathPoints[i] = child.gameObject;
            }
        }

        if (pathPoints != null && pathPoints.Length >= 4)
        {
            p0 = pathPoints[0].transform.position;
            p1 = pathPoints[1].transform.position;
            p2 = pathPoints[2].transform.position;
            p3 = pathPoints[3].transform.position;
            transform.position = p0;
        }
    }

    private void AdvancePath()
    {
        if (nextControlIndex + 3 >= pathPoints.Length)
        {
            GameObject playerHM = GameObject.FindWithTag("PlayerHealth");
            if (playerHM != null)
            {
                PlayerHealth ph = playerHM.GetComponent<PlayerHealth>();
                if (ph != null) ph.PlayerDamaged(damageToPlayer);
            }
            Destroy(gameObject);
        }
        else
        {
            t = 0f;
            p0 = p3;
            p1 = pathPoints[nextControlIndex + 1].transform.position;
            p2 = pathPoints[nextControlIndex + 2].transform.position;
            p3 = pathPoints[nextControlIndex + 3].transform.position;
            nextControlIndex += 3;
        }
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, blastRadius);
    }
}