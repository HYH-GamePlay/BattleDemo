using Animancer;
using GameAnimation.Core;
using GameAnimation.Data;

namespace GameAnimation.Runtime
{
    internal sealed class AnimationLayerPlaybackState
    {
        public AnimationLayerType Layer { get; }
        public int LayerIndex { get; }
        public AnimationDefinition CurrentDefinition { get; private set; }
        public AnimancerState CurrentState { get; private set; }
        public int CurrentPriority { get; private set; }

        public AnimationLayerPlaybackState(AnimationLayerType layer, int layerIndex)
        {
            Layer = layer;
            LayerIndex = layerIndex;
        }

        public bool IsPlaying => CurrentState != null && CurrentState.IsPlayingAndNotEnding();

        public bool CanPlay(AnimationDefinition definition, AnimationCommand command)
        {
            if (definition == null) return false;
            if (command != null && command.ignoreInterruption) return true;
            if (!IsPlaying) return true;

            if (command != null &&
                command.requiredCancelGroupId != 0 &&
                !IsInCancelWindow(command.requiredCancelGroupId))
            {
                return false;
            }

            var requestedPriority = ResolvePriority(definition, command);
            if (CurrentDefinition == null) return true;

            return CurrentDefinition.canBeInterrupted
                ? requestedPriority >= CurrentPriority
                : requestedPriority > CurrentPriority;
        }

        public void SetCurrent(AnimationDefinition definition, AnimancerState state, AnimationCommand command)
        {
            CurrentDefinition = definition;
            CurrentState = state;
            CurrentPriority = ResolvePriority(definition, command);
        }

        public void Clear()
        {
            CurrentDefinition = null;
            CurrentState = null;
            CurrentPriority = 0;
        }

        public bool IsInCancelWindow(int cancelGroupId)
        {
            if (cancelGroupId == 0 || CurrentDefinition == null || CurrentState == null)
                return false;

            var normalizedTime = CurrentState.NormalizedTime;
            if (CurrentState.IsLooping)
                normalizedTime -= UnityEngine.Mathf.Floor(normalizedTime);

            foreach (var cancelWindow in CurrentDefinition.cancelWindows)
            {
                if (cancelWindow == null || cancelWindow.cancelGroupId != cancelGroupId)
                    continue;

                if (cancelWindow.Contains(normalizedTime))
                    return true;
            }

            return false;
        }

        private static int ResolvePriority(AnimationDefinition definition, AnimationCommand command)
        {
            return command != null && command.hasPriorityOverride
                ? command.priorityOverride
                : definition.priority;
        }
    }
}
