using System.Collections.Generic;
using UnityEngine;

namespace Combat.View
{
    /// <summary>
    /// 战斗场景桥接器 — 顶层组装点，负责依赖注入和实体创建。
    /// 逻辑层通过构造函数注入，View 层通过 EventBus 解耦。
    /// </summary>
    public class CombatSceneBridge : MonoBehaviour
    {
        [Header("出生点")]
        [SerializeField] private Transform _playerSpawnPoint;
        [SerializeField] private Transform[] _enemySpawnPoints;

        private Core.CombatManager _manager;
        private TransformSyncSystem _transformSync;
        private SkillVFXHandler _vfxHandler;
        private readonly Dictionary<string, EntityView> _views = new();

        public Core.CombatManager Manager => _manager;
        public Core.CombatManager CombatManager => _manager;

        public Core.CombatEntity GetEntity(string id) => _manager.GetEntity(id);

        public void ExecuteAttack(string attackerId, string targetId, float damage)
        {
            var attacker = _manager.GetEntity(attackerId);
            var target = _manager.GetEntity(targetId);
            if (attacker != null && target != null)
                _manager.ExecuteAttack(attacker, target, damage);
        }

        public void StartDefense(string id)
        {
            var e = _manager.GetEntity(id);
            if (e != null) _manager.TryStartDefend(e);
        }

        public void EndDefense(string id)
        {
            var e = _manager.GetEntity(id);
            if (e != null) _manager.TryEndDefend(e);
        }

        public void TryPerfectBlock(string id, float holdTime)
        {
            var e = _manager.GetEntity(id);
            if (e != null) e.Fsm.SetData("defendHoldTime", holdTime);
        }

        public void TryDodge(string id)
        {
            var e = _manager.GetEntity(id);
            if (e != null) _manager.TryDodge(e);
        }

        public void Initialize(cfg.Tables tables)
        {
            _manager = new Core.CombatManager(tables);
            _transformSync = gameObject.AddComponent<TransformSyncSystem>();
            _vfxHandler = gameObject.AddComponent<SkillVFXHandler>();
            _vfxHandler.Initialize(_manager.EventBus);
        }

        private void Awake() { }

        private void Update() => _manager?.Tick(Time.deltaTime);

        public Core.CombatEntity CreatePlayer(string id, cfg.Config.ActorCfg stats, GameObject prefab)
        {
            var entity = _manager.CreateEntity(id, Core.EntityType.Player, stats);
            SpawnView(entity, prefab, _playerSpawnPoint?.position ?? Vector3.zero, Quaternion.identity);
            return entity;
        }

        public Core.CombatEntity CreateEnemy(string id, int enemyId, int spawnIndex = 0)
        {
            var enemyCfg = _manager.Tables.TbEnemy.GetOrDefault(enemyId);
            var stats = enemyCfg != null ? _manager.Tables.TbActor.GetOrDefault(enemyCfg.ActorId) : null;
            var entity = _manager.CreateEntity(id, Core.EntityType.Enemy, stats);
            var pos = _enemySpawnPoints != null && spawnIndex < _enemySpawnPoints.Length
                ? _enemySpawnPoints[spawnIndex].position : Vector3.zero;
            SpawnView(entity, null, pos, Quaternion.identity);
            return entity;
        }

        public void RemoveEntity(string id)
        {
            if (_views.TryGetValue(id, out var view))
            {
                _transformSync.Unregister(_manager.GetEntity(id));
                Destroy(view.gameObject);
                _views.Remove(id);
            }
            _manager.RemoveEntity(id);
        }

        private void SpawnView(Core.CombatEntity entity, GameObject prefab, Vector3 pos, Quaternion rot)
        {
            if (prefab == null) return;
            var go = Instantiate(prefab, pos, rot);
            var view = go.GetComponent<EntityView>() ?? go.AddComponent<EntityView>();
            view.Initialize(entity.EntityId, _manager.EventBus);
            _views[entity.EntityId] = view;
            _transformSync.Register(entity, go.transform);
        }

        private void OnDestroy() => _manager?.Clear();
    }
}
