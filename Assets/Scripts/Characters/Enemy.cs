using UnityEngine;

public class Enemy : Character
{
    [Header("Enemy Info")]
    public string enemyName;
    public EnemyIntention currentIntention;

    void Start()
    {
        maxHealth = 50;
        currentHealth = maxHealth;
        currentBlock = 0;
        PrepareNextIntention();
    }

    public void PerformAction()
    {
        switch (currentIntention.type)
        {
            case IntentionType.Attack:
                Debug.Log($"{enemyName} ataca por {currentIntention.value}");
                Player.Instance.TakeDamage(currentIntention.value);
                break;

            case IntentionType.Defend:
                Debug.Log($"{enemyName} se defiende por {currentIntention.value}");
                GainBlock(currentIntention.value);
                break;
        }

        PrepareNextIntention();
    }

    void PrepareNextIntention()
    {
        // Por ahora siempre ataca por 10
        currentIntention = new EnemyIntention
        {
            type = IntentionType.Attack,
            value = 10
        };
    }

    protected override void OnHealthChanged()
    {
        Debug.Log($"{enemyName} - HP: {currentHealth}/{maxHealth} | Block: {currentBlock}");
    }

    protected override void Die()
    {
        Debug.Log($"{enemyName} murio!");
        Destroy(gameObject);
    }
}