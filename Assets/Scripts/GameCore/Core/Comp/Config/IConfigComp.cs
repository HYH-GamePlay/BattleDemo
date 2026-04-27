using Cysharp.Threading.Tasks;

namespace GameCore.Core.Comp.Config
{
    public interface IConfigComp : IComp
    {
        cfg.Tables Tables { get; }
    }
}
