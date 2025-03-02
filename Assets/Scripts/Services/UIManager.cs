using System.Collections.Generic;
using UnityEngine;

namespace Services
{
    public class UIManager : MonoBehaviour, IUIManager
    {
        [SerializeField] private List<Menu> _menuPrefabs = new List<Menu>();

        private Transform _uiRoot; // кэш своего трансформ
        private Stack<Menu> _menuStack = new Stack<Menu>();
        private Dictionary<string, Menu> _prefabCache = new Dictionary<string, Menu>();
        private Dictionary<string, Menu> _activeMenus = new Dictionary<string, Menu>();


        private void InitializePrefabCache()
        {
            foreach (var menuPrefab in _menuPrefabs)
            {
                var type = menuPrefab.MenuType;

                if (!_prefabCache.TryAdd(type, menuPrefab))
                {
                    Debug.LogError($"Duplicate menu type: {type}");
                    continue;
                }

                menuPrefab.gameObject.SetActive(true);
            }
        }

        private void BringToFront(Menu menu)
        {
            menu.transform.SetAsLastSibling();
            menu.Open();
        }

        #region IService

        public void Initialize()
        {
            InitializePrefabCache();
            Debug.Log("UIManager initialized");
            isInitialized = true;
        }

        public void Shutdown()
        {
            Debug.Log("UIManager shutting down");
        }

        public bool isInitialized { get; private set; }

        #endregion

        #region IUIManager

        public void OpenMenu(string menuType)
        {
            if (_prefabCache.TryGetValue(menuType, out Menu prefab))
            {
                // Если меню уже открыто
                if (_activeMenus.TryGetValue(menuType, out Menu existingMenu))
                {
                    BringToFront(existingMenu);
                    return;
                }

                // Создаем новый экземпляр
                var instance = Instantiate(prefab, _uiRoot);
                instance.Initialize();
                instance.Open();

                _menuStack.Push(instance);
                _activeMenus.Add(menuType, instance);
            }
            else Debug.LogError($"Could not find menu type: {menuType}");
        }

        public void CloseMenu(string menuType)
        {
            if (_activeMenus.TryGetValue(menuType, out Menu menu))
            {
                // Удаляем из стека
                var newStack = new Stack<Menu>();
                while (_menuStack.Count > 0)
                {
                    var item = _menuStack.Pop();
                    if (item != menu) newStack.Push(item);
                }
                _menuStack = newStack;

                // Уничтожаем меню
                menu.Close();
                Destroy(menu.gameObject);
                _activeMenus.Remove(menuType);

                // Восстанавливаем порядок
                if (_menuStack.Count > 0)
                {
                    BringToFront(_menuStack.Peek());
                }
            }else Debug.LogError($"Could not find menu type in active menus: {menuType}");
        }

        public void CloseTopMenu()
        {
            if (_menuStack.Count == 0) return;

            var menu = _menuStack.Pop();
            _activeMenus.Remove(menu.MenuType);
            menu.Close();
            
            if (_menuStack.Count > 0)
            {
                BringToFront(_menuStack.Peek());
            }
        }

        #endregion
    }
}