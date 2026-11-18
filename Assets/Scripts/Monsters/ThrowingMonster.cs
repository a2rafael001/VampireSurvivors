using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class ThrowingMonster : Monster
    {
        public enum State
        {
            Walking,
            Shooting
        }

        [SerializeField] protected Transform throwableSpawnPosition;
        
        protected ThrowingMonsterBlueprint ThrowingBlueprint => (ThrowingMonsterBlueprint)monsterBlueprint;
        
        protected float timeSinceLastAttack;
        protected State state;
        protected float outOfRangeTime;
        protected int throwableIndex;

        public override void Setup(int monsterIndex, Vector2 position, MonsterBlueprint monsterBlueprint, float hpBuff = 0)
        {
            base.Setup(monsterIndex, position, monsterBlueprint, hpBuff);
            throwableIndex = entityManager.AddPoolForThrowable(ThrowingBlueprint.throwablePrefab);
            outOfRangeTime = 0;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (alive)
            {
                Vector2 toPlayer = (playerCharacter.transform.position - transform.position);
                float distance = toPlayer.magnitude;
                Vector2 dirToPlayer = toPlayer / distance;

                switch (state)
                {
                    case State.Walking:
                        rb.linearVelocity += dirToPlayer * ThrowingBlueprint.acceleration * Time.fixedDeltaTime;
                        if (distance <= ThrowingBlueprint.range)
                        {
                            state = State.Shooting;
                            monsterSpriteAnimator.StopAnimating();
                        }
                        break;

                    case State.Shooting:
                        timeSinceLastAttack += Time.fixedDeltaTime;
                        if (timeSinceLastAttack >= 1.0f / ThrowingBlueprint.atkspeed)
                        {
                            LaunchThrowable(playerCharacter.transform.position);
                            timeSinceLastAttack = 0;
                        }
                        if (distance <= ThrowingBlueprint.range)
                            outOfRangeTime = 0;
                        else
                            outOfRangeTime += Time.deltaTime;
                        if (outOfRangeTime > ThrowingBlueprint.timeAllowedOutsideRange)
                        {
                            state = State.Walking;
                            monsterSpriteAnimator.StartAnimating();
                        }
                        break;
                }
            }
        }

        protected void LaunchThrowable(Vector2 targetPosition)
        {
            Throwable throwable = entityManager.SpawnThrowable(
                throwableIndex, 
                throwableSpawnPosition.position, 
                ThrowingBlueprint.atk, 
                0, 
                -909, 
                ThrowingBlueprint.targetLayer
            );
            targetPosition += playerCharacter.Velocity * throwable.ThrowTime;
            throwable.Throw(targetPosition);
        }

        public override IEnumerator Killed(bool killedByPlayer = true)
        {
            LaunchThrowable(transform.position);
            yield return base.Killed(killedByPlayer);
        }
    }
}