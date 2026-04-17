public class MinotaurBoss : Enemy
{
    void Awake()
    {
        isBoss = true;
    }

    protected override void PrepareNextIntention()
    {
        currentIntention = new EnemyIntention { type = IntentionType.Attack, value = 12 };
    }
}