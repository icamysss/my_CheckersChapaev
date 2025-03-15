using Services;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Menu : MonoBehaviour
    {
        [BoxGroup("Menu Debug")]
        [SerializeField, ReadOnly] private string menuType;
        [BoxGroup("Menu Debug")]
        [SerializeField, ReadOnly] protected CanvasGroup canvasGroup;
        
        
       
        public string MenuType => menuType;
        private protected UIManager uiManager;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            // Автоматически устанавливаем тип при изменении в редакторе
            if (string.IsNullOrEmpty(menuType)) menuType = GetType().Name;
            if ( canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        }
#endif

        public virtual void Initialize(UIManager manager)
        {
            uiManager = manager;
        }

        public virtual void Show()
        {
            canvasGroup.blocksRaycasts = true;  
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
        }

        public virtual void Hide()
        {
            canvasGroup.blocksRaycasts = false;  
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
        }
    }
}