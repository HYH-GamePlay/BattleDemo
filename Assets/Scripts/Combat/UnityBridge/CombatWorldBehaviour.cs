using System.Collections.Generic;
using Combat.Runtime.Actors;
using Combat.Runtime.Core;
using UnityEngine;

namespace Combat.UnityBridge
{
    public sealed class CombatWorldBehaviour : MonoBehaviour
    {
        [SerializeField] private bool initializeOnAwake = true;
        [SerializeField] private bool tickAutomatically = true;
        [SerializeField] private int randomSeed;
        [SerializeField] private List<CombatDefinitionCatalog> definitionCatalogs = new();

        public CombatWorld World { get; private set; }
        public bool IsInitialized => World != null;

        private void Awake()
        {
            if (initializeOnAwake)
                Initialize();
        }

        private void Update()
        {
            if (tickAutomatically)
                Tick(Time.deltaTime);
        }

        public CombatWorld Initialize()
        {
            World = new CombatWorld(new CombatWorldOptions { RandomSeed = randomSeed });

            foreach (var catalog in definitionCatalogs)
                catalog?.RegisterDefinitions(World.DataRegistry);

            return World;
        }

        public void Tick(float delta)
        {
            World?.Tick(delta);
        }

        public CombatActor CreateActor(CombatActorDefinition definition)
        {
            if (World == null)
                Initialize();

            return World.CreateActor(definition);
        }
    }
}
