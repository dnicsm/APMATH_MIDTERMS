using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public MenuManager menu;
    public Text coinAmount, title;

    public GameObject currentCoins;

    public AudioClip onClick;
    public AudioSource Audio;

    private Vector3 coin, header;
    void Start()
    {
        
        StartCoroutine(Expand());
        menu = Object.FindAnyObjectByType<MenuManager>();
        coin = coinAmount.transform.localPosition;
        header = title.transform.localPosition;      
        StartCoroutine(getCoin());  
    }

    void Update()
    {
        float movement = Mathf.PingPong(Time.unscaledTime * 50f, 30f);
        float bounce = movement - 30f;        
        title.transform.localPosition = header + new Vector3(0, bounce, 0);  

    }
    public void Resume()
    {
        StartCoroutine(ClickSFX());
        Debug.Log("RESUMED");
        if (SceneManager.GetSceneByName("Pause Menu").isLoaded)
        {
            menu.isPaused = false;
        }
        Time.timeScale = 1f;
        StartCoroutine(Shrink());

    }

    public void Restart()
    {
        StartCoroutine(ClickSFX());
        Debug.Log("RESTARTED");

        if (SceneManager.GetSceneByName("Pause Menu").isLoaded)
        {
            menu.isPaused = false;
        }

        Time.timeScale = 1f;
        StartCoroutine(Shrink());
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }

    public void Exit()
    {
        StartCoroutine(ClickSFX());
        Debug.Log("EXITED");
        if (SceneManager.GetSceneByName("Pause Menu").isLoaded)
        {
            menu.isPaused = false;
        }
        Time.timeScale = 1f;
        StartCoroutine(Shrink());
        SceneManager.LoadScene("Game Menu", LoadSceneMode.Single);
    }

    IEnumerator Shrink()
    {
        float counter = 0;
        
    
        while(counter < 1f)
        {
            float shrink = Mathf.Lerp(1f, 0, counter);
            transform.localScale = new Vector3(shrink, shrink, 1);

            counter += Time.unscaledDeltaTime * 5f;
            yield return null;
        }

        SceneManager.UnloadSceneAsync("Pause Menu");

    }

    IEnumerator Expand()
    {
        float counter = 0;
        
    
        while(counter < 1f)
        {
            float expand = Mathf.Lerp(0, 1f, counter);
            transform.localScale = new Vector3(expand, expand, 1);

            counter += Time.unscaledDeltaTime * 5f;
            yield return null;
        }
        
        transform.localScale = new Vector3(1, 1, 1);

    }

    IEnumerator ClickSFX()
    {
        Audio.clip = onClick;
        Audio.Play();
        yield return null;
    }

    IEnumerator getCoin()
{
    yield return null;
    
    currentCoins = GameObject.FindWithTag("Coin");
    
    if (currentCoins != null)
    {
        TextMeshProUGUI tmpText = currentCoins.GetComponentInChildren<TextMeshProUGUI>();

        if (tmpText != null)
        {
            coinAmount.text = tmpText.text;
        }

        if (tmpText = null)
            {
                Debug.Log("NO TEXT BROO");
            }
    }
}
    
}
