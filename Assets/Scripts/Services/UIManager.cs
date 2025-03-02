using UnityEngine;

namespace Services
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private CanvasGroup Mainmenu;
        [SerializeField] private CanvasGroup SelectColor;
        [SerializeField] private CanvasGroup InGame;
        [SerializeField] private CanvasGroup EndGame;
        [SerializeField] private CanvasGroup HowToPlay;
        [SerializeField] private CanvasGroup Options;

        #region ShowMenu
        
        public void ShowMainMenu(bool show)
        {
            if (show)
                EnableCanvasGroup(Mainmenu);
            else DisableCanvasGroup(Mainmenu);
        }

        public void ShowSelectColor(bool show)
        {
            if (show)
                EnableCanvasGroup(SelectColor);
            else DisableCanvasGroup(SelectColor);
        }

        public void ShowInGame(bool show)
        {
            if (show)
                EnableCanvasGroup(InGame);
            else DisableCanvasGroup(InGame);
        }

        public void ShowEndGame(bool show)
        {
            if (show)
                EnableCanvasGroup(EndGame);
            else DisableCanvasGroup(EndGame);
        }

        public void ShowHowToPlay(bool show)
        {
            if (show)
                EnableCanvasGroup(HowToPlay);
            else DisableCanvasGroup(HowToPlay);
        }

        public void ShowOptions(bool show)
        {
            if (show)
                EnableCanvasGroup(Options);
            else DisableCanvasGroup(Options);
        }
        #endregion
        
        #region CanvasGroup

        private void EnableCanvasGroup(CanvasGroup group)
        {
            group.alpha = 1;
            group.interactable = true;
            group.blocksRaycasts = true;
        }

        private void DisableCanvasGroup(CanvasGroup group)
        {
            group.alpha = 0;
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        #endregion
    }
}