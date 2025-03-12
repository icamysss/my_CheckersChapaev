using System;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    public static class ServiceLocator
    {
        public static event Action OnAllServicesRegistered;
        private static readonly Dictionary<Type, object> _services = new ();
        private static bool _isAllSent;
        
        
        public static void Register<T>(T service) where T : IService
        {
            service.Initialize();
            _services[typeof(T)] = service;
        }

        private static void CheckAndTriggerEvent()
        {
            if (AllServicesRegistered)
            {
                OnAllServicesRegistered?.Invoke();
            }
        }

        public static T Get<T>() where T : IService
        {
            if (!_services.TryGetValue(typeof(T), out var service))
                throw new InvalidOperationException($"Service {typeof(T)} not registered.");

            if (!((IService)service).isInitialized)
                throw new InvalidOperationException($"Service {typeof(T)} not initialized.");

            return (T)_services[typeof(T)];
        }

        public static void ResetAll()
        {
            foreach (var service in _services.Values.OfType<IService>())
            {
                service.Shutdown();
            }
            _services.Clear();
        }

        public static bool AllServicesRegistered
        {
            get => _isAllSent && _services.Values.OfType<IService>().All(s => s.isInitialized);
            set
            {
                _isAllSent = value;
                CheckAndTriggerEvent();
            }
        }
    }
}