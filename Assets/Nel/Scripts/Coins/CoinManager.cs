using TMPro;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public TMP_Text coinText;
    public Transform coinTarget;
    public int startingCoins = 50;
    public int totalCoins;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        totalCoins = startingCoins;
        UpdateUI();
    }

    public void AddCoins(int amount)
    {
        totalCoins += amount;
        UpdateUI();
    }

    public bool SpendCoins(int amount)
    {
        if (totalCoins < amount)
            return false;

        totalCoins -= amount;
        UpdateUI();

        return true;
    }

    void UpdateUI()
    {
        coinText.text = totalCoins.ToString();
    }

    public int GetCoins()
    {
        return totalCoins;
    }
}
