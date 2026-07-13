using UnityEngine;

public class NarutoNinja : MonoBehaviour
{
    public float speed = 1f;
    public float Damage = 3f;

    [Header("Path Setup")]
    private GameObject[] pathPoints;

    private Vector3 p0, p1, p2, p3;
    private float t = 0f;
    private int nextControlIndex = 3;

    private SpriteRenderer spriteRenderer;
    private PlayerHealth playerHit;
    private GameObject playerHM;
    private EnemySFX sfx;

    // Hook to connect with your updated status effect script
    private EnemyHealthManager healthManager;

    void Start()
    {
        sfx = GetComponentInChildren<EnemySFX>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Cache the health manager script living on this enemy instance
        healthManager = GetComponent<EnemyHealthManager>();
        if (healthManager == null) healthManager = GetComponentInChildren<EnemyHealthManager>();

        playerHM = GameObject.FindWithTag("PlayerHealth");
        if (playerHM != null)
            playerHit = playerHM.GetComponent<PlayerHealth>();

        GameObject pathParent = GameObject.Find("Path Control");

        if (pathParent != null)
        {
            pathPoints = new GameObject[pathParent.transform.childCount];

            for (int i = 0; i < pathPoints.Length; i++)
            {
                Transform child = pathParent.transform.Find($"p{i}");

                if (child != null)
                {
                    pathPoints[i] = child.gameObject;
                }
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

    void Update()
    {
        if (pathPoints == null || pathPoints.Length < 4)
            return;

        // 1. STUN CHECK: Freeze movement instantly if stun status conditions are active
        if (healthManager != null && !healthManager.canMove)
        {
            return; // Stops frame delta evaluations and locks the ninja in its position
        }

        // 2. SLOW FACTOR CHECK: Read current tower movement speed modifiers
        float activeSlowFactor = (healthManager != null) ? healthManager.currentSlowFactor : 1f;

        // Progress path tracking factor scaled down by active status modifiers
        t += Time.deltaTime * speed * activeSlowFactor;
        t = Mathf.Clamp01(t);

        transform.position = CubicFast(p0, p1, p2, p3, t);

        if (t >= 1f)
        {
            if (nextControlIndex + 3 >= pathPoints.Length)
            {
                PlayerDeath();
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
    }

    public static Vector3 CubicFast(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;

        return u * u * u * p0 +
               3f * u * u * t * p1 +
               3f * u * t * t * p2 +
               t * t * t * p3;
    }

    void PlayerDeath()
    {
        if (sfx != null)
            sfx.DeathSFX();

        Debug.Log("PLAYER HIT");

        if (playerHit != null)
            playerHit.PlayerDamaged(Damage);

        Destroy(gameObject);
    }
}