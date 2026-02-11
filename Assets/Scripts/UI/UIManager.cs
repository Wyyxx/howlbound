using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panel del Jugador")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI blockText;
    public TextMeshProUGUI energyText;
    public Slider playerHealthBar;

    [Header("Panel del Enemigo")]
    public Slider enemyHealthBar;
    public TextMeshProUGUI enemyHealthText;

    [Header("Cartas")]
    public GameObject cardPrefab;
    public Transform handContainer;

    [Header("Botones")]
    public Button endTurnButton;

    private List<GameObject> cardObjects = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        endTurnButton.onClick.AddListener(OnEndTurnPressed);
        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (Player.Instance != null)
        {
            healthText.text = $"HP: {Player.Instance.currentHealth}/{Player.Instance.maxHealth}";
            blockText.text = $"Block: {Player.Instance.currentBlock}";

            if (playerHealthBar != null)
            {
                playerHealthBar.maxValue = Player.Instance.maxHealth;
                playerHealthBar.value = Player.Instance.currentHealth;
            }
        }

        if (CombatManager.Instance != null)
        {
            energyText.text = $"Energía: {CombatManager.Instance.currentEnergy}/{CombatManager.Instance.maxEnergy}";
        }

        Enemy enemy = FindFirstObjectByType<Enemy>();
        if (enemy != null && enemyHealthBar != null)
        {
            enemyHealthBar.maxValue = enemy.maxHealth;
            enemyHealthBar.value = enemy.currentHealth;

            if (enemyHealthText != null)
                enemyHealthText.text = $"{enemy.currentHealth}/{enemy.maxHealth}";
        }

        UpdateHandUI();
    }

    void UpdateHandUI()
    {
        foreach (GameObject obj in cardObjects)
            Destroy(obj);
        cardObjects.Clear();

        foreach (CardData cardData in CombatManager.Instance.hand)
        {
            GameObject cardObj = Instantiate(cardPrefab, handContainer);
            Card card = cardObj.GetComponent<Card>();
            card.InitializeCard(cardData);

            Button btn = cardObj.GetComponent<Button>();
            if (btn == null)
                btn = cardObj.AddComponent<Button>();

            CardData capturedCard = cardData;
            btn.onClick.AddListener(() => OnCardClicked(capturedCard));

            cardObjects.Add(cardObj);
        }
    }

    void OnCardClicked(CardData cardData)
    {
        bool played = CombatManager.Instance.PlayCard(cardData);
        if (played)
        {
            Debug.Log($"Carta jugada: {cardData.cardName}");
        }
    }

    void OnEndTurnPressed()
    {
        CombatManager.Instance.EndPlayerTurn();
    }
}