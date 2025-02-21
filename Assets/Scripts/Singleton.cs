using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;

    private static bool HasInstance => _instance != null;
    public static T TryGetInstance() => HasInstance ? _instance : null;

    public static T Instance
    {
        get
        {
            if (_instance != null) return _instance;
                
            _instance = FindAnyObjectByType<T>();
            if (_instance != null) return _instance;
                
            var go = new GameObject(typeof(T).Name + " Auto-Generated");
            _instance = go.AddComponent<T>();

            return _instance;
        }
    }

    /// <summary>
    /// Убедится в вызове base.Awake() при переопределении, если нужен Awake().
    /// </summary>
    private void Awake()
    {
        InitializeSingleton();
    }

    private void InitializeSingleton()
    {
        if (!Application.isPlaying) return;
            
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}