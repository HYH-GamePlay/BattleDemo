using Tools;
using UnityEngine;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameCore.Core.Comp.UI{
    /// <summary>
    /// UI配置
    /// 定义UI的层级、预制体、脚本类型
    /// </summary>
    [CreateAssetMenu(fileName = "UIConfig", menuName = "UI/UIConfig", order = 1)]
    public class UIConfig : ScriptableObject{
        /// <summary>
        /// UI层级
        /// </summary>
        public EUILayer layer;

        /// <summary>
        /// UI预制体
        /// </summary>
        public ConfigAssetRef prefab;

        /// <summary>
        /// UI脚本类型
        /// </summary>
        public ConfigAssetRef script;
    }
}