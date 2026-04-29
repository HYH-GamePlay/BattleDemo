using Combat.Runtime.Core;

namespace Combat.Runtime.Cues
{
    public readonly struct CueRequest
    {
        public readonly string CueIdentifier;
        public readonly string SourceActorIdentifier;
        public readonly string TargetActorIdentifier;
        public readonly CombatVector3 Position;

        public CueRequest(string cueIdentifier, string sourceActorIdentifier, string targetActorIdentifier, CombatVector3 position)
        {
            CueIdentifier = cueIdentifier;
            SourceActorIdentifier = sourceActorIdentifier;
            TargetActorIdentifier = targetActorIdentifier;
            Position = position;
        }
    }
}
