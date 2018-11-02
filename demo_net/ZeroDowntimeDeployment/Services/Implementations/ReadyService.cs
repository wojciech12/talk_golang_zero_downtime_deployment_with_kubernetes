namespace ZeroDowntimeDeployment.Services.Implementations
{
    public class ReadyService : IReadyService
    {
        private bool _ready = true;
        public void SetReady(bool ready)
        {
            _ready = ready;
        }

        public bool IsReady()
        {
            return _ready;
        }
    }
}