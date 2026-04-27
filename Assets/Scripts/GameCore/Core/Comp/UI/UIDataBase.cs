using Tools.ReferencePool;

namespace GameCore.Core.Comp.UI{
    public interface IUIData{

    }

    public class UIDataBase : IUIData{
        /// <summary>
        /// UI数据ID，用于唯一标识UI数据
        /// </summary>
        public int DataID { get; set; }

        /// <summary>
        /// UI数据是否有效
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// UI数据创建时间
        /// </summary>
        public long CreatedTime { get; set; }

        /// <summary>
        /// 验证UI数据是否有效
        /// </summary>
        public virtual void Validate()
        {
            IsValid = true;
        }

        /// <summary>
        /// 无效化UI数据
        /// </summary>
        public virtual void Invalidate()
        {
            IsValid = false;
        }
    }
}