using Animancer;
using UnityEngine;

namespace Combat.View
{
    /// <summary>
    /// 动画视图 — 双层 Animancer 架构：
    ///   Layer 0: LinearMixerState 驱动移动混合（Idle→Walk→Run）
    ///   Layer 1: Override 层播放动作动画，结束后自动淡出回 Layer 0
    /// </summary>
    [RequireComponent(typeof(AnimancerComponent))]
    public class AnimationView : MonoBehaviour
    {
        [Header("Layer 0 — Locomotion")]
        [SerializeField] private LinearMixerTransition _locomotionMixer;

        [Header("Layer 1 — Actions")]
        [SerializeField] public ClipTransition AttackClip;
        [SerializeField] public ClipTransition BlockClip;
        [SerializeField] public ClipTransition PerfectBlockClip;
        [SerializeField] public ClipTransition CounterClip;
        [SerializeField] public ClipTransition DodgeClip;
        [SerializeField] public ClipTransition DamageClip;
        [SerializeField] public ClipTransition DeathClip;

        [Header("Settings")]
        [SerializeField] private float actionFadeOut = 0.25f;

        private AnimancerComponent _animancer;
        private AnimancerLayer _locomotionLayer;
        private AnimancerLayer _actionLayer;
        private LinearMixerState _mixer;

        private void Awake()
        {
            _animancer = GetComponent<AnimancerComponent>();
            _locomotionLayer = _animancer.Layers[0];
            _actionLayer     = _animancer.Layers[1];

            if (_locomotionMixer != null)
                _mixer = _locomotionLayer.Play(_locomotionMixer) as LinearMixerState;
        }

        /// <summary>0=Idle, 0.5=Walk, 1=Run — 驱动 LinearMixerState.Parameter</summary>
        public void SetMoveSpeed(float normalizedSpeed)
        {
            if (_mixer != null) _mixer.Parameter = normalizedSpeed;
        }

        /// <summary>在 Layer 1 播放动作动画，结束后淡出回 Layer 0。</summary>
        public void PlayAction(ClipTransition clip)
        {
            if (clip == null || _animancer == null) return;
            clip.Events.OnEnd = () => _actionLayer.StartFade(0f, actionFadeOut);
            _actionLayer.Play(clip);
        }
    }
}
