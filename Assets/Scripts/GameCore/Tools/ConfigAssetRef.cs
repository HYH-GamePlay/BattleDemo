using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tools{
    /// <summary>
    /// 用于在配置文件中引用资源或脚本。
    /// 引用脚本时，可通过 <see cref="ScriptType"/> 在运行时获取对应的 Type。
    /// </summary>
    [Serializable]
    public class ConfigAssetRef{
        [SerializeField] private UnityEngine.Object _asset;

        /// <summary>
        /// 脚本的 AssemblyQualifiedName，有值即代表当前引用的是脚本，否则为普通资源。
        /// 由 Editor 赋值时自动写入，序列化后运行时直接使用。
        /// </summary>
        [SerializeField] private string _assemblyQualifiedName;

        /// <summary>是否为脚本引用（由 _assemblyQualifiedName 是否有值自动推断）</summary>
        public bool IsScript => !string.IsNullOrEmpty(_assemblyQualifiedName);

        /// <summary>获取资源引用（IsScript 时返回 null）</summary>
        public UnityEngine.Object Asset => IsScript ? null : _asset;

        /// <summary>获取脚本对应的 Type（IsScript 为 false 时返回 null）</summary>
        public Type ScriptType =>
            IsScript ? Type.GetType(_assemblyQualifiedName) : null;

#if UNITY_EDITOR
        /// <summary>在 Editor 中获取 MonoScript 对象</summary>
        public MonoScript MonoScript => _asset as MonoScript;

        /// <summary>
        /// 设置引用，自动识别是普通资源还是脚本：
        /// 传入 MonoScript → 记录类型名；其他资源 → 清空类型名。
        /// </summary>
        public void SetAsset(UnityEngine.Object asset){
            _asset = asset;
            _assemblyQualifiedName = asset is MonoScript mono
                ? mono.GetClass()?.AssemblyQualifiedName ?? string.Empty
                : null;
        }
#endif
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ConfigAssetRef))]
    public class ConfigAssetRefDrawer : PropertyDrawer{
        private const float BadgeWidth = 52f;
        private const float Spacing    = 4f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label){
            var nameProp = property.FindPropertyRelative("_assemblyQualifiedName");
            bool hasType = !string.IsNullOrEmpty(nameProp.stringValue);
            return hasType
                ? EditorGUIUtility.singleLineHeight * 2 + 2f
                : EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){
            EditorGUI.BeginProperty(position, label, property);

            var assetProp = property.FindPropertyRelative("_asset");
            var nameProp  = property.FindPropertyRelative("_assemblyQualifiedName");

            bool isScript = !string.IsNullOrEmpty(nameProp.stringValue);

            // 第一行：[Script/Asset 徽标]  [Object Field]
            var lineRect   = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var labelRect  = new Rect(lineRect.x, lineRect.y, EditorGUIUtility.labelWidth, lineRect.height);
            var badgeRect  = new Rect(lineRect.x + EditorGUIUtility.labelWidth, lineRect.y, BadgeWidth, lineRect.height);
            var fieldRect  = new Rect(badgeRect.xMax + Spacing, lineRect.y,
                                      lineRect.xMax - badgeRect.xMax - Spacing, lineRect.height);

            EditorGUI.LabelField(labelRect, label);

            // 只读徽标：根据自动识别结果显示
            var oldColor = GUI.color;
            GUI.color = isScript
                ? new Color(0.5f, 0.85f, 1f)   // 蓝色 = Script
                : new Color(0.75f, 0.95f, 0.6f); // 绿色 = Asset
            GUI.Label(badgeRect, isScript ? "◈ Script" : "◇ Asset", EditorStyles.centeredGreyMiniLabel);
            GUI.color = oldColor;

            // Object 引用框：统一接受 Object，拖入后自动识别
            var newAsset = EditorGUI.ObjectField(fieldRect, assetProp.objectReferenceValue,
                                                 typeof(UnityEngine.Object), false);
            if (newAsset != assetProp.objectReferenceValue){
                assetProp.objectReferenceValue = newAsset;
                nameProp.stringValue = newAsset is MonoScript mono
                    ? mono.GetClass()?.AssemblyQualifiedName ?? string.Empty
                    : null;
            }

            // 第二行（仅脚本时）：显示解析出的完整类型名
            if (isScript && !string.IsNullOrEmpty(nameProp.stringValue)){
                var typeRect  = new Rect(position.x + EditorGUIUtility.labelWidth + BadgeWidth + Spacing,
                                         position.y + EditorGUIUtility.singleLineHeight + 2f,
                                         position.width - EditorGUIUtility.labelWidth - BadgeWidth - Spacing,
                                         EditorGUIUtility.singleLineHeight);
                var resolved  = Type.GetType(nameProp.stringValue);
                var hint      = resolved != null
                    ? $"→ {resolved.FullName}"
                    : "⚠ 类型解析失败，请重新赋值";
                GUI.color = resolved != null ? new Color(0.6f, 0.9f, 0.6f) : new Color(1f, 0.6f, 0.6f);
                GUI.Label(typeRect, hint, EditorStyles.miniLabel);
                GUI.color = oldColor;
            }

            EditorGUI.EndProperty();
        }
    }
#endif
}

