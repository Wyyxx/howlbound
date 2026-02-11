using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("Configuración")]
    public int maxEnergy = 3;
    public int handSize = 5;

    [Header("Estado Actual")]
    public int currentEnergy;
    public bool isPlayerTurn = false;

    [Header("Referencias")]
    public List<CardData> deck = new List<CardData>();
    public List<CardData> hand = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();

    private List<CardData> drawPile = new List<CardData>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        InitializeDeck();
        StartPlayerTurn();
    }

    void InitializeDeck()
    {
        // Copiar el mazo al pile de robo y barajarlo
        drawPile = new List<CardData>(deck);
        ShuffleDeck(drawPile);
        Debug.Log($"Mazo inicializado con {drawPile.Count} cartas");
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
        isPlayerTurn = true;
        currentEnergy = maxEnergy;
        Player.Instance.currentBlock = 0;
        DrawCards(handSize);
        Debug.Log("--- Turno del Jugador ---");
    }

    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            // Si no hay cartas en el pile, barajar el descarte
            if (drawPile.Count == 0)
            {
                if (discardPile.Count == 0)
                {
                    Debug.Log("No hay mas cartas!");
                    return;
                }
                drawPile = new List<CardData>(discardPile);
                discardPile.Clear();
                ShuffleDeck(drawPile);
                Debug.Log("Mazo rebarajado!");
            }

            CardData card = drawPile[0];
            drawPile.RemoveAt(0);
            hand.Add(card);
            Debug.Log($"Robaste: {card.cardName}");
        }
    }

    public bool PlayCard(CardData card)
    {
        if (!isPlayerTurn) return false;
        if (!hand.Contains(card)) return false;
        if (currentEnergy < card.energyCost)
        {
            Debug.Log("No tienes suficiente energía!");
            return false;
        }

        // Gastar energía
        currentEnergy -= card.energyCost;

        // Aplicar efectos
        ApplyCardEffect(card);

        // Mover carta al descarte
        hand.Remove(card);
        discardPile.Add(card);

        return true;
    }

    void ApplyCardEffect(CardData card)
    {
        Enemy enemy = FindFirstObjectByType<Enemy>();

        if (card.damageAmount > 0 && enemy != null)
        {
            enemy.TakeDamage(card.damageAmount);
            Debug.Log($"Causaste {card.damageAmount} de daño!");
        }

        if (card.blockAmount > 0)
        {
            Player.Instance.GainBlock(card.blockAmount);
            Debug.Log($"Ganaste {card.blockAmount} de bloqueo!");
        }

        if (card.drawAmount > 0)
        {
            DrawCards(card.drawAmount);
        }
    }

    public void EndPlayerTurn()
    {
        if (!isPlayerTurn) return;

        isPlayerTurn = false;

        // Descartar mano
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
            enemy.PerformAction();
        }

        StartPlayerTurn();
    }
}