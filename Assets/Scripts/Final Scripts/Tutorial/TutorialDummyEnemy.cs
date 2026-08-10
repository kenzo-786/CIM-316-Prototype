using UnityEngine;

public class TutorialDummyEnemy : EnemyBase
{
    protected override void TickEnemy()
    {
        StopMoving();
    }
}
