using GameCore.Core.Comp.UI;

namespace GameCore.Core.Comp.UI{
    /// <summary>
    /// 任务界面UI数据
    /// </summary>
    public class QuestUIData : UIDataBase
    {
        /// <summary>
        /// 任务列表
        /// </summary>
        public QuestInfo[] Quests { get; set; }

        /// <summary>
        /// 任务信息类
        /// </summary>
        public class QuestInfo
        {
            /// <summary>
            /// 任务ID
            /// </summary>
            public int QuestID { get; set; }

            /// <summary>
            /// 任务名称
            /// </summary>
            public string QuestName { get; set; }

            /// <summary>
            /// 任务描述
            /// </summary>
            public string QuestDescription { get; set; }

            /// <summary>
            /// 任务状态
            /// </summary>
            public QuestStatus Status { get; set; }

            /// <summary>
            /// 任务进度
            /// </summary>
            public int Progress { get; set; }

            /// <summary>
            /// 任务目标
            /// </summary>
            public string Goal { get; set; }

            /// <summary>
            /// 任务奖励
            /// </summary>
            public QuestReward[] Rewards { get; set; }

            /// <summary>
            /// 任务奖励类
            /// </summary>
            public class QuestReward
            {
                /// <summary>
                /// 奖励类型
                /// </summary>
                public string RewardType { get; set; }

                /// <summary>
                /// 奖励数量
                /// </summary>
                public int RewardCount { get; set; }

                /// <summary>
                /// 奖励名称
                /// </summary>
                public string RewardName { get; set; }
            }
        }

        /// <summary>
        /// 任务状态枚举
        /// </summary>
        public enum QuestStatus
        {
            /// <summary>
            /// 未接受
            /// </summary>
            NotAccepted,

            /// <summary>
            /// 进行中
            /// </summary>
            InProgress,

            /// <summary>
            /// 已完成
            /// </summary>
            Completed,

            /// <summary>
            /// 已放弃
            /// </summary>
            Abandoned
        }
    }
}
