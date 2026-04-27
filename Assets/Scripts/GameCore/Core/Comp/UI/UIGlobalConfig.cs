using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;

namespace GameCore.Core.Comp.UI{
    [GlobalConfig("Assets/Res/UI/Common/Config")]
    public class UIGlobalConfig : GlobalConfig<UIGlobalConfig>, ISerializationCallbackReceiver{
        [SerializeField, HideInInspector] private SerializationData serializationData;
        
        [ShowInInspector, DictionaryDrawerSettings] public Dictionary<Type, UIConfig> UIConfigs = new();

        void ISerializationCallbackReceiver.OnAfterDeserialize(){
            UnitySerializationUtility.DeserializeUnityObject(this, ref serializationData);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize(){
            UnitySerializationUtility.SerializeUnityObject(this, ref serializationData);
        }
    }
}