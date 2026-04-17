using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class EncounterDef
{
    public string name;
    public GameObject[] enemiesToSpawn;
}

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("Configuración")]
    public int maxEnergy = 3;
    public int handSize = 5;
    public int maxPlayerActions = 3;

    [Header("Estado Actual")]
    public int currentEnergy;
    public int playerActionsLeft;

    [Header("Referencias")]
    public List<CardData> deck = new List<CardData>();
    public List<CardData> hand = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();

    [Header("Generación de Enemigos")]
    public Transform[] enemySpawnPoints;

    [Header("Formaciones Predefinidas")]
    public EncounterDef[] easyBattles;
    public EncounterDef[] mediumBattles;
    public EncounterDef[] bossBattles;

    private List<CardData> drawPile = new List<CardData>();

    public enum CombatState { Setup, PlayerTurn, EnemyTurn, Victory, Defeat }

    [Header("Estado Actual")]
    public CombatState currentState;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        InitializeDeck();
        SpawnEnemies();
        StartPlayerTurn();
    }

    void InitializeDeck()
    {
        if (PlayerRunData.Instance != null && PlayerRunData.Instance.masterDeck.Count > 0)
        {
            drawPile = new List<CardData>(PlayerRunData.Instance.masterDeck);
            Debug.Log($"Mazo cargado desde PlayerRunData con {drawPile.Count} cartas");
        }
        else
        {
            drawPile = new List<CardData>(deck);
            Debug.LogWarning("Usando mazo de respaldo del CombatManager.");
        }

        ShuffleDeck(drawPile);
    }

    void ShuffleDeck(List<CardData> deckToShuffle)
    {
        for (int i = 0; i < deckToShuffle.Count; i++)
        {
            int randomIndex = Random.Range(i, deckToShuffle.Count);
            CardData temp = deckToShuffle[i];
            deckToShuffle[i] = deckToShuffle[randomIndex];
            deckToShuffle[randomIndex] = temp;
        }
    }

    public void StartPlayerTurn()
    {
        if (currentState == CombatState.Defeat || currentState == CombatState.Victory) return;
        currentState = CombatState.PlayerTurn;
        currentEnergy = maxEnergy;
        playerActionsLeft = maxPlayerActions;
        Player.Instance.currentBlock = 0;

        DrawCards(handSize);
        Debug.Log($"--- Turno del Jugador | Acciones: {playerActionsLeft} ---");
    }

    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (drawPile.Count == 0)
            {
                if (discardPile.Count == 0) return;
                drawPile = new List<CardData>(discardPile);
                discardPile.Clear();
                ShuffleDeck(drawPile);
                Debug.Log("Mazo rebarajado!");
            }

            CardData card = drawPile[0];
            drawPile.RemoveAt(0);
            hand.Add(card);
        }
    }

    public void EndPlayerTurn()
    {
        if (currentState != CombatState.PlayerTurn) return;

        currentState = CombatState.EnemyTurn;

        discardPile.AddRange(hand);
        hand.Clear();

        Debug.Log("--- Turno del Enemigo ---");
        StartEnemyTurn();
    }

    void StartEnemyTurn()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;

            enemy.ProcessStartOfTurnEffects();

            CheckForVictory();
            if (currentState == CombatState.Victory) return;

            if (enemy.currentHealth > 0)
                enemy.PerformAction();
        }

        if (currentState != CombatState.Victory)
            StartPlayerTurn();
    }

    // ── MODIFICADO: detecta si había un jefe en la pelea ──────────────
    public void CheckForVictory()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        bool allDead = true;
        bool wasBossFight = false;

        foreach (Enemy e in enemies)
        {
            if (e != null && e.isBoss) wasBossFight = true;

            if (e != null && e.currentHealth > 0)
            {
                allDead = false;
                break;
            }
        }

        if (allDead && currentState != CombatState.Victory)
            WinCombat(wasBossFight);
    }

    void EndCombatAndReturnToMap()
    {
        if (MapManager.Instance != null)
            MapManager.Instance.ReturnToMap("CombatScene");
        else
        {
            Debug.LogError("No se encontró el MapManager.");
            SceneManager.LoadScene("Mapa");
        }
    }

    void SpawnEnemies()
    {
        NodeType encounterType = NodeType.Battle;
        if (PlayerRunData.Instance != null)
            encounterType = PlayerRunData.Instance.currentEncounterType;

        EncounterDef selectedEncounter = null;

        if (encounterType == NodeType.Boss)
            selectedEncounter = bossBattles[Random.Range(0, bossBattles.Length)];
        else if (encounterType == NodeType.MiniBoss)
            selectedEncounter = mediumBattles[Random.Range(0, mediumBattles.Length)];
        else
            selectedEncounter = easyBattles[Random.Range(0, easyBattles.Length)];

        if (selectedEncounter != null)
        {
            Debug.Log($"Iniciando encuentro: {selectedEncounter.name}");

            for (int i = 0; i < selectedEncounter.enemiesToSpawn.Length; i++)
            {
                if (i < enemySpawnPoints.Length)
                    Instantiate(selectedEncounter.enemiesToSpawn[i], enemySpawnPoints[i].position, Quaternion.identity, enemySpawnPoints[i]);
            }
        }
    }

    public bool TryPlayCard(Card cardScript, Enemy target)
    {
        if (currentState != CombatState.PlayerTurn) return false;

        CardData data = cardScript.cardData;

        if (currentEnergy < data.energyCost || playerActionsLeft <= 0)
        {
            Debug.Log("No tienes energía o acciones suficientes.");
            return false;
        }

        if (data.damageAmount > 0 && !data.isAoE && target == null)
        {
            Debug.Log("Esta carta requiere un objetivo. Arrástrala sobre un enemigo.");
            return false;
        }

        currentEnergy -= data.energyCost;
        playerActionsLeft--;

        ApplyCardEffect(data, target);

        int exactIndex = hand.LastIndexOf(data);
        if (exactIndex != -1)
            hand.RemoveAt(exactIndex);

        discardPile.Add(data);
        Destroy(cardScript.gameObject);

        return true;
    }

    void ApplyCardEffect(CardData card, Enemy target)
    {
        if (card.damageAmount > 0)
        {
            if (card.isAoE)
            {
                Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
                foreach (Enemy e in allEnemies)
                {
                    if (e != null && e.currentHealth > 0)
                        ProcessDamageAndLifesteal(card, e);
                }
                Debug.Log($"{card.cardName} hizo Daño de Área.");
            }
            else if (target != null)
            {
                ProcessDamageAndLifesteal(card, target);
            }
        }

        if (card.blockAmount > 0) Player.Instance.GainBlock(card.blockAmount);
        if (card.drawAmount > 0) DrawCards(card.drawAmount);

        if (card.poisonAmount > 0 && card.poisonDuration > 0 && target != null)
        {
            target.ApplyPoison(card.poisonAmount, card.poisonDuration);
            Debug.Log($"Envenenado por {card.poisonAmount} daño durante {card.poisonDuration} turnos.");
        }

        CheckForVictory();
    }

    void ProcessDamageAndLifesteal(CardData card, Enemy e)
    {
        int actualDamageDealt = Mathf.Min(card.damageAmount, e.currentHealth + e.currentBlock);
        e.TakeDamage(card.damageAmount);

        if (card.lifestealPercentage > 0)
        {
            int healAmount = Mathf.FloorToInt(actualDamageDealt * card.lifestealPercentage);
            if (healAmount > 0)
            {
                Player.Instance.currentHealth = Mathf.Min(Player.Instance.currentHealth + healAmount, Player.Instance.maxHealth);
                Debug.Log($"Robaste {healAmount} de HP.");
            }
        }
    }

    // ── MODIFICADO: recibe si era pelea de jefe ────────────────────────
    public void WinCombat(bool isBossFight = false)
    {
        currentState = CombatState.Victory;
        Debug.Log("<color=green>¡Victoria! Todos los enemigos derrotados.</color>");

        if (isBossFight)
        {
            if (BossWinScreenManager.Instance != null)
                BossWinScreenManager.Instance.ShowBossWinScreen();
            else
            {
                Debug.LogWarning("No hay BossWinScreenManager. Regresando al mapa.");
                EndCombatAndReturnToMap();
            }
            return;
        }

        int goldEarned = Random.Range(15, 31);

        if (RewardScreenManager.Instance != null)
            RewardScreenManager.Instance.ShowRewards(goldEarned);
        else
        {
            Debug.LogWarning("No hay RewardScreenManager. Regresando al mapa.");
            EndCombatAndReturnToMap();
        }
    }

    // ── SIN CAMBIOS ────────────────────────────────────────────────────
    public void LoseCombat()
    {
        if (currentState == CombatState.Defeat) return;

        currentState = CombatState.Defeat;
        Debug.Log("<color=red>El jugador ha muerto. Transición a pantalla de muerte.</color>");

        if (DeathScreenManager.Instance != null)
            DeathScreenManager.Instance.ShowDeathScreen();
    }
}