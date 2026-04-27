using GameCore.Core;
using Tools.Log;
using UnityEngine;

namespace Combat.View
{
    /// <summary>
    /// 战斗测试启动器
    /// </summary>
    public class BattleTestRunner : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Transform _playerSpawnPoint;
        [SerializeField] private Transform[] _enemySpawnPoints;
        [SerializeField] private CombatSceneBridge _bridge;

        [Header("Data")]
        [SerializeField] private int _playerActorId;
        [SerializeField] private int _enemyId;
        [SerializeField] private GameObject _playerPrefab;

        [Header("UI")]
        [SerializeField] private UnityEngine.UI.Text _statusText;

        private Core.CombatEntity _playerEntity;
        private CombatInputHandler _inputHandler;

        private void Start()
        {
            HLog.Log(this, "BattleTestRunner.Start() called");
            HLog.Log(this, $"_enemyId: {_enemyId}");
            HLog.Log(this, $"_playerPrefab: {(_playerPrefab != null ? _playerPrefab.name : "null")}");
            HLog.Log(this, $"_bridge: {(_bridge != null ? _bridge.name : "null")}");
            InitializeBattle();
        }

        private void InitializeBattle()
        {
            if (_bridge == null)
            {
                _bridge = FindObjectOfType<CombatSceneBridge>();
            }

            if (_bridge == null)
            {
                HLog.LogE(this, "CombatSceneBridge not found!");
                return;
            }

            _bridge.Initialize(Game.Config.Tables);

            // 创建玩家
            CreatePlayer();

            // 创建敌人
            CreateEnemies();

            // 设置输入处理
            SetupInputHandler();

            UpdateStatusText("Battle Started - WASD: Move, Left Click: Attack, Right Click: Block, Space: Dodge");
        }

        private void CreatePlayer()
        {
            var stats = Game.Config.Tables.TbActor.GetOrDefault(_playerActorId);
            if (stats == null)
            {
                HLog.LogE(this, $"ActorCfg not found for id={_playerActorId}!");
                return;
            }

            _playerEntity = _bridge.CreatePlayer("player", stats, _playerPrefab);

            if (_playerSpawnPoint != null)
            {
                _playerEntity.PositionX = _playerSpawnPoint.position.x;
                _playerEntity.PositionZ = _playerSpawnPoint.position.z;
            }

            HLog.Log(this, $"Player created: HP={_playerEntity.Hp}, Stamina={_playerEntity.Stamina}");
        }

        private void CreateEnemies()
        {
            int spawnCount = _enemySpawnPoints != null ? _enemySpawnPoints.Length : 1;
            if (spawnCount == 0) spawnCount = 1;

            for (int i = 0; i < spawnCount; i++)
            {
                var enemy = _bridge.CreateEnemy($"enemy_{i}", _enemyId, i);
                HLog.Log(this, $"Enemy created: {enemy.EntityId}");
            }
        }

        private void SetupInputHandler()
        {
            var inputObj = new GameObject("InputHandler");
            inputObj.transform.SetParent(transform);
            _inputHandler = inputObj.AddComponent<CombatInputHandler>();
            _inputHandler.Initialize(_bridge, "player");
        }

        private void Update()
        {
            UpdateStatusText();
        }

        private void UpdateStatusText(string message = null)
        {
            if (_statusText == null) return;

            if (!string.IsNullOrEmpty(message))
            {
                _statusText.text = message;
                return;
            }

            if (_playerEntity != null)
            {
                var state = _playerEntity.State;
                var hp = _playerEntity.Hp;
                var stamina = _playerEntity.Stamina;
                _statusText.text = $"HP: {hp:F0} | Stamina: {stamina:F0} | State: {state}\n" +
                                   $"WASD: Move | LMB: Attack | RMB: Block | Space: Dodge";
            }
        }

        private void OnDestroy()
        {
            if (_bridge != null)
            {
                _bridge.CombatManager?.Clear();
            }
        }
    }
}
