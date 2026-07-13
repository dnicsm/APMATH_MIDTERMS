using UnityEngine;
using UnityEngine.UI;   

public class BlinkNinja : MonoBehaviour
{
    public float speed = 5f;
    public float BlinkInterval = 3f;
    public float BlinkChance = 0.10f;
    public float Damage;
    public bool isHit = false;

    [Header("Path Setup")]
    private GameObject[] pathPoints;

    private Vector3 p0, p1, p2, p3;
    private float t = 0f;
    private int nextControlIndex = 3;
    private float setSpeed;
    private Animator animator;
    EnemyHealthManager enemyHealth;
    private PlayerHealth playerHit;
    private GameObject playerHM;
    private EnemySFX sfx;

    void Start()
    {
        sfx = GetComponentInChildren<EnemySFX>();
        enemyHealth = GetComponentInChildren<EnemyHealthManager>();
        
        // Defensive check to make sure enemyHealth is cached if it lives on a parent/child object
        if (enemyHealth == null) enemyHealth = GetComponent<EnemyHealthManager>();

        playerHM = GameObject.FindWithTag("PlayerHealth");
        if (playerHM != null)
            playerHit = playerHM.GetComponent<PlayerHealth>();
            
        setSpeed = speed;
        animator = GetComponent<Animator>();
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

        InvokeRepeating("BlinkTimer", BlinkInterval, BlinkInterval);
    }

    void Update()
    {
        if (pathPoints == null || pathPoints.Length < 4)
            return;

        // 1. STUN & INTERRUPT INTERACTION: Freeze movement if stunned by the Bell Tower
        if (enemyHealth != null && !enemyHealth.canMove)
        {
            // If the Bell Tower hits while this ninja is mid-blink, interrupt it!
            if (enemyHealth.isBlinking)
            {
                CancelInvoke(nameof(Unblink)); // Stop the scheduled Unblink timing loop
                Unblink();                     // Forcefully exit the blink state early
                Debug.Log("Blink Skill Interrupted by Shockwave!");
            }
            return; // Stop processing path progression this frame
        }

        // 2. SLOW INTERACTION: Scale speed by the current slow factor applied from towers
        float activeSlowFactor = (enemyHealth != null) ? enemyHealth.currentSlowFactor : 1f;

        // Progress along the curve
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

    void Blink()
    {
        // Don't start a blink if we are currently stunned or slowed to zero speed
        if (enemyHealth != null && !enemyHealth.canMove) return;

        enemyHealth.isBlinking = true;
        var sprite = GetComponent<SpriteRenderer>();
        if (animator != null) animator.SetBool("Blink", true);
        
        t += 0.15f;
        Invoke(nameof(Unblink), 0.15f);
    }

    void Unblink()
    {
        if (enemyHealth != null) enemyHealth.isBlinking = false;
        speed = setSpeed;
        if (animator != null) animator.SetBool("Blink", false);
        var sprite = GetComponent<SpriteRenderer>();
    }
    
    void BlinkTimer()
    {
        if (Random.value < BlinkChance)
        {
            Debug.Log("Blinking!");
            Blink();
        }
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