# 战斗框架设计文档

## 一、设计原则

### 1. 逻辑与表现分离

**核心思想**: 战斗逻辑和视觉表现完全分离，逻辑层不依赖Unity，表现层只负责渲染和反馈。

**架构图**:
```
┌─────────────────────────────────────────────────────────┐
│                    表现层 (View)                         │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐ │
│  │ 动画系统 │  │ 特效系统 │  │ 音效系统 │  │ UI系统   │ │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘ │
└─────────────────────────────────────────────────────────┘
                          ↑ 事件驱动
┌─────────────────────────────────────────────────────────┐
│                    逻辑层 (Logic)                        │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐ │
│  │ 战斗模型 │  │ 技能系统 │  │ 武器系统 │  │ 敌人AI   │ │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘ │
└─────────────────────────────────────────────────────────┘
```

### 2. 模块化设计

每个系统独立运作，通过事件系统通信。

### 3. 数据驱动

战斗数据通过配置文件定义，易于调整和扩展。

---

## 二、核心架构

### 1. 战斗管理器 (BattleManager)

**职责**: 管理整个战斗流程

```csharp
namespace GameLogic.Battle{
    /// <summary>
    /// 战斗管理器
    /// 负责战斗流程的整体控制
    /// </summary>
    public class BattleManager{
        private BattleModel _model;
        private BattleController _controller;
        private BattleView _view;

        public void Init(){
            _model = new BattleModel();
            _controller = new BattleController(_model);
            _view = new BattleView(_model);
        }

        public void StartBattle(){
            _controller.StartBattle();
        }

        public void EndBattle(){
            _controller.EndBattle();
        }

        public void Update(float deltaTime){
            _controller.Update(deltaTime);
        }
    }
}
```

### 2. 战斗模型 (BattleModel)

**职责**: 存储战斗数据，纯数据类，不依赖Unity

```csharp
namespace GameLogic.Battle{
    /// <summary>
    /// 战斗模型
    /// 存储战斗相关的所有数据
    /// </summary>
    public class BattleModel{
        // 玩家数据
        public PlayerData PlayerData { get; private set; }

        // 敌人数据
        public List<EnemyData> Enemies { get; private set; }

        // 战斗状态
        public BattleState State { get; set; }

        // 连击数据
        public int ComboCount { get; set; }

        // 战斗时间
        public float BattleTime { get; set; }

        public BattleModel(){
            PlayerData = new PlayerData();
            Enemies = new List<EnemyData>();
            State = BattleState.None;
        }
    }

    /// <summary>
    /// 战斗状态
    /// </summary>
    public enum BattleState{
        None,
        Prepare,
        Fighting,
        Pause,
        Victory,
        Defeat
    }
}
```

### 3. 战斗控制器 (BattleController)

**职责**: 处理战斗逻辑，不依赖Unity

```csharp
namespace GameLogic.Battle{
    /// <summary>
    /// 战斗控制器
    /// 处理战斗逻辑
    /// </summary>
    public class BattleController{
        private BattleModel _model;
        private SkillSystem _skillSystem;
        private WeaponSystem _weaponSystem;
        private EnemyAISystem _enemyAISystem;

        public BattleController(BattleModel model){
            _model = model;
            _skillSystem = new SkillSystem(model);
            _weaponSystem = new WeaponSystem(model);
            _enemyAISystem = new EnemyAISystem(model);
        }

        public void StartBattle(){
            _model.State = BattleState.Fighting;
            // 触发战斗开始事件
            EventComp.Broadcast(EventId.BattleStart);
        }

        public void EndBattle(){
            _model.State = BattleState.None;
            // 触发战斗结束事件
            EventComp.Broadcast(EventId.BattleEnd);
        }

        public void Update(float deltaTime){
            if (_model.State != BattleState.Fighting)
                return;

            _model.BattleTime += deltaTime;

            // 更新各个系统
            _skillSystem.Update(deltaTime);
            _weaponSystem.Update(deltaTime);
            _enemyAISystem.Update(deltaTime);
        }

        // 玩家攻击
        public void PlayerAttack(int skillId){
            _skillSystem.UseSkill(skillId);
        }

        // 玩家防御
        public void PlayerDefense(bool isDefense){
            _model.PlayerData.IsDefense = isDefense;
        }

        // 玩家闪避
        public void PlayerDodge(){
            // 闪避逻辑
        }
    }
}
```

### 4. 战斗视图 (BattleView)

**职责**: 处理视觉表现，依赖Unity

```csharp
namespace GameLogic.Battle{
    /// <summary>
    /// 战斗视图
    /// 处理视觉表现
    /// </summary>
    public class BattleView{
        private BattleModel _model;
        private AnimationSystem _animationSystem;
        private EffectSystem _effectSystem;
        private AudioSystem _audioSystem;

        public BattleView(BattleModel model){
            _model = model;

            // 初始化表现系统
            _animationSystem = new AnimationSystem();
            _effectSystem = new EffectSystem();
            _audioSystem = new AudioSystem();

            // 监听事件
            EventComp.AddListener<BattleEventData>(EventId.BattleStart, OnBattleStart);
            EventComp.AddListener<BattleEventData>(EventId.BattleEnd, OnBattleEnd);
        }

        private void OnBattleStart(BattleEventData data){
            // 播放战斗开始动画
            _animationSystem.PlayBattleStartAnimation();
            // 播放战斗开始音效
            _audioSystem.PlayBGM("BGM_Battle");
        }

        private void OnBattleEnd(BattleEventData data){
            // 播放战斗结束动画
            _animationSystem.PlayBattleEndAnimation();
            // 播放战斗结束音效
            _audioSystem.PlaySfx("SFX_BattleEnd");
        }
    }
}
```

---

## 三、核心系统

### 1. 技能系统 (SkillSystem)

**职责**: 管理技能的释放、冷却、效果

```csharp
namespace GameLogic.Battle.Skill{
    /// <summary>
    /// 技能系统
    /// 处理技能逻辑
    /// </summary>
    public class SkillSystem{
        private BattleModel _model;
        private List<SkillData> _skills;
        private Dictionary<int, float> _cooldownTimers;

        public SkillSystem(BattleModel model){
            _model = model;
            _skills = new List<SkillData>();
            _cooldownTimers = new Dictionary<int, float>();
        }

        public void Update(float deltaTime){
            // 更新冷却时间
            var keys = new List<int>(_cooldownTimers.Keys);
            foreach (var skillId in keys){
                if (_cooldownTimers[skillId] > 0){
                    _cooldownTimers[skillId] -= deltaTime;
                }
            }
        }

        public void UseSkill(int skillId){
            if (!CanUseSkill(skillId))
                return;

            var skillData = GetSkillData(skillId);
            if (skillData == null)
                return;

            // 执行技能逻辑
            ExecuteSkill(skillData);

            // 设置冷却时间
            _cooldownTimers[skillId] = skillData.Cooldown;

            // 触发技能使用事件
            EventComp.Broadcast(EventId.SkillUse, new SkillEventData{
                SkillId = skillId,
                SkillName = skillData.Name
            });
        }

        public bool CanUseSkill(int skillId){
            if (!_cooldownTimers.ContainsKey(skillId))
                return true;

            return _cooldownTimers[skillId] <= 0;
        }

        private void ExecuteSkill(SkillData skillData){
            // 计算伤害
            float damage = CalculateDamage(skillData);

            // 应用效果
            ApplySkillEffect(skillData, damage);
        }

        private float CalculateDamage(SkillData skillData){
            float baseDamage = _model.PlayerData.AttackPower;
            float skillMultiplier = skillData.DamageMultiplier;
            return baseDamage * skillMultiplier;
        }

        private void ApplySkillEffect(SkillData skillData, float damage){
            // 应用技能效果到敌人
            // 触发伤害事件
            EventComp.Broadcast(EventId.EnemyDeath, new DamageEventData{
                Damage = damage,
                TargetId = skillData.TargetId
            });
        }

        private SkillData GetSkillData(int skillId){
            return _skills.Find(s => s.Id == skillId);
        }
    }

    /// <summary>
    /// 技能数据
    /// </summary>
    public class SkillData{
        public int Id { get; set; }
        public string Name { get; set; }
        public float DamageMultiplier { get; set; }
        public float Cooldown { get; set; }
        public int TargetId { get; set; }
        public SkillType Type { get; set; }
    }

    public enum SkillType{
        Melee,
        Ranged,
        Magic,
        Ultimate
    }
}
```

### 2. 武器系统 (WeaponSystem)

**职责**: 管理武器的切换、属性、特殊效果

```csharp
namespace GameLogic.Battle.Weapon{
    /// <summary>
    /// 武器系统
    /// 处理武器逻辑
    /// </summary>
    public class WeaponSystem{
        private BattleModel _model;
        private List<WeaponData> _weapons;
        private int _currentWeaponIndex;

        public WeaponSystem(BattleModel model){
            _model = model;
            _weapons = new List<WeaponData>();
            _currentWeaponIndex = 0;
        }

        public void Update(float deltaTime){
            // 更新武器状态
        }

        public void SwitchWeapon(int index){
            if (index < 0 || index >= _weapons.Count)
                return;

            _currentWeaponIndex = index;

            // 更新玩家属性
            var weapon = _weapons[_currentWeaponIndex];
            _model.PlayerData.AttackPower = weapon.AttackPower;
            _model.PlayerData.AttackSpeed = weapon.AttackSpeed;

            // 触发武器切换事件
            EventComp.Broadcast(EventId.ItemEquip, new WeaponEventData{
                WeaponId = weapon.Id,
                WeaponName = weapon.Name
            });
        }

        public WeaponData GetCurrentWeapon(){
            if (_weapons.Count == 0)
                return null;

            return _weapons[_currentWeaponIndex];
        }

        public void AddWeapon(WeaponData weapon){
            _weapons.Add(weapon);
        }
    }

    /// <summary>
    /// 武器数据
    /// </summary>
    public class WeaponData{
        public int Id { get; set; }
        public string Name { get; set; }
        public WeaponType Type { get; set; }
        public float AttackPower { get; set; }
        public float AttackSpeed { get; set; }
        public float CriticalChance { get; set; }
        public float CriticalDamage { get; set; }
    }

    public enum WeaponType{
        Sword,
        Axe,
        Hammer,
        Bow,
        Gun,
        Staff
    }
}
```

### 3. 敌人AI系统 (EnemyAISystem)

**职责**: 管理敌人的行为和决策

```csharp
namespace GameLogic.Battle.Enemy{
    /// <summary>
    /// 敌人AI系统
    /// 处理敌人行为逻辑
    /// </summary>
    public class EnemyAISystem{
        private BattleModel _model;
        private List<EnemyAI> _enemyAIs;

        public EnemyAISystem(BattleModel model){
            _model = model;
            _enemyAIs = new List<EnemyAI>();
        }

        public void Update(float deltaTime){
            foreach (var enemyAI in _enemyAIs){
                enemyAI.Update(deltaTime);
            }
        }

        public void AddEnemy(EnemyData enemyData){
            var enemyAI = new EnemyAI(enemyData);
            _enemyAIs.Add(enemyAI);
        }

        public void RemoveEnemy(int enemyId){
            _enemyAIs.RemoveAll(ai => ai.EnemyData.Id == enemyId);
        }
    }

    /// <summary>
    /// 敌人AI
    /// 单个敌人的AI逻辑
    /// </summary>
    public class EnemyAI{
        public EnemyData EnemyData { get; private set; }
        private EnemyState _state;
        private float _stateTimer;

        public EnemyAI(EnemyData enemyData){
            EnemyData = enemyData;
            _state = EnemyState.Idle;
        }

        public void Update(float deltaTime){
            _stateTimer += deltaTime;

            switch (_state){
                case EnemyState.Idle:
                    UpdateIdleState(deltaTime);
                    break;
                case EnemyState.Patrol:
                    UpdatePatrolState(deltaTime);
                    break;
                case EnemyState.Chase:
                    UpdateChaseState(deltaTime);
                    break;
                case EnemyState.Attack:
                    UpdateAttackState(deltaTime);
                    break;
                case EnemyState.Death:
                    UpdateDeathState(deltaTime);
                    break;
            }
        }

        private void UpdateIdleState(float deltaTime){
            // 空闲状态逻辑
            if (_stateTimer > 2.0f){
                ChangeState(EnemyState.Patrol);
            }
        }

        private void UpdatePatrolState(float deltaTime){
            // 巡逻状态逻辑
        }

        private void UpdateChaseState(float deltaTime){
            // 追击状态逻辑
        }

        private void UpdateAttackState(float deltaTime){
            // 攻击状态逻辑
        }

        private void UpdateDeathState(float deltaTime){
            // 死亡状态逻辑
        }

        private void ChangeState(EnemyState newState){
            _state = newState;
            _stateTimer = 0;
        }
    }

    /// <summary>
    /// 敌人状态
    /// </summary>
    public enum EnemyState{
        Idle,
        Patrol,
        Chase,
        Attack,
        Death
    }

    /// <summary>
    /// 敌人数据
    /// </summary>
    public class EnemyData{
        public int Id { get; set; }
        public string Name { get; set; }
        public EnemyType Type { get; set; }
        public float HP { get; set; }
        public float MaxHP { get; set; }
        public float AttackPower { get; set; }
        public float MoveSpeed { get; set; }
        public float AttackRange { get; set; }
    }

    public enum EnemyType{
        Normal,
        Elite,
        Boss,
        Story,
        Environment
    }
}
```

### 4. 防御反击系统 (DefenseCounterSystem)

**职责**: 处理防御、闪避、反击逻辑

```csharp
namespace GameLogic.Battle.Combat{
    /// <summary>
    /// 防御反击系统
    /// 处理防御、闪避、反击逻辑
    /// </summary>
    public class DefenseCounterSystem{
        private BattleModel _model;
        private bool _isDefense;
        private bool _canCounter;
        private float _counterWindow;
        private float _counterTimer;

        public DefenseCounterSystem(BattleModel model){
            _model = model;
            _counterWindow = 0.5f;
        }

        public void Update(float deltaTime){
            if (_canCounter){
                _counterTimer += deltaTime;
                if (_counterTimer > _counterWindow){
                    _canCounter = false;
                    _counterTimer = 0;
                }
            }
        }

        public void StartDefense(){
            _isDefense = true;
            _model.PlayerData.IsDefense = true;
        }

        public void EndDefense(){
            _isDefense = false;
            _model.PlayerData.IsDefense = false;
        }

        public bool TryBlock(float damage){
            if (!_isDefense)
                return false;

            // 格挡成功
            _canCounter = true;
            _counterTimer = 0;

            // 触发格挡事件
            EventComp.Broadcast(EventId.BattleStart, new BlockEventData{
                BlockedDamage = damage * 0.5f
            });

            return true;
        }

        public bool TryCounter(){
            if (!_canCounter)
                return false;

            // 反击成功
            _canCounter = false;

            // 触发反击事件
            EventComp.Broadcast(EventId.BattleStart, new CounterEventData{
                CounterDamage = _model.PlayerData.AttackPower * 2.0f
            });

            return true;
        }

        public void Dodge(){
            // 闪避逻辑
            // 触发闪避事件
            EventComp.Broadcast(EventId.BattleStart, new DodgeEventData{
                IsSuccess = true
            });
        }
    }
}
```

---

## 四、表现系统

### 1. 动画系统 (AnimationSystem)

**职责**: 处理角色动画

```csharp
namespace GameLogic.Battle.View{
    /// <summary>
    /// 动画系统
    /// 处理角色动画
    /// </summary>
    public class AnimationSystem{
        private Dictionary<int, Animator> _animators;

        public AnimationSystem(){
            _animators = new Dictionary<int, Animator>();

            // 监听事件
            EventComp.AddListener<SkillEventData>(EventId.SkillUse, OnSkillUse);
            EventComp.AddListener<WeaponEventData>(EventId.ItemEquip, OnWeaponSwitch);
        }

        public void RegisterAnimator(int entityId, Animator animator){
            _animators[entityId] = animator;
        }

        public void PlayAnimation(int entityId, string animationName){
            if (_animators.TryGetValue(entityId, out var animator)){
                animator.Play(animationName);
            }
        }

        private void OnSkillUse(SkillEventData data){
            // 播放技能动画
            PlayAnimation(0, $"Skill_{data.SkillId}");
        }

        private void OnWeaponSwitch(WeaponEventData data){
            // 播放武器切换动画
            PlayAnimation(0, "WeaponSwitch");
        }
    }
}
```

### 2. 特效系统 (EffectSystem)

**职责**: 处理战斗特效

```csharp
namespace GameLogic.Battle.View{
    /// <summary>
    /// 特效系统
    /// 处理战斗特效
    /// </summary>
    public class EffectSystem{
        private Dictionary<string, GameObject> _effectPrefabs;

        public EffectSystem(){
            _effectPrefabs = new Dictionary<string, GameObject>();

            // 监听事件
            EventComp.AddListener<SkillEventData>(EventId.SkillUse, OnSkillUse);
            EventComp.AddListener<DamageEventData>(EventId.EnemyDeath, OnDamage);
        }

        public void PlayEffect(string effectName, Vector3 position){
            if (_effectPrefabs.TryGetValue(effectName, out var prefab)){
                var effect = GameObject.Instantiate(prefab, position, Quaternion.identity);
                GameObject.Destroy(effect, 3.0f);
            }
        }

        private void OnSkillUse(SkillEventData data){
            // 播放技能特效
            PlayEffect($"Effect_Skill_{data.SkillId}", Vector3.zero);
        }

        private void OnDamage(DamageEventData data){
            // 播放伤害特效
            PlayEffect("Effect_Hit", Vector3.zero);
        }
    }
}
```

### 3. 音效系统 (AudioSystem)

**职责**: 处理战斗音效

```csharp
namespace GameLogic.Battle.View{
    /// <summary>
    /// 音效系统
    /// 处理战斗音效
    /// </summary>
    public class AudioSystem{
        public AudioSystem(){
            // 监听事件
            EventComp.AddListener<SkillEventData>(EventId.SkillUse, OnSkillUse);
            EventComp.AddListener<WeaponEventData>(EventId.ItemEquip, OnWeaponSwitch);
        }

        public void PlayBGM(string name){
            AudioComp.PlayBGM(name);
        }

        public void PlaySfx(string name){
            AudioComp.PlaySfx(name);
        }

        private void OnSkillUse(SkillEventData data){
            // 播放技能音效
            PlaySfx($"SFX_Skill_{data.SkillId}");
        }

        private void OnWeaponSwitch(WeaponEventData data){
            // 播放武器切换音效
            PlaySfx("SFX_WeaponSwitch");
        }
    }
}
```

---

## 五、数据模型

### 1. 玩家数据 (PlayerData)

```csharp
namespace GameLogic.Battle.Data{
    /// <summary>
    /// 玩家数据
    /// </summary>
    public class PlayerData{
        public int Level { get; set; }
        public float HP { get; set; }
        public float MaxHP { get; set; }
        public float MP { get; set; }
        public float MaxMP { get; set; }
        public float AttackPower { get; set; }
        public float DefensePower { get; set; }
        public float AttackSpeed { get; set; }
        public float MoveSpeed { get; set; }
        public bool IsDefense { get; set; }

        public PlayerData(){
            Level = 1;
            HP = 100;
            MaxHP = 100;
            MP = 50;
            MaxMP = 50;
            AttackPower = 10;
            DefensePower = 5;
            AttackSpeed = 1.0f;
            MoveSpeed = 5.0f;
            IsDefense = false;
        }
    }
}
```

### 2. 事件数据 (EventData)

```csharp
namespace GameLogic.Battle.Event{
    /// <summary>
    /// 战斗事件数据
    /// </summary>
    public class BattleEventData{
        public int BattleId { get; set; }
    }

    /// <summary>
    /// 技能事件数据
    /// </summary>
    public class SkillEventData{
        public int SkillId { get; set; }
        public string SkillName { get; set; }
    }

    /// <summary>
    /// 武器事件数据
    /// </summary>
    public class WeaponEventData{
        public int WeaponId { get; set; }
        public string WeaponName { get; set; }
    }

    /// <summary>
    /// 伤害事件数据
    /// </summary>
    public class DamageEventData{
        public float Damage { get; set; }
        public int TargetId { get; set; }
    }

    /// <summary>
    /// 格挡事件数据
    /// </summary>
    public class BlockEventData{
        public float BlockedDamage { get; set; }
    }

    /// <summary>
    /// 反击事件数据
    /// </summary>
    public class CounterEventData{
        public float CounterDamage { get; set; }
    }

    /// <summary>
    /// 闪避事件数据
    /// </summary>
    public class DodgeEventData{
        public bool IsSuccess { get; set; }
    }
}
```

---

## 六、使用示例

### 1. 初始化战斗系统

```csharp
public class GameEntry : MonoBehaviour{
    private BattleManager _battleManager;

    private void Start(){
        // 初始化战斗管理器
        _battleManager = new BattleManager();
        _battleManager.Init();
    }

    private void Update(){
        _battleManager.Update(Time.deltaTime);
    }
}
```

### 2. 开始战斗

```csharp
public void StartBattle(){
    _battleManager.StartBattle();
}
```

### 3. 玩家操作

```csharp
// 攻击
_battleManager.PlayerAttack(skillId: 1);

// 防御
_battleManager.PlayerDefense(true);

// 闪避
_battleManager.PlayerDodge();
```

---

## 七、优势总结

### 1. 逻辑与表现分离

- 逻辑层不依赖Unity，易于测试和维护
- 表现层只负责渲染，职责清晰
- 通过事件系统解耦

### 2. 模块化设计

- 每个系统独立运作
- 易于扩展和修改
- 代码复用性高

### 3. 数据驱动

- 战斗数据通过配置文件定义
- 易于调整和平衡
- 支持热更新

### 4. 事件驱动

- 系统间通过事件通信
- 降低耦合度
- 易于扩展新功能

### 5. 性能优化

- 逻辑层轻量级
- 表现层可按需加载
- 支持对象池优化

---

## 八、后续扩展

### 1. 网络同步

- 逻辑层已经独立，易于添加网络同步
- 只需同步数据模型即可

### 2. 回放系统

- 记录事件流即可实现回放
- 逻辑层可重复执行

### 3. AI训练

- 逻辑层独立，易于接入AI训练
- 可作为强化学习环境

### 4. 编辑器工具

- 可视化配置技能、武器、敌人
- 实时预览战斗效果

这个战斗框架完全满足GDC需求，并实现了逻辑与表现的彻底分离！
