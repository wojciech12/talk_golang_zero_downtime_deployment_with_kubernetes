namespace ZeroDowntimeDeployment.Services.Implementations
{
    public class ReadyService : IReadyService
    {
        private bool _ready = true;
        private readonly object _lock = new object();

        public void SetReady(bool ready)
        {
            lock (_lock)
            {
                _ready = ready;
            }
        }

        public bool IsReady()
        {
            lock (_lock)
            {
                return _ready;
            }
        }
    }
}