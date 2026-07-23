using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WaveManagement : MonoBehaviour
{
    public List<GameObject> enemyNinjas = new List<GameObject>();

    [Header("Enemy Prefabs")]
    public GameObject smokeBomberPrefab;
    public GameObject saboteurPrefab;
    public GameObject shadowNinjaPrefab;
    public GameObject echoShinobiPrefab;
    public GameObject arashiBossPrefab;

    public Transform spawnPoint;

    [Header("Adjustable")]
    public int SetUpTime = 10;
    public float timeBetweenSpawns = 2f;

    [Header("Level UI")]
    public Text LevelNum;
    public Text WaveNum;
    public TextMeshProUGUI coinGained;

    private TextMeshProUGUI coinSave;
    private GameObject GameTutorialUI;

    [System.Serializable]
    public struct WaveManager
    {
        public int smokeBomberCount;
        public int saboteurCount;
        public int shadowNinjaCount;
        public int echoShinobiCount;
        public int arashiBossCount;
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
        WaveManager currentWaveData = levelList[level - 1].waveList[wave - 1];

        for (int i = 0; i < currentWaveData.smokeBomberCount; i++)
        {
            SpawnEnemy(smokeBomberPrefab);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        for (int i = 0; i < currentWaveData.saboteurCount; i++)
        {
            SpawnEnemy(saboteurPrefab);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        for (int i = 0; i < currentWaveData.shadowNinjaCount; i++)
        {
            SpawnEnemy(shadowNinjaPrefab);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        for (int i = 0; i < currentWaveData.echoShinobiCount; i++)
        {
            SpawnEnemy(echoShinobiPrefab);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        for (int i = 0; i < currentWaveData.arashiBossCount; i++)
        {
            SpawnEnemy(arashiBossPrefab);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        Debug.Log($"Wave {wave} of Level {level} spawned! Waiting for clearance...");

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
        Debug.Log($"Wave {wave} of Level {level} completed!");
        yield return new WaitForSeconds(3f);
        wave++;
        if (WaveNum != null) WaveNum.text = "Wave " + wave;
        StartCoroutine(spawnWave());
    }

    void nextLevel()
    {
        StartCoroutine(spawnWave());
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab != null && spawnPoint != null)
        {
            GameObject enemyInstance = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            enemyNinjas.Add(enemyInstance);
        }
    }

    IEnumerator LevelUi()
    {
        if (LevelNum != null) LevelNum.text = "Prepare";

        for (int i = SetUpTime; i > 0; i--)
        {
            if (WaveNum != null) WaveNum.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        isSettingUp = false;

        if (LevelNum != null) LevelNum.text = "Level " + level;
        if (WaveNum != null) WaveNum.text = "Wave " + wave;
    }

    IEnumerator GameTutorial()
    {
        foreach (var arrow in PathArrows)
        {
            if (arrow != null) arrow.enabled = false;
        }

        yield return new WaitForSeconds(1f);

        int count = 3;

        while (count > 0)
        {
            foreach (var arrow in PathArrows)
            {
                if (arrow != null) arrow.enabled = true;
            }
            yield return new WaitForSeconds(0.2f);

            foreach (var arrow in PathArrows)
            {
                if (arrow != null) arrow.enabled = false;
            }
            yield return new WaitForSeconds(0.2f);

            count--;
        }

        foreach (var arrow in PathArrows)
        {
            if (arrow != null) arrow.enabled = false;
        }

        StartCoroutine(StartPhase());
    }
}