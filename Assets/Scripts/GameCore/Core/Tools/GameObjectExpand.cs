using System;
using UnityEngine;

namespace GameCore.Core.Tools{
    public static class GameObjectExpand{
        public static T GetOrAddComp<T>(this GameObject go) where T : Component{
            var comp = go.GetComponent<T>();
            if (comp == null)
                comp = go.AddComponent<T>();
            return comp;
        }
        public static Component GetOrAddComp(this GameObject go, Type type){
            var comp = go.GetComponent(type);
            if (comp == null)
                comp = go.AddComponent(type);
            return comp;
        }
    }
}