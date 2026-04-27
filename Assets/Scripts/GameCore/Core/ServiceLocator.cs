using System;
using System.Collections.Generic;
using System.Linq;

namespace GameCore.Core{
    public static class ServiceLocator{
        private static readonly Dictionary<Type, object> Services = new();

        public static void Register<T>(T service) => Services[typeof(T)] = service;
        public static T Get<T>() => (T)Services[typeof(T)];

        public static IEnumerable<T> GetAll<T>(){
            return Services.Values.OfType<T>();
        }
    }
}