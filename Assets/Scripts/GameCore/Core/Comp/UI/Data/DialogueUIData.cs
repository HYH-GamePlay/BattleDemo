using GameCore.Core.Comp.UI;

namespace GameCore.Core.Comp.UI{
    /// <summary>
    /// 对话界面UI数据
    /// </summary>
    public class DialogueUIData : UIDataBase
    {
        /// <summary>
        /// 对话角色名称
        /// </summary>
        public string CharacterName { get; set; }

        /// <summary>
        /// 对话文本
        /// </summary>
        public string DialogueText { get; set; }

        /// <summary>
        /// 对话选项列表
        /// </summary>
        public DialogueOption[] Options { get; set; }

        /// <summary>
        /// 对话选项类
        /// </summary>
        public class DialogueOption
        {
            /// <summary>
            /// 选项文本
            /// </summary>
            public string OptionText { get; set; }

            /// <summary>
            /// 选项索引
            /// </summary>
            public int OptionIndex { get; set; }

            /// <summary>
            /// 选项效果
            /// </summary>
            public DialogueEffect Effect { get; set; }

            /// <summary>
            /// 对话效果类
            /// </summary>
            public class DialogueEffect
            {
                /// <summary>
                /// 是否触发剧情
                /// </summary>
                public bool TriggerQuest { get; set; }

                /// <summary>
                /// 是否触发事件
                /// </summary>
                public bool TriggerEvent { get; set; }

                /// <summary>
                /// 是否改变剧情走向
                /// </summary>
                public bool ChangePlot { get; set; }

                /// <summary>
                /// 剧情ID
                /// </summary>
                public int PlotID { get; set; }

                /// <summary>
                /// 事件ID
                /// </summary>
                public int EventID { get; set; }
            }
        }
    }
}
