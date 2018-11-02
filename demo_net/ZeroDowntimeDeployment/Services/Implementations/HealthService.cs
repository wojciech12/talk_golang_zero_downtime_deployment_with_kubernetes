namespace ZeroDowntimeDeployment.Services.Implementations
{
    public class HealthService : IHealthService
    {
        private bool _health = true;
        private readonly object _lock = new object();

        public bool IsHealth()
        {
            lock (_lock)
            {
                return _health;
            }
        }

        public void SetHealth(bool health)
        {
            lock (_lock)
            {
                _health = health;
            }
        }
    }
}
