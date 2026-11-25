namespace ZombieFPS
{
    public class DirectorImplementation : IAIDirector
    {
        public void OnGameStart(DirectorContext context, DirectorActions actions)
        {
            context.Events.PlayerDamaged += amount => 
        }

        public void Tick(DirectorContext context, DirectorActions actions, float deltaTime)
        {
            
        }
    }
}