using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health UI")]
    public Image GhostBar;
    public Image HealthBar;
    public Image SlideGhost;
    public Text HpText;

    [Header("Player Health Audio")]
    public AudioSource PlayerAudio;

    [Header("Camera")]
    public Camera mainCamera;

    private float MaxHealth = 100f;
    private bool isDead = false;
    
    [SerializeField] private float CurrentHealth;

    void Start()
    {
        CurrentHealth = MaxHealth;
        UpdateHpText();
    }

    void Update()
    {
        if (CurrentHealth <= 0 && !isDead)
        {
            isDead = true;
            CurrentHealth = 0;
            UpdateHpText();
            Time.timeScale = 0f;
            SceneManager.LoadSceneAsync("Lose Menu", LoadSceneMode.Additive);
        }
    }

    public void PlayerDamaged(float damage)
    {
        if (isDead) return;

        CurrentHealth -= damage;
        if (CurrentHealth <= 0) CurrentHealth = 0;

        StopAllCoroutines();
        StartCoroutine(HitAnimation());
    }

    IEnumerator HitAnimation()
    {
        PlayerAudio.Play();
        float startGhostFill = SlideGhost.fillAmount;
        
        float targetFill = CurrentHealth / MaxHealth;

        HealthBar.fillAmount = targetFill;
        HealthBar.color = Color.red;
        UpdateHpText();

        float counter = 0;
    
        while (counter < 1f)
        {
            counter += Time.deltaTime * 15f;
            float currentAngle = Mathf.Lerp(0f, 3f, Mathf.Clamp01(counter));
            
            GhostBar.transform.localEulerAngles = new Vector3(0, 0, currentAngle);
            if (mainCamera != null) mainCamera.transform.localEulerAngles = new Vector3(0, 0, -currentAngle);
            yield return null;
        }

        counter = 0;
        while (counter < 1f)
        {
            counter += Time.deltaTime * 15f;
            float currentAngle = Mathf.Lerp(3f, -3f, Mathf.Clamp01(counter));
            
            GhostBar.transform.localEulerAngles = new Vector3(0, 0, currentAngle);
            if (mainCamera != null) mainCamera.transform.localEulerAngles = new Vector3(0, 0, -currentAngle);
            yield return null;
        }

        counter = 0;
        while (counter < 1f)
        {
            counter += Time.deltaTime * 15f;
            float currentAngle = Mathf.Lerp(-3f, 0f, Mathf.Clamp01(counter));
            
            GhostBar.transform.localEulerAngles = new Vector3(0, 0, currentAngle);
            if (mainCamera != null) mainCamera.transform.localEulerAngles = new Vector3(0, 0, -currentAngle);
            yield return null;
        }

        GhostBar.transform.localEulerAngles = Vector3.zero;
        if (mainCamera != null) mainCamera.transform.localEulerAngles = Vector3.zero;
        HealthBar.color = Color.white;

        float slide = 0;
        while (slide < 1f)
        {
            slide += Time.deltaTime * 3f;
            SlideGhost.fillAmount = Mathf.Lerp(startGhostFill, targetFill, Mathf.Clamp01(slide));
            yield return null;
        }
    }

    void UpdateHpText()
    {
        if (HpText != null)
        {
            HpText.text = Mathf.Round(CurrentHealth).ToString() + " / " + Mathf.Round(MaxHealth).ToString();
        }
    }
}