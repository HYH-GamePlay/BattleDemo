using Combat.Runtime.Core;
using Combat.Runtime.Events;

namespace Combat.Runtime.Cues
{
    public sealed class CueSystem
    {
        private readonly CombatWorld _world;

        public CueSystem(CombatWorld world)
        {
            _world = world;
        }

        public void Request(CueRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CueIdentifier)) return;

            _world.Events.Publish(new CueRequestedEvent
            {
                CueIdentifier = request.CueIdentifier,
                SourceActorIdentifier = request.SourceActorIdentifier,
                TargetActorIdentifier = request.TargetActorIdentifier,
                Position = request.Position,
            });
        }
    }
}
