using GameAnimation.Core;
using UnityEngine;

namespace GameAnimation.Samples
{
    /// <summary>
    /// Creates simple procedural animation clips at runtime for testing.
    /// </summary>
    public static class TestAnimationFactory
    {
        public static AnimationClip CreateIdleClip(float length = 2f)
        {
            var clip = new AnimationClip { name = "Test_Idle", legacy = false };

            // Subtle breathing animation - vertical position
            var curve = AnimationCurve.EaseInOut(0f, 0f, length, 0f);
            curve.AddKey(new Keyframe(length * 0.25f, 0.02f));
            curve.AddKey(new Keyframe(length * 0.75f, -0.02f));

            clip.SetCurve("", typeof(Transform), "localPosition.y", curve);

            // Wrap mode
            clip.wrapMode = WrapMode.Loop;
            clip.hideFlags = HideFlags.DontSave;

            return clip;
        }

        public static AnimationClip CreateWalkClip(float length = 1f)
        {
            var clip = new AnimationClip { name = "Test_Walk", legacy = false };

            // Walking bounce
            var yCurve = new AnimationCurve();
            var halfStep = length * 0.25f;
            yCurve.AddKey(0f, 0f);
            yCurve.AddKey(halfStep, 0.03f);
            yCurve.AddKey(halfStep * 2, 0f);
            yCurve.AddKey(halfStep * 3, 0.03f);
            yCurve.AddKey(length, 0f);

            clip.SetCurve("", typeof(Transform), "localPosition.y", yCurve);

            // Slight forward lean
            var rotCurve = AnimationCurve.EaseInOut(0f, 0f, length, 0f);
            rotCurve.AddKey(new Keyframe(0f, 2f));
            rotCurve.AddKey(new Keyframe(length, 2f));

            clip.SetCurve("", typeof(Transform), "localEulerAngles.x", rotCurve);

            clip.wrapMode = WrapMode.Loop;
            clip.hideFlags = HideFlags.DontSave;

            return clip;
        }

        public static AnimationClip CreateRunClip(float length = 0.5f)
        {
            var clip = new AnimationClip { name = "Test_Run", legacy = false };

            // Running bounce - more pronounced
            var yCurve = new AnimationCurve();
            var halfStep = length * 0.25f;
            yCurve.AddKey(0f, 0f);
            yCurve.AddKey(halfStep, 0.06f);
            yCurve.AddKey(halfStep * 2, 0f);
            yCurve.AddKey(halfStep * 3, 0.06f);
            yCurve.AddKey(length, 0f);

            clip.SetCurve("", typeof(Transform), "localPosition.y", yCurve);

            // More forward lean
            var rotCurve = AnimationCurve.EaseInOut(0f, 0f, length, 0f);
            rotCurve.AddKey(new Keyframe(0f, 8f));
            rotCurve.AddKey(new Keyframe(length, 8f));

            clip.SetCurve("", typeof(Transform), "localEulerAngles.x", rotCurve);

            clip.wrapMode = WrapMode.Loop;
            clip.hideFlags = HideFlags.DontSave;

            return clip;
        }

        public static AnimationClip CreateAttackClip(float length = 0.6f)
        {
            var clip = new AnimationClip { name = "Test_Attack", legacy = false };

            // Forward thrust
            var zCurve = new AnimationCurve();
            zCurve.AddKey(0f, 0f);
            zCurve.AddKey(length * 0.3f, 0f);
            zCurve.AddKey(length * 0.5f, 0.5f);
            zCurve.AddKey(length * 0.7f, 0.3f);
            zCurve.AddKey(length, 0f);

            clip.SetCurve("", typeof(Transform), "localPosition.z", zCurve);

            // Rotation swing
            var rotCurve = new AnimationCurve();
            rotCurve.AddKey(0f, 0f);
            rotCurve.AddKey(length * 0.3f, -15f);
            rotCurve.AddKey(length * 0.5f, 20f);
            rotCurve.AddKey(length * 0.8f, 0f);
            rotCurve.AddKey(length, 0f);

            clip.SetCurve("", typeof(Transform), "localEulerAngles.y", rotCurve);

            clip.wrapMode = WrapMode.Once;
            clip.hideFlags = HideFlags.DontSave;

            // Add animation event for completion
            var evt = new AnimationEvent
            {
                time = length,
                functionName = "OnAnimationComplete",
                intParameter = (int)AnimationEventType.ActionComplete
            };
            clip.AddEvent(evt);

            return clip;
        }

        public static AnimationClip CreateJumpClip(float length = 1f)
        {
            var clip = new AnimationClip { name = "Test_Jump", legacy = false };

            // Jump arc
            var yCurve = new AnimationCurve();
            yCurve.AddKey(0f, 0f);
            yCurve.AddKey(length * 0.1f, 0.1f);
            yCurve.AddKey(length * 0.3f, 0.5f);
            yCurve.AddKey(length * 0.5f, 0.6f);
            yCurve.AddKey(length * 0.7f, 0.4f);
            yCurve.AddKey(length * 0.9f, 0.1f);
            yCurve.AddKey(length, 0f);

            clip.SetCurve("", typeof(Transform), "localPosition.y", yCurve);

            // Crouch anticipation
            var rotCurve = new AnimationCurve();
            rotCurve.AddKey(0f, 0f);
            rotCurve.AddKey(length * 0.1f, 10f);
            rotCurve.AddKey(length * 0.3f, -5f);
            rotCurve.AddKey(length * 0.6f, -5f);
            rotCurve.AddKey(length, 0f);

            clip.SetCurve("", typeof(Transform), "localEulerAngles.x", rotCurve);

            clip.wrapMode = WrapMode.Once;
            clip.hideFlags = HideFlags.DontSave;

            var evt = new AnimationEvent
            {
                time = length,
                functionName = "OnAnimationComplete",
                intParameter = (int)AnimationEventType.ActionComplete
            };
            clip.AddEvent(evt);

            return clip;
        }
    }
}