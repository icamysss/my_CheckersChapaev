using System;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Register<T>(T service) where T : IService
        {
            service.Initialize();
            _services[typeof(T)] = service;
        }

        public static T Get<T>() where T : IService
        {
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
    }
}