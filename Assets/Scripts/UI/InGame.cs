using Core;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InGame : Menu
    {
        #region Inspector

        [Header("Text")]
        [SerializeField] private Text whiteCount, blackCount;
        [SerializeField] private Text whoTurn;
        
        [Header("Buttons")]
        [SerializeField] private Button menuButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button soundButton;
        [SerializeField] private Button restartButton;
        
        [Header("Sprites")]
        [SerializeField] private Sprite volumeOff;
        [SerializeField] private Sprite volumeOffHover;
        
        [SerializeField] private Sprite volumeOn;
        [SerializeField] private Sprite volumeOnHover;

        #endregion
        
        private Game game;
        private UIManager uiManager;
        private bool soundOn;

        public override void Initialize(UIManager manager)
        {
            base.Initialize(manager);
            uiManager = manager;
           
            whoTurn.text = "";
            
            game = ServiceLocator.Get<IGameManager>().CurrentGame;
            
            game.OnStartTurn += UpdateUI;
            game.OnGameEnd += OnEndGame;
            game.OnGameStart += OnStartGame;
            
            
            whiteCount.text = string.Empty;
            blackCount.text = string.Empty;
            whoTurn.text = string.Empty;
            
            menuButton.onClick.AddListener(MenuButton);
            backButton.onClick.AddListener(BackToMainMenu);
            soundButton.onClick.AddListener(SwitchSound);
            restartButton.onClick.AddListener(RestartGame);
        }
        private void OnDestroy()
        {
            game.OnStartTurn -= UpdateUI;
            game.OnGameEnd -= OnEndGame;
            game.OnGameStart -= OnStartGame;
        }

        
        #region Action callbacks
        
        private void UpdateUI()
        {
            whiteCount.text = game.Board.GetPawnsOnBoard(PawnColor.White).Count.ToString();
            blackCount.text = game.Board.GetPawnsOnBoard(PawnColor.Black).Count.ToString();
            
            whoTurn.text = $" Ходит игрок: {game.CurrentTurn.Name}";
        }

        private void OnEndGame(PawnColor c)
        {
            var p = game.GetPlayerByColor(c);
            whoTurn.text = $"Победил игрок: {p.Name}";
        }

        private void OnStartGame()
        {
           UpdateUI();
        }
        #endregion
        
        #region Buttons Listeners

        private void BackToMainMenu()
        {
            uiManager.OpenMainMenu();
        }

        private void MenuButton()
        {
            var active = !backButton.gameObject.activeSelf;

            backButton.gameObject.SetActive(active);
            soundButton.gameObject.SetActive(active);
            restartButton.gameObject.SetActive(active);
        }

        private void SwitchSound()
        {
            soundOn = !soundOn;
            var spriteState = soundButton.spriteState;
            spriteState.highlightedSprite = soundOn ? volumeOnHover : volumeOffHover;
            soundButton.spriteState = spriteState;
            soundButton.image.sprite = soundOn ? volumeOn : volumeOff;
            
            uiManager.SwitchSound(soundOn);
        }

        private void RestartGame()
        {
            uiManager.RestartGame();
        }

        #endregion
       
    }
}