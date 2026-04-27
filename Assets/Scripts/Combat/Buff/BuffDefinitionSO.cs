using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Combat.Buff
{
    public enum BuffType { Buff, Debuff, DamageOverTime, Shield }

    [CreateAssetMenu(menuName = "Combat/Buff Definition", fileName = "BuffDef_")]
    public class BuffDefinitionSO : ScriptableObject
    {
        [BoxGroup("基础")]
        public string BuffId;
        [BoxGroup("基础")]
        public BuffType Type;
        [BoxGroup("基础")]
        public float DefaultDuration = 5f;
        [BoxGroup("基础")]
        public int MaxStacks = 1;
        [BoxGroup("基础")]
        public Sprite Icon;

        [BoxGroup("效果")]
        [ListDrawerSettings(ShowIndexLabels = true, AddCopiesLastElement = false)]
        public List<BuffEffectData> Effects = new();
    }
}
