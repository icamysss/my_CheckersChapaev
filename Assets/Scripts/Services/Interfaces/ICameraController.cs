using UnityEngine;

namespace Services.Interfaces
{
    public interface ICameraController : IService
    {
        int MoveDurationMS { get; }  // время в милисек
        Camera MainCamera { get; }
    }
}