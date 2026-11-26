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
        
        [Header("Misc settings")]
        private const int EnemyCap = 15;
        private float time;
        
        public Dictionary<string, float> Context = new();
        
        public void OnGameStart(DirectorContext context, DirectorActions actions)
        {
            Context.Add("PlayerHealth", 0);
            Context.Add("PlayerAmmo", 0);
            Context.Add("PlayerProgression", 0);
        }

        public void Tick(DirectorContext context, DirectorActions actions, float deltaTime)
        {
            var bestAction = ChooseAction();

            Context["PlayerProgression"] = context.Player.Progress;
            Context["PlayerAmmo"] = context.Player.AmmoRatio;
            Context["PlayerHealth"] = context.Player.HealthRatio;
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

    }
}