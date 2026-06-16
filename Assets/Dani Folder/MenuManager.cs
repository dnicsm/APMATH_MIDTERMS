using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;  

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    public Scene Pause;
    public bool isPaused = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Scene Main = SceneManager.GetSceneByName("Main");
        Pause = SceneManager.GetSceneByName("Pause Menu");
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Main"))
        {
            if (Input.GetButtonDown("Cancel"))
            {
                if (!isPaused)
                {
                    isPaused = true;
                    Time.timeScale = 0;
                    SceneManager.LoadScene("Pause Menu", LoadSceneMode.Additive);
                }
            }
        }
    }

    public void pauseButton()
    {
        if (!isPaused)
                {
                    isPaused = true;
                    Time.timeScale = 0;
                    SceneManager.LoadScene("Pause Menu", LoadSceneMode.Additive);
                }
    }
}
