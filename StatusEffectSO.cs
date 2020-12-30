using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Consortya.Dungeon
{
    public abstract class StatusEffectSO : ScriptableObject
    {
        [Header("Base Effect Parameters")]
        //How long DOT lasts
        public uint effectDuration;
        //Particle Effect applied to targets during Damage Over Time
        public GameObject ParticleEffectPrefab;
        //Image related to the effect
        public Sprite effectImage;
        //The StatusEffect Enum
        public StatusEffectEnum statusEffectEnum;
        //Tooltip for the effect
        public string tooltip;

        //Enemies Affected
        protected List<EnemyAI> affectedEnemies = new List<EnemyAI>();
        //Players Affected
        protected List<Player> affectedPlayers = new List<Player>();

        /// <summary>
        /// Applies the effect to an enemy (Damage, Debuffs)
        /// </summary>
        /// <param name="enemyAI">Enemy being affected</param>
        public void ApplyEffect(TargetableObject target)
        {
            if(target.IsEnemy() == false)
            {
                return;
            }

            EnemyAI enemyAI = ((Enemy)target).mEnemyAI;

            GameObject activeEffect = null;
            int index = -1;

            if (ParticleEffectPrefab != null)
            {
                activeEffect = Instantiate(ParticleEffectPrefab, enemyAI.gameObject.transform);
                affectedEnemies.Add(enemyAI);
                StatusEffectManager.Instance.AddEffect(enemyAI.enemy, this, effectDuration);

                index = affectedEnemies.Count - 1;
            }

            Timing.RunCoroutine(_StatusEffect(activeEffect, index));
        }

        /// <summary>
        /// Applies the effect to a player (Healing, Buffs)
        /// </summary>
        /// <param name="player">Player being affected</param>
        public void ApplyEffect(Player player)
        {
            GameObject activeEffect = null;
            int index = -1;

            if (ParticleEffectPrefab != null)
            {
                activeEffect = Instantiate(ParticleEffectPrefab, player.gameObject.transform);
                affectedPlayers.Add(player);

                index = affectedPlayers.Count - 1;
            }

            Timing.RunCoroutine(_StatusEffect(activeEffect, index));
        }

        protected virtual IEnumerator<float> _StatusEffect(GameObject currEffect, int index)
        {
            if (currEffect)
            {
                float mainEffectTimer = effectDuration;

                while (mainEffectTimer > 0)
                {
                    yield return Timing.WaitForOneFrame;
                    mainEffectTimer -= Time.deltaTime;
                }

                Destroy(currEffect);
            }
        }
    }
}
