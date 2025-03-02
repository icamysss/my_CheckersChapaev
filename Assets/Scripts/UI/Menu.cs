using Sirenix.OdinInspector;
using UnityEngine;

public abstract class Menu : MonoBehaviour
{
    [SerializeField, ReadOnly] private string _menuType;

    public string MenuType => _menuType;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        // Автоматически устанавливаем тип при изменении в редакторе
        _menuType = GetType().Name;
    }
#endif

    public virtual void Initialize()
    {
    }

    public virtual void Open()
    {
    }

    public virtual void Close()
    {
    }
}