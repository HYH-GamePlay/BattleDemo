using System;
using Battle.Behavior.Action;
using Battle.Behavior.State;
using Battle.CombatInfo.Action;
using Battle.CombatInfo.State;
using Battle.Core;
using NUnit.Framework;

namespace Battle.Tests
{
    public sealed class ActorStateBehaviorTests
    {
        [Test]
        public void RequestAction_TransitionsThroughAttackAndReturnsToLocomotion()
        {
            var world = new CombatWorld();
            var actor = world.CreateActor();
            var stateBehavior = new ActorStateBehavior();
            var actionBehavior = new ActionBehavior();

            world.AttachBehavior(actor, stateBehavior);
            world.AttachBehavior(actor, actionBehavior);
            actionBehavior.RegisterAction(1001, new ActionData { length = 1 });

            Assert.AreEqual(ActorStateId.Locomotion, stateBehavior.Info.CurrentState);

            Assert.IsTrue(stateBehavior.RequestAction(1001));
            Assert.AreEqual(ActorStateId.Attack, stateBehavior.Info.CurrentState);
            Assert.AreEqual(1001, stateBehavior.Info.CurrentActionId);
            Assert.IsTrue(actionBehavior.Info.HasRunningRuntimes);

            world.Tick(TimeSpan.FromSeconds(1.0 / 60.0));

            Assert.AreEqual(ActorStateId.Locomotion, stateBehavior.Info.CurrentState);
            Assert.AreEqual(0, stateBehavior.Info.CurrentActionId);
            Assert.IsFalse(actionBehavior.Info.HasRunningRuntimes);
        }
    }
}
