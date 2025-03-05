using UnityEngine;

namespace Services
{
    public interface ICameraController : IService
    {
        float MoveDuration { get; }
        Camera MainCamera { get; }
    }
}