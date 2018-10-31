namespace ZeroDowntimeDeployment.Services.Implementations
{
    public class HealthService : IHealthService
    {
        public bool IsHealth()
        {
            return true;
        }
    }
}
