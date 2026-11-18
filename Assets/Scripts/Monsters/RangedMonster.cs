using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class RangedMonster : Monster
    {
        public enum State
        {
            Walking,
            Shooting
        }

        [SerializeField] protected Transform projectileSpawnPosition;
        
        protected RangedMonsterBlueprint RangedBlueprint => (RangedMonsterBlueprint)monsterBlueprint;
        
        protected float timeSinceLastAttack;
        protected State state;
        protected float outOfRangeTime;
        protected int projectileIndex;

        public override void Setup(int monsterIndex, Vector2 position, MonsterBlueprint monsterBlueprint, float hpBuff = 0)
        {
            base.Setup(monsterIndex, position, monsterBlueprint, hpBuff);
            projectileIndex = entityManager.AddPoolForProjectile(RangedBlueprint.projectilePrefab);
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
                        rb.linearVelocity += dirToPlayer * RangedBlueprint.acceleration * Time.fixedDeltaTime;
                        entityManager.Grid.UpdateClient(this);
                        if (distance <= RangedBlueprint.range)
                        {
                            state = State.Shooting;
                            monsterSpriteAnimator.StopAnimating();
                        }
                        break;

                    case State.Shooting:
                        timeSinceLastAttack += Time.fixedDeltaTime;
                        if (timeSinceLastAttack >= 1.0f / RangedBlueprint.atkspeed)
                        {
                            LaunchProjectile(dirToPlayer);
                            timeSinceLastAttack = 0;
                        }
                        if (distance <= RangedBlueprint.range)
                            outOfRangeTime = 0;
                        else
                            outOfRangeTime += Time.deltaTime;
                        if (outOfRangeTime > RangedBlueprint.timeAllowedOutsideRange)
                        {
                            state = State.Walking;
                            monsterSpriteAnimator.StartAnimating();
                        }
                        break;
                }
            }
        }

        protected void LaunchProjectile(Vector2 direction)
        {
            Projectile projectile = entityManager.SpawnProjectile(
                projectileIndex, 
                projectileSpawnPosition.position, 
                RangedBlueprint.atk, 
                0, 
                RangedBlueprint.projectileSpeed, 
                RangedBlueprint.targetLayer
            );
            projectile.Launch(direction);
        }
    }
}