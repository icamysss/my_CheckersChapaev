namespace Services.Interfaces
{
    public interface IUIManager : IService
    {
        void ShowMenu(string menuName);
        void HideMenu(string menuName);
        
        void CloseAllMenus();
    }
}