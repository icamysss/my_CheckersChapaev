namespace Services
{
    public interface IService
    {
        void Initialize();
        void Shutdown();
        bool IsInitialized { get; }
    }
}