using Cysharp.Threading.Tasks;
using Luban;
using Tools.Log;
using YooAsset;

namespace GameCore.Core.Comp.Config
{
    public class ConfigComp : IConfigComp
    {
        private const string BytesRoot = "Assets/Res/Config/GenerateDatas/Byte/";

        public cfg.Tables Tables { get; private set; }

        public async UniTask Init()
        {
            await UniTask.SwitchToMainThread();
            Tables = new cfg.Tables(name =>
            {
                var handle = YooAssets.LoadAssetSync<UnityEngine.TextAsset>($"{BytesRoot}{name}.bytes");
                return new ByteBuf(handle.AssetObject as UnityEngine.TextAsset != null
                    ? ((UnityEngine.TextAsset)handle.AssetObject).bytes
                    : System.Array.Empty<byte>());
            });
            HLog.Log(this, "ConfigComp initialized.");
        }

        public UniTask UnInit()
        {
            Tables = null;
            return UniTask.CompletedTask;
        }
    }
}
