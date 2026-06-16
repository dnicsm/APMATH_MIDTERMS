using System.Collections;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    [Header("Menu UI")]
    public Text title;
    public Button start;
    public Button exit;
    public GameObject menu;

    public AudioClip onClick;
    public AudioSource Audio;

    private Vector3 titlePos;
    private Vector3 startPos;
    private Vector3 exitPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        titlePos = title.transform.localPosition;
        startPos = start.transform.localPosition;
        exitPos = exit.transform.localPosition;

    }

    // Update is called once per frame
    void Update()
    {
        //use ease or smth para bouncy fr
        float movement = Mathf.PingPong(Time.time * 50f, 30f);
        float bounce = movement - 30f;
        title.transform.localPosition = titlePos + new Vector3(0, bounce, 0);
        start.transform.localPosition = startPos + new Vector3(0, bounce, 0);  
        exit.transform.localPosition = exitPos + new Vector3(0, bounce, 0);


    }

    public void GameStart()
    {
        StartCoroutine(ClickSFX());
        SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);
    }

    public void ExitGame()
    {
        StartCoroutine(ClickSFX());
        // UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

    IEnumerator ClickSFX()
    {
        Audio.clip = onClick;
        Audio.Play();
        yield return null;
    }
}
