using System.Collections;
using UnityEngine;

namespace Vampire
{
    public class MeleeMonster : Monster
    {
        protected MeleeMonsterBlueprint MeleeBlueprint => (MeleeMonsterBlueprint)monsterBlueprint;
        
        protected float timeSinceLastAttack;

        public override void Setup(int monsterIndex, Vector2 position, MonsterBlueprint monsterBlueprint, float hpBuff = 0)
        {
            base.Setup(monsterIndex, position, monsterBlueprint, hpBuff);
        }

        protected override void Update()
        {
            base.Update();
            timeSinceLastAttack += Time.deltaTime;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            Vector2 moveDirection = (playerCharacter.transform.position - transform.position).normalized;
            rb.linearVelocity += moveDirection * MeleeBlueprint.acceleration * Time.fixedDeltaTime;
            entityManager.Grid.UpdateClient(this);
        }

        void OnCollisionStay2D(Collision2D col)
        {
            if (alive && ((MeleeBlueprint.meleeLayer & (1 << col.collider.gameObject.layer)) != 0) 
                && timeSinceLastAttack >= 1.0f / MeleeBlueprint.atkspeed)
            {
                playerCharacter.TakeDamage(MeleeBlueprint.atk);
                timeSinceLastAttack = Mathf.Repeat(timeSinceLastAttack, 1.0f / MeleeBlueprint.atkspeed);
            }
        }
    }
}