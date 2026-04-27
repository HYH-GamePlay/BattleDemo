using Tools.Log;
using UnityEngine;

namespace Combat.View
{
    /// <summary>
    /// 订阅技能表现事件，驱动 VFX/Audio 播放。
    /// </summary>
    public class SkillVFXHandler : MonoBehaviour
    {
        private Core.EventBus _bus;

        public void Initialize(Core.EventBus bus)
        {
            _bus = bus;
            bus.Subscribe<Core.SkillVFXEvent>(OnVFX);
            bus.Subscribe<Core.SkillAudioEvent>(OnAudio);
        }

        private void OnVFX(Core.SkillVFXEvent e)
        {
            // 通过 key 查找预制体并在指定位置播放
            // 实际项目中接入资源管理系统（YooAsset 等）
            HLog.Log($"[VFX] {e.VfxKey} at ({e.PosX}, {e.PosZ})");
        }

        private void OnAudio(Core.SkillAudioEvent e)
        {
            HLog.Log($"[Audio] {e.AudioKey} vol={e.Volume}");
        }

        private void OnDestroy()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<Core.SkillVFXEvent>(OnVFX);
            _bus.Unsubscribe<Core.SkillAudioEvent>(OnAudio);
        }
    }
}
