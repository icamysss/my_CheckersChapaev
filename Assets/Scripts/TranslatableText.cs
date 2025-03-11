// Компонент для текстовых элементов с автоматическим переводом

using Services;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TranslatableText : MonoBehaviour
{
    [SerializeField] private string translationKey;

    private Text textComponent;

    private void Awake()
    {
        
        textComponent = GetComponent<Text>();
        ServiceLocator.OnAllServicesRegistered += OnAllServicesRegistered;
    }

    private void OnDestroy()
    {
        ServiceLocator.OnAllServicesRegistered -= OnAllServicesRegistered;
    }

    public void UpdateText()
    {
        if (textComponent != null && !string.IsNullOrEmpty(translationKey))
        {
            textComponent.text = LanguageManager.Instance.GetTranslation(translationKey);
        }
    }

    // Для изменения ключа во время выполнения
    public void SetKey(string newKey)
    {
        translationKey = newKey;
        UpdateText();
    }

    private void OnAllServicesRegistered()
    {
        
        UpdateText();
        ServiceLocator.OnAllServicesRegistered -= OnAllServicesRegistered;
    }
}