using GameCore.Core.Comp.UI;

namespace GameCore.Core.Comp.UI{
    /// <summary>
    /// 主界面UI数据
    /// </summary>
    public class MainUIData : UIDataBase
    {
        /// <summary>
        /// 玩家名称
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// 玩家等级
        /// </summary>
        public int PlayerLevel { get; set; }

        /// <summary>
        /// 玩家经验值
        /// </summary>
        public int PlayerExp { get; set; }

        /// <summary>
        /// 玩家生命值
        /// </summary>
        public int PlayerHP { get; set; }

        /// <summary>
        /// 玩家最大生命值
        /// </summary>
        public int PlayerMaxHP { get; set; }

        /// <summary>
        /// 玩家魔法值
        /// </summary>
        public int PlayerMP { get; set; }

        /// <summary>
        /// 玩家最大魔法值
        /// </summary>
        public int PlayerMaxMP { get; set; }

        /// <summary>
        /// 当前武器名称
        /// </summary>
        public string CurrentWeaponName { get; set; }

        /// <summary>
        /// 当前武器等级
        /// </summary>
        public int CurrentWeaponLevel { get; set; }
    }
}
