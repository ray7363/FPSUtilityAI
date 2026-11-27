using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ZombieFPS
{
    public class MyDirector : MonoBehaviour, IAIDirector
    {
        [Header("Parameters")]
        [SerializeField] private List<UtilityAction> _actions;
        [SerializeField] private float paramSpeed = 0.01f;

        [Header("Context stats")]
        [SerializeField, Range(0, 1)] private float playerHealth;
        [SerializeField, Range(0, 1)] private float playerAmmo;
        [SerializeField, Range(0, 1)] private float playerProgression;
        [SerializeField, Range(0, 1)] private float playerGrenades;
        [SerializeField, Range(0, 1)] private float enemyCount;
        [SerializeField, Range(0, 1)] private float enemyPressure;
        [SerializeField, Range(0, 1)] private float timeSinceCombat;
        [SerializeField, Range(0, 1)] private float damageTakenRecent;

        [Header("Misc settings")]
        private const int EnemyCap = 15;
        private float time;

        public Dictionary<string, float> Context = new();

        public void OnGameStart(DirectorContext context, DirectorActions actions)
        {
            Context.Add("PlayerHealth", 0);
            Context.Add("PlayerAmmo", 0);
            Context.Add("PlayerProgression", 0);
            Context.Add("PlayerGrenades", 0);
            Context.Add("EnemyCount", 0);
            Context.Add("EnemyPressure", 0);
            Context.Add("TimeSinceCombat", 0);
            Context.Add("DamageTakenRecent", 0);
        }

        public void Tick(DirectorContext context, DirectorActions actions, float deltaTime)
        {
            Context["PlayerProgression"] = context.Player.Progress;
            Context["PlayerAmmo"] = context.Player.AmmoRatio;
            Context["PlayerHealth"] = context.Player.HealthRatio;
            Context["PlayerGrenades"] = Mathf.Clamp01(context.Player.Grenades / 3f);
            Context["EnemyCount"] = Mathf.Clamp01((float)context.CountEnemies() / EnemyCap);
            Context["EnemyPressure"] = Mathf.Clamp01((float)context.CountEnemiesWithin(10f) / EnemyCap);
            Context["TimeSinceCombat"] = Mathf.Clamp01(context.Stats.TimeSinceLastCombat / 30f);
            Context["DamageTakenRecent"] = Mathf.Clamp01(context.Stats.DamageTakenInLast(5f) / 50f);

            playerHealth = Context["PlayerHealth"];
            playerAmmo = Context["PlayerAmmo"];
            playerProgression = Context["PlayerProgression"];
            playerGrenades = Context["PlayerGrenades"];
            enemyCount = Context["EnemyCount"];
            enemyPressure = Context["EnemyPressure"];
            timeSinceCombat = Context["TimeSinceCombat"];
            damageTakenRecent = Context["DamageTakenRecent"];

            var bestAction = ChooseAction();
        }


        private UtilityAction ChooseAction() {
            // Selection
            UtilityAction bestAction = null;
            float bestScore = -1f;
            foreach (UtilityAction utilityAction in _actions) {
                float score = utilityAction.Evaluate(Context);

                if (score > bestScore) {
                    bestScore = score;
                    bestAction = utilityAction;
                }
            }

            return bestAction;
        }

        private bool SpawnEnemyNearPlayer(DirectorContext context, DirectorActions actions, EnemyType type, float minDistance = 8f, float maxDistance = 25f, bool alerted = false)
        {
            foreach (var point in context.EnemySpawnPoints)
            {
                if (point.IsOccupied || point.IsVisibleToPlayer) continue;
                if (point.DistanceToPlayer < minDistance || point.DistanceToPlayer > maxDistance) continue;

                return actions.SpawnEnemy(type, point, alerted);
            }

            return false;
        }

    }
}
