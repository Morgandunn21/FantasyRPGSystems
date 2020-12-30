using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Consortya.Dungeon
{
    [CreateAssetMenu(menuName = "Data/Status Effects/DOTEffect")]
    public class DOTEffect : StatusEffectSO
    {
        [Header("DOT Effect Parameters")]
        //Damage applied over time
        public uint effectDamagePerSecond;
        //How long often the damage is applied
        public float effectDamageInterval = 1;
        //How long the particle effect is up when activated
        public float particleEffectUpTime = 0.2f;
        //How long the particle effect stays deactivated
        public float particleEffectDownTime = 1.0f;

        //How much damage is done per tick
        protected float DOTdamage;

        protected override IEnumerator<float> _StatusEffect(GameObject currEffect, int index)
        {
            if (currEffect)
            {
                float mainEffectTimer = effectDuration;
                float DOTtimer = effectDamageInterval;
                float particleTimer = particleEffectDownTime;

                DOTdamage = effectDamagePerSecond * effectDamageInterval;

                //Show the particle effect when the status is first applied
                Timing.RunCoroutine(_ShowParticleEffect(currEffect));

                //while the effect is active
                while (mainEffectTimer > 0)
                {
                    //if it is time to apply damage
                    if (DOTtimer < 0)
                    {
                        //apply the damage
                        DealDOTDamage(index);

                        //if there is no down time, activate the particles
                        if (particleEffectDownTime <= 0)
                        {
                            Timing.RunCoroutine(_ShowParticleEffect(currEffect));
                        }
                        //if there is down time, check if enough time has passed to activate the particles
                        else if (particleTimer < 0)
                        {
                            Timing.RunCoroutine(_ShowParticleEffect(currEffect));
                            particleTimer = particleEffectDownTime;
                        }

                        //reset the damage timer
                        DOTtimer = effectDamageInterval;
                    }

                    yield return Timing.WaitForOneFrame;

                    //decrease all timers
                    DOTtimer -= Time.deltaTime;
                    particleTimer -= Time.deltaTime;
                    mainEffectTimer -= Time.deltaTime;
                }

                //destroy the particle effect
                Destroy(currEffect);
            }
        }

        protected bool DealDOTDamage(int index)
        {
            Debug.Log("Dot applied");
            EnemyAI affectedEnemy = affectedEnemies[index];

            bool killed = affectedEnemy.enemy.TakeDamage((uint)DOTdamage);

            return killed;
        }

        /// <summary>
        /// Handles activating and deactivating the associated particle effect
        /// </summary>
        /// <param name="currEffect"></param>
        /// <returns></returns>
        private IEnumerator<float> _ShowParticleEffect(GameObject currEffect)
        {
            currEffect.SetActive(true);

            yield return Timing.WaitForSeconds(particleEffectUpTime);

            //check that the currEffect wasnt destroyed during the wait
            if (currEffect != null)
            {
                currEffect.SetActive(false);
            }
        }
    }
}
