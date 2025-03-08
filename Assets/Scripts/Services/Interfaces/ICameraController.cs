using UnityEngine;

namespace Services
{
    public interface ICameraController : IService
    {
        int MoveDuration { get; }  // время в милисек
        Camera MainCamera { get; }
    }
}