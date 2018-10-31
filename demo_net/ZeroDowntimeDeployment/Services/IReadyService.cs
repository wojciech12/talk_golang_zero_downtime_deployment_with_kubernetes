namespace ZeroDowntimeDeployment.Services
{
    public interface IReadyService
    {
        void SetReady(bool ready);
        bool IsReady();
    }
}