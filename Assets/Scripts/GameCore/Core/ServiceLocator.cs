using System;
using System.Collections.Generic;

namespace GameCore{
    public static class ServiceLocator{
        private static readonly Dictionary<Type, object> Services = new();
    
        public static void Register<T>(T service) => Services[typeof(T)] = service;
        public static T Get<T>() => (T)Services[typeof(T)];
    }
}