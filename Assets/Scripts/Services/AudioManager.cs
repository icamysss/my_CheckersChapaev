using UnityEngine;

namespace Services
{
    public class AudioManager : MonoBehaviour, IAudioService
    {
        public void Initialize()
        {
            Debug.Log("Audio Manager initialized");
            isInitialized = true;
        }

        public void Shutdown()
        {
            throw new System.NotImplementedException();
        }

        public bool isInitialized { get; private set; }
    }
}