using UnityEngine;
using System.Collections.Generic;

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
    public bool isPlayerTurn = false;

    [Header("Referencias")]
    public List<CardData> deck = new List<CardData>();
    public List<CardData> hand = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();
    public List<CardData> selectedCards = new List<CardData>();

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
        playerActionsLeft = maxPlayerActions;
        Player.Instance.currentBlock = 0;
        selectedCards.Clear();
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

    public void SelectCard(CardData card)
    {
        if (!selectedCards.Contains(card))
        {
            selectedCards.Add(card);
            currentEnergy -= card.energyCost;
            Debug.Log($"Carta seleccionada: {card.cardName}");
        }
    }

    public void DeselectCard(CardData card)
    {
        if (selectedCards.Contains(card))
        {
            selectedCards.Remove(card);
            currentEnergy += card.energyCost;
            Debug.Log($"Carta deseleccionada: {card.cardName}");
        }
    }

    public int GetSelectedCount()
    {
        return selectedCards.Count;
    }

    public void ConfirmSelectedCard()
    {
        if (selectedCards.Count == 0)
        {
            Debug.Log("No hay carta seleccionada!");
            return;
        }

        CardData card = selectedCards[0];

        // Aplicar efecto
        ApplyCardEffect(card);

        // Sacar de la mano y descartar
        hand.Remove(card);
        discardPile.Add(card);
        selectedCards.Clear();

        // Restar acción
        playerActionsLeft--;

        // Robar carta nueva para reemplazar
        DrawCards(1);

        Debug.Log($"Acción usada! Acciones restantes: {playerActionsLeft}");

        // Si se acabaron las acciones, terminar turno automáticamente
        if (playerActionsLeft <= 0)
        {
            Debug.Log("Sin acciones! Turno del enemigo.");
            EndPlayerTurn();
        }
    }

    void ApplyCardEffect(CardData card)
    {
        Enemy enemy = FindFirstObjectByType<Enemy>();

        if (card.damageAmount > 0 && enemy != null)
        {
            enemy.TakeDamage(card.damageAmount);
            Debug.Log($"{card.cardName}: {card.damageAmount} daño!");
        }

        if (card.blockAmount > 0)
        {
            Player.Instance.GainBlock(card.blockAmount);
            Debug.Log($"{card.cardName}: {card.blockAmount} bloqueo!");
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
        selectedCards.Clear();

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