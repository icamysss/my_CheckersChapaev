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
        
        private IGameManager gameManager;
        private Game game;
        private bool isMute = false;

        public override void Initialize(UIManager manager)
        {
            base.Initialize(manager);
            uiManager = manager;
           
            whoTurn.text = "";
            // ссылки
            gameManager = manager.GameManager;
            game = gameManager.CurrentGame;
            // события 
            game.OnStartTurn += UpdateUI;
            gameManager.OnGameStateChanged += OnEndGame;
            game.OnGameStart += OnStartGame;
            // инициализация текстовых полей
            whiteCount.text = string.Empty;
            blackCount.text = string.Empty;
            whoTurn.text = string.Empty;
            // Слушатели для кнопок
            menuButton.onClick.AddListener(MenuButton);
            backButton.onClick.AddListener(BackToMainMenu);
            soundButton.onClick.AddListener(SwitchSound);
            restartButton.onClick.AddListener(RestartGame);
        }
        private void OnDestroy()
        {
            game.OnStartTurn -= UpdateUI;
            gameManager.OnGameStateChanged -= OnEndGame;
            game.OnGameStart -= OnStartGame;
        }

        
        #region Action callbacks
        
        private void UpdateUI()
        {
            whiteCount.text = game.Board.GetPawnsOnBoard(PawnColor.White).Count.ToString();
            blackCount.text = game.Board.GetPawnsOnBoard(PawnColor.Black).Count.ToString();
            
            whoTurn.text = $" Ходит игрок: {game.CurrentTurn.Name}";
        }

        private void OnEndGame(GameState state)
        {
            if (state != GameState.GameOver) return;
            
            var winner = game.Winner;
            whoTurn.text = winner == null ? "Ничья" 
                                          : $"Победил игрок: {winner.Name}";
           
        }

        private void OnStartGame()
        {
           UpdateUI();
        }
        
        #endregion
        
        #region Buttons Listeners

        private void BackToMainMenu()
        {
            gameManager.CurrentState = GameState.MainMenu;
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
            isMute = !isMute;
            var spriteState = soundButton.spriteState;
            spriteState.highlightedSprite = isMute ? volumeOffHover : volumeOnHover;
            soundButton.spriteState = spriteState;
            soundButton.image.sprite = isMute ? volumeOff : volumeOn;
            
            uiManager.AudioService.Mute(isMute);
        }

        private void RestartGame()
        {
            game.RestartGame();
        }

        #endregion
       
    }
}