using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WaveManagement : MonoBehaviour
{
    public List<GameObject> enemyNinjas = new List<GameObject>();

    [Header("Enemy Prefabs")]
    public GameObject basicPrefab;
    public GameObject blinkPrefab;
    public GameObject narutoPrefab;

    public Transform spawnPoint;

    [Header("Adjustable")]
    public int SetUpTime = 10;

    [Header("Level UI")]
    public Text LevelNum;
    public Text WaveNum;
    public TextMeshProUGUI coinGained;

    private TextMeshProUGUI coinSave;
    private GameObject GameTutorialUI;

    [System.Serializable]
    public struct WaveManager
    {
        public int basicCount;
        public int blinkCount;
        public int narutoCount;
    }

    [System.Serializable]
    public struct LevelManager
    {
        public List<WaveManager> waveList;
    }
    
    public List<LevelManager> levelList = new List<LevelManager>();
    public int level = 0;
    public bool isSettingUp;
    private int wave = 1;
    private bool isWin = false;
    [SerializeField]
    private SpriteRenderer[] PathArrows = new SpriteRenderer[3];

    void Start()
    {
        StartCoroutine(GameTutorial());
    }

    void Update()
    {
        coinSave = coinGained;
        enemyNinjas.RemoveAll(item => item == null);

        if (level == 4)
        {
            if (enemyNinjas.Count == 0 && !isWin)
            {
                isWin = true;
                SceneManager.LoadSceneAsync("Win Menu", LoadSceneMode.Additive);
                Time.timeScale = 0;
            }
        }
    }

    IEnumerator StartPhase()
    {
        yield return new WaitForSeconds(0.1f);
        isSettingUp = true;
        Debug.Log("SETTING UP PHASE...");
        level = 1;
        wave = 1;

        yield return StartCoroutine(LevelUi()); 
        StartCoroutine(spawnWave());
    }

    IEnumerator spawnWave()
    {
        for (int i = 0; i < levelList[level - 1].waveList[wave - 1].basicCount; i++)
        {
            spawnBasic();
            yield return new WaitForSeconds(2f);
        }
        for (int i = 0; i < levelList[level - 1].waveList[wave - 1].narutoCount; i++)
        {
            spawnNaruto();
            yield return new WaitForSeconds(2f);
        }
        for (int i = 0; i < levelList[level - 1].waveList[wave - 1].blinkCount; i++)
        {
            spawnBlink();
            yield return new WaitForSeconds(2f);
        }

        Debug.Log("Wave " + wave + " of Level " + level + " spawned! Waiting for clearance...");

        while (enemyNinjas.Count > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (wave == 3)
        {   
            Debug.Log("Level Complete! SETTING UP NEXT LEVEL...10 SECONDS");
            isSettingUp = true;
            level++;
            wave = 1;
            yield return StartCoroutine(LevelUi());
            nextLevel(); 
        }
        else
        {
            StartCoroutine(nextWave());
        }
    }

    IEnumerator nextWave()
    { 
        Debug.Log("Wave " + wave + " of Level " + level + " completed!");
        yield return new WaitForSeconds(3f);
        wave++;
        WaveNum.text = "Wave " + wave; 
        StartCoroutine(spawnWave());
    }

    void nextLevel()
    {
        StartCoroutine(spawnWave());
    }

    void spawnBasic()
    {
        var basic = Instantiate(basicPrefab, spawnPoint.position, Quaternion.identity);
        enemyNinjas.Add(basic);
    }

    void spawnBlink()
    {
        var blink = Instantiate(blinkPrefab, spawnPoint.position, Quaternion.identity);
        enemyNinjas.Add(blink);
    }

    void spawnNaruto()
    {
        var naruto = Instantiate(narutoPrefab, spawnPoint.position, Quaternion.identity);
        enemyNinjas.Add(naruto);
    }

    IEnumerator LevelUi()
    {
        LevelNum.text = "Prepare";
        for (int i = SetUpTime; i > 0; i--)
        {
            WaveNum.text = i.ToString();
    
            yield return new WaitForSeconds(1f);
        }
        isSettingUp = false;

        LevelNum.text = "Level " + level;
        WaveNum.text = "Wave " + wave; 
    }

    IEnumerator GameTutorial()
    {
            foreach(var arrow in PathArrows)
            {
                arrow.enabled = false;
            } 

        yield return new WaitForSeconds(1f);

        int count = 3;

        while (count > 0)
        {
            foreach(var arrow in PathArrows)
            {
                arrow.enabled = true;
                yield return new WaitForSeconds(0.2f);
            }

            foreach(var arrow in PathArrows)
            {
                arrow.enabled = false;
                yield return new WaitForSeconds(0.2f);
            } 

            count--;
        }

        foreach(var arrow in PathArrows)
            {
                arrow.enabled = false;
            } 
        StartCoroutine(StartPhase());
    }
}