using System;
using System.Collections.Generic;
using Combat.Runtime.Events;
using UnityEngine;
using UnityEngine.Events;

namespace Combat.UnityBridge
{
    public sealed class CombatCueRouter : MonoBehaviour
    {
        [SerializeField] private CombatWorldBehaviour worldBehaviour;
        [SerializeField] private List<CombatCueRoute> routes = new();

        private void OnEnable()
        {
            if (worldBehaviour != null && worldBehaviour.World != null)
                worldBehaviour.World.Events.Subscribe<CueRequestedEvent>(OnCueRequested);
        }

        private void OnDisable()
        {
            if (worldBehaviour != null && worldBehaviour.World != null)
                worldBehaviour.World.Events.Unsubscribe<CueRequestedEvent>(OnCueRequested);
        }

        private void OnCueRequested(CueRequestedEvent combatEvent)
        {
            foreach (var route in routes)
            {
                if (!route.Matches(combatEvent.CueIdentifier)) continue;

                var payload = new CombatCuePayload(combatEvent);
                route.OnCueRequested.Invoke(payload);

                if (route.Prefab != null)
                    Instantiate(route.Prefab, payload.Position, Quaternion.identity);
            }
        }
    }

    [Serializable]
    public sealed class CombatCueRoute
    {
        public string CueIdentifier;
        public GameObject Prefab;
        public CombatCueUnityEvent OnCueRequested = new();

        public bool Matches(string cueIdentifier)
            => CueIdentifier == cueIdentifier;
    }

    [Serializable]
    public sealed class CombatCueUnityEvent : UnityEvent<CombatCuePayload>
    {
    }

    [Serializable]
    public sealed class CombatCuePayload
    {
        public string CueIdentifier;
        public string SourceActorIdentifier;
        public string TargetActorIdentifier;
        public Vector3 Position;

        public CombatCuePayload(CueRequestedEvent combatEvent)
        {
            CueIdentifier = combatEvent.CueIdentifier;
            SourceActorIdentifier = combatEvent.SourceActorIdentifier;
            TargetActorIdentifier = combatEvent.TargetActorIdentifier;
            Position = combatEvent.Position.ToUnityVector3();
        }
    }
}
