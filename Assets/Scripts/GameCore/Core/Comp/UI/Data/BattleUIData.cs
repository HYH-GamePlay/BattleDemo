using GameCore.Core.Comp.UI;

namespace GameCore.Core.Comp.UI{
    /// <summary>
    /// 战斗界面UI数据
    /// </summary>
    public class BattleUIData : UIDataBase
    {
        /// <summary>
        /// 敌人生命值
        /// </summary>
        public int EnemyHP { get; set; }

        /// <summary>
        /// 敌人最大生命值
        /// </summary>
        public int EnemyMaxHP { get; set; }

        /// <summary>
        /// 敌人名称
        /// </summary>
        public string EnemyName { get; set; }

        /// <summary>
        /// 连击数
        /// </summary>
        public int ComboCount { get; set; }

        /// <summary>
        /// 玩家生命值百分比
        /// </summary>
        public float PlayerHPPercent { get; set; }

        /// <summary>
        /// 玩家魔法值百分比
        /// </summary>
        public float PlayerMPPercent { get; set; }

        /// <summary>
        /// 技能冷却状态
        /// </summary>
        public SkillCooldownData[] SkillCooldowns { get; set; }

        /// <summary>
        /// 技能冷却数据类
        /// </summary>
        public class SkillCooldownData
        {
            /// <summary>
            /// 技能索引
            /// </summary>
            public int SkillIndex { get; set; }

            /// <summary>
            /// 技能名称
            /// </summary>
            public string SkillName { get; set; }

            /// <summary>
            /// 冷却剩余时间
            /// </summary>
            public float RemainingTime { get; set; }

            /// <summary>
            /// 冷却最大时间
            /// </summary>
            public float MaxTime { get; set; }

            /// <summary>
            /// 是否冷却中
            /// </summary>
            public bool IsCooldown { get; set; }
        }
    }
}
