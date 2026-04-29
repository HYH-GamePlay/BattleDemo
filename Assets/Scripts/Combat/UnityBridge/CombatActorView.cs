using Combat.Runtime.Actors;
using Combat.Runtime.Core;
using Combat.Runtime.Events;
using UnityEngine;
using UnityEngine.Events;

namespace Combat.UnityBridge
{
    public sealed class CombatActorView : MonoBehaviour
    {
        [SerializeField] private CombatWorldBehaviour worldBehaviour;
        [SerializeField] private string actorIdentifier;
        [SerializeField] private string displayName;
        [SerializeField] private string teamIdentifier;
        [SerializeField] private bool createActorOnStart = true;
        [SerializeField] private bool pushTransformToRuntime;
        [SerializeField] private bool pullTransformFromRuntime = true;
        [SerializeField] private CombatActorDefinition actorDefinition = new();
        [SerializeField] private CombatAttributeChangedUnityEvent onAttributeChanged = new();
        [SerializeField] private UnityEvent onActorDied = new();

        public string ActorIdentifier => actorIdentifier;
        public CombatActor Actor { get; private set; }

        private void Start()
        {
            if (createActorOnStart)
                CreateActor();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void LateUpdate()
        {
            if (Actor == null || worldBehaviour == null || worldBehaviour.World == null) return;

            if (pushTransformToRuntime)
                worldBehaviour.World.SetActorPosition(actorIdentifier, transform.position.ToCombatVector3());

            if (pullTransformFromRuntime)
                transform.position = Actor.Position.ToUnityVector3();
        }

        public CombatActor CreateActor()
        {
            if (worldBehaviour == null) return null;

            var definition = BuildDefinition();
            Actor = worldBehaviour.CreateActor(definition);
            actorIdentifier = Actor.ActorIdentifier;
            Subscribe();
            return Actor;
        }

        public void Bind(CombatWorldBehaviour world, string runtimeActorIdentifier)
        {
            Unsubscribe();
            worldBehaviour = world;
            actorIdentifier = runtimeActorIdentifier;
            Actor = worldBehaviour != null &&
                    worldBehaviour.World != null &&
                    worldBehaviour.World.TryGetActor(actorIdentifier, out var actor)
                ? actor
                : null;
            Subscribe();
        }

        private CombatActorDefinition BuildDefinition()
        {
            actorDefinition.ActorIdentifier = string.IsNullOrWhiteSpace(actorIdentifier)
                ? gameObject.name
                : actorIdentifier;
            actorDefinition.DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? gameObject.name
                : displayName;
            actorDefinition.TeamIdentifier = teamIdentifier;
            actorDefinition.InitialPosition = transform.position.ToCombatVector3();
            return actorDefinition;
        }

        private void Subscribe()
        {
            if (worldBehaviour == null || worldBehaviour.World == null || string.IsNullOrEmpty(actorIdentifier)) return;

            worldBehaviour.World.Events.Subscribe<AttributeChangedEvent>(OnAttributeChanged);
            worldBehaviour.World.Events.Subscribe<ActorDeathEvent>(OnActorDeath);
        }

        private void Unsubscribe()
        {
            if (worldBehaviour == null || worldBehaviour.World == null) return;

            worldBehaviour.World.Events.Unsubscribe<AttributeChangedEvent>(OnAttributeChanged);
            worldBehaviour.World.Events.Unsubscribe<ActorDeathEvent>(OnActorDeath);
        }

        private void OnAttributeChanged(AttributeChangedEvent combatEvent)
        {
            if (combatEvent.ActorIdentifier != actorIdentifier) return;

            onAttributeChanged.Invoke(
                combatEvent.AttributeIdentifier,
                combatEvent.OldValue,
                combatEvent.NewValue);
        }

        private void OnActorDeath(ActorDeathEvent combatEvent)
        {
            if (combatEvent.ActorIdentifier == actorIdentifier)
                onActorDied.Invoke();
        }
    }

    [System.Serializable]
    public sealed class CombatAttributeChangedUnityEvent : UnityEvent<string, float, float>
    {
    }
}
