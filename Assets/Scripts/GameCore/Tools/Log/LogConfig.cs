using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;

namespace Tools.Log{
    [GlobalConfig("Assets/Configs/")]
    public class LogConfig : GlobalConfig<LogConfig>, ISerializationCallbackReceiver{
        [SerializeField, HideInInspector] private SerializationData serializationData;

        [LabelText("启用")] public bool enable;

        [ShowInInspector, DictionaryDrawerSettings(KeyLabel = "Log类型", ValueLabel = "颜色")]
        public Dictionary<LogLevel, Color> ColorDic = new();

        [ShowInInspector, LabelText("元素列表"), ReadOnly, InfoBox("使用Log元素组织模板")]
        private List<string> _elementList = new(){
            "{tag}",
            "{time}",
            "{message}",
            "{owner}"
        };

        [LabelText("模板"), BoxGroup("个性模板")] public string template = "{tag}{owner}{time}{message} ";

        [LabelText("个性标题"), BoxGroup("个性模板")] public string tag = "[HLog]";

        [LabelText("时间"), BoxGroup("个性模板")] public string time = "[*]";

        [LabelText("内容"), BoxGroup("个性模板")] public string message = ":*";

        [LabelText("持有者"), BoxGroup("个性模板")] public string owner = "-[*]";

        void ISerializationCallbackReceiver.OnAfterDeserialize(){
            UnitySerializationUtility.DeserializeUnityObject(this, ref serializationData);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize(){
            UnitySerializationUtility.SerializeUnityObject(this, ref serializationData);
        }
    }
}