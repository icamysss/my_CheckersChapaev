using Services;

namespace UI
{
    public class MainMenu : Menu
    {
        private GameManager  gameManager;
       
        
        public override void Initialize()
        {
            base.Initialize();
            canvasGroup.ignoreParentGroups = true;
            gameManager = ServiceLocator.Get<GameManager>();
            
        }

       
    }
}