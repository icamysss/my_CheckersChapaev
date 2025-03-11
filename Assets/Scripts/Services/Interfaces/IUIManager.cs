namespace Services.Interfaces
{
    public interface IUIManager : IService
    {
        void OpenMenu(string menuName);
        void CloseMenu(string menuName);
        
        void CloseAllMenus();
        void CloseTopMenu();
    }
}