using UnityEngine;

namespace Services.Interfaces
{
    public interface ICameraController : IService
    {
        int MoveDuration { get; }  // время в милисек
        Camera MainCamera { get; }
    }
}