using UnityEngine;

public class Player : Character
{
    public static Player Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        maxHealth = 100;
        currentHealth = maxHealth;
        currentBlock = 0;
    }

    protected override void OnHealthChanged()
    {
        Debug.Log($"Jugador - HP: {currentHealth}/{maxHealth} | Block: {currentBlock}");
    }

    protected override void Die()
    {
        Debug.Log("Muriste!");
    }
}