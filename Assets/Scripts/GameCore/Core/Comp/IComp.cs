using Cysharp.Threading.Tasks;

namespace GameCore.Core.Comp{
    public interface IComp{
        UniTask Init();
        UniTask UnInit();
    }
}