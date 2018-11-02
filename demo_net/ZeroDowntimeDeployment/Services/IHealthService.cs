namespace ZeroDowntimeDeployment.Services
{
    public interface IHealthService
    {
        bool IsHealth();
        void SetHealth(bool health);
    }
}
