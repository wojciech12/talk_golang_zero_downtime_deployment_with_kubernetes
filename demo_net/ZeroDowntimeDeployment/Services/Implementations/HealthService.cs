namespace ZeroDowntimeDeployment.Services.Implementations
{
    public class HealthService : IHealthService
    {
        private bool _health = true;

        public bool IsHealth()
        {
            return _health;
        }

        public void SetHealth(bool health)
        {
            _health = health;
        }
    }
}
