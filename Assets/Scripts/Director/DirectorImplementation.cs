using UnityEngine;

namespace ZombieFPS
{
    public class DirectorImplementation : MonoBehaviour, IAIDirector
    {
        private float time;
        private DirectorActions _actions;
        
        public void OnGameStart(DirectorContext context, DirectorActions actions)
        {
            _actions = actions;
            context.Events.GameOver += GameOverMessageText;
        }

        public void Tick(DirectorContext context, DirectorActions actions, float deltaTime)
        {
            time += deltaTime;
            if (time > 1f)
            {
                SpawnPointInfo info = context.EnemySpawnPoints[Random.Range(0, context.EnemySpawnPoints.Count)];
                actions.SpawnEnemy(EnemyType.Common, info);
                time = 0f;
            }
        }

        public void GameOverMessageText(bool isVictory)
        {
            _actions.ShowMessage(isVictory ? "Victory" : "t'es trop nul");
        }
    }
}