# Combat Framework

## 如何使用

### 1. 创建战斗世界

场景内挂一个 `CombatWorldBehaviour`。它只负责持有 `CombatWorld`、注册定义数据、按帧调用 `World.Tick(delta)`。

可选做法：

- 勾选 `Initialize On Awake`：场景启动时自动创建战斗世界。
- 勾选 `Tick Automatically`：由 `Update` 自动驱动 Runtime。
- 填入 `Definition Catalogs`：把技能、效果、时间轴、碰撞盒定义注册到 `CombatDataRegistry`。

代码侧也可以直接创建纯 Runtime：

```csharp
var world = new CombatWorld(new CombatWorldOptions { RandomSeed = 10001 });
world.DataRegistry.RegisterAbility(abilityDefinition);
world.DataRegistry.RegisterTimeline(timelineDefinition);
world.DataRegistry.RegisterHitbox(hitboxDefinition);
world.Tick(Time.deltaTime);
```

### 2. 创建角色

运行时角色只需要 `CombatActorDefinition`。表现层对象可以用 `CombatActorView` 自动创建，也可以由业务层手动创建后再绑定。

```csharp
var actor = world.CreateActor(new CombatActorDefinition
{
    ActorIdentifier = "Player.001",
    DisplayName = "Player",
    TeamIdentifier = "Player",
    InitialPosition = new CombatVector3(0f, 0f, 0f),
    MaxHealth = 100f,
    MaxStamina = 100f,
    StaminaRecovery = 15f,
    AttackPower = 1f,
    Defense = 0f,
    CriticalChance = 0.1f,
    CriticalMultiplier = 1.5f,
    DamageDealtMultiplier = 1f,
    DamageTakenMultiplier = 1f,
    MoveSpeed = 5f,
    AbilityCooldownMultiplier = 1f,
});
```

如果角色来自 Luban `ActorCfg`：

```csharp
var definition = LubanCombatDefinitionFactory.CreateActorDefinition(
    actorCfg,
    "Enemy.1001",
    "Enemy",
    spawnPosition);
world.CreateActor(definition);
```

### 3. 注册并释放技能

技能定义只描述激活规则：消耗、冷却、Tag 条件、启动哪个 Timeline。

```csharp
world.Abilities.GrantAbility("Player.001", "Ability.Slash");
world.Abilities.Activate(new AbilityActivationRequest(
    "Player.001",
    "Ability.Slash",
    "Enemy.1001",
    new CombatVector3(0f, 0f, 1f),
    enemyPosition));
```

一条典型链路是：

`AbilitySystem` 检查资源/冷却/Tag -> 扣体力 -> 添加施法 Tag -> 启动 `TimelineSystem` -> 时间轴触发 `HitboxSystem` 或 `DamageSystem` -> 发布 `CueRequestedEvent` 给表现层。

### 4. 监听结果和表现请求

Runtime 不播放动画、不实例化特效、不操作 UI。表现层只订阅事件：

- `AttributeChangedEvent`：血量、体力等数值变化。
- `DamageResolvedEvent`：伤害结算完成。
- `ActorDeathEvent`：角色死亡。
- `CueRequestedEvent`：请求播放表现，如命中特效、音效、镜头震动。

Unity 侧可以用：

- `CombatActorView`：绑定角色、同步 Transform、把属性和死亡事件转成 UnityEvent。
- `CombatCueRouter`：把 `CueIdentifier` 映射到 Prefab 或 UnityEvent。

### 5. 使用 ScriptableObject 数据

创建 `Combat/Definition Catalog` 资源，把以下定义填进去：

- `AbilityDefinition`
- `EffectDefinition`
- `TimelineDefinition`
- `HitboxDefinition`

`CombatWorldBehaviour.Initialize()` 会把 Catalog 注册到 `CombatDataRegistry`。这适合设计期调试和非 Luban 数据；正式项目可以改由表格加载后注册同样的 Definition 对象。

### 6. 使用 Luban Buff 数据

已有 Buff 表可以通过适配器注册为新框架的 `EffectDefinition`：

```csharp
LubanCombatDefinitionFactory.RegisterEffects(world.DataRegistry, tables);
```

注意：Luban 生成的枚举值本身就是 `cfg.StatType` / `cfg.ModOp`，不要做 `-1` 这类偏移。当前适配器会直接按枚举名映射到 Runtime 属性和修改器操作。

## 如何拓展

### 添加一个技能

新增技能通常不需要写代码，只要新增三类数据：

1. `AbilityDefinition`：配置技能标识、冷却、体力消耗、需要/阻止/授予的 Tag、Timeline 标识。
2. `TimelineDefinition`：配置技能在第几秒触发碰撞盒、伤害、效果、Cue、位移或 Tag。
3. `HitboxDefinition` 或 `EffectDefinition`：配置范围命中和命中后的效果。

只有当技能需要新的时间轴行为时，才扩展 `TimelineEventType` 与 `TimelineSystem.ExecuteEvent`。

### 添加一个效果

优先用数据表达：

- 持续属性修改：填 `EffectDefinition.AttributeModifiers`。
- 周期伤害/治疗：填 `Period` 和 `ExecutionsOnPeriod`。
- 无敌、眩晕、霸体等状态：填 `GrantedTags`。
- 一次性伤害/治疗/套娃效果/表现：填 `ExecutionsOnApply`。

如果效果逻辑无法通过现有执行类型表达，再添加新的 `EffectExecutionType`，并在 `EffectSystem.Execute` 中集中实现。

### 添加一个属性

1. 在 `CombatAttributes` 新增规范化属性标识。
2. 在 `CombatActorDefinition` 增加初始字段。
3. 在 `CombatActor` 构造时写入 `AttributeSet`。
4. 如果来自 Luban，在 `LubanCombatDefinitionFactory.ResolveAttributeIdentifier` 中补映射。
5. 如果影响公式，在 `DamageSystem` 或对应系统读取该属性。

当前已内置：

- 生存：`Health`、`MaxHealth`
- 资源：`Stamina`、`MaxStamina`、`StaminaRecovery`
- 伤害：`AttackPower`、`Defense`、`CriticalChance`、`CriticalMultiplier`
- 伤害倍率：`DamageDealtMultiplier`、`DamageTakenMultiplier`
- 行动：`MoveSpeed`、`AbilityCooldownMultiplier`

### 添加一个 Tag

Tag 用字符串标识，建议遵循分层命名：

- `State.*`：运行时状态，如死亡、无敌、眩晕、防御。
- `Ability.*`：技能或技能族。
- `Damage.*`：伤害类型。
- `Cue.*`：表现请求。

新增公共 Tag 时放到 `CombatTags`，业务专用 Tag 可以由配置直接写字符串。`CombatWorld` 对 Tag 做引用计数：多个效果授予同一个 Tag 时，只有最后一个移除后 Tag 才会真正消失。

### 添加新的命中形状

当前 `HitboxSystem` 提供半径命中，适合先覆盖 ARPG 肉鸽的近战圆形、爆点和简化投射物命中。

扩展方向：

- 在 `HitboxDefinition` 增加 `ShapeType`、`Length`、`Width`、`Angle`。
- 在 `HitboxSystem.CanHit` 或独立 `IHitQuery` 中实现圆形、扇形、胶囊、盒形。
- 如果需要物理场景查询，把 Unity Physics 放在 `UnityBridge`，Runtime 只接收候选 Actor 或命中结果。

### 添加表现

不要从 Runtime 直接播放动画/特效。正确接入方式：

- 技能开始：监听 `AbilityActivatedEvent`。
- 命中、受击、音效、震屏：用 `CueRequestedEvent`。
- UI：监听 `AttributeChangedEvent`。
- 死亡表现：监听 `ActorDeathEvent`。

Unity 侧可以继续扩展 `CombatCueRouter`，把 `CueIdentifier` 映射到 Addressables、对象池、Timeline、Cinemachine 或 Wwise/FMOD。

### 添加输入和 AI

输入和 AI 不属于 Combat Runtime。它们只负责组装 `AbilityActivationRequest`：

- `ActorIdentifier`
- `AbilityIdentifier`
- `TargetActorIdentifier`
- `AimDirection`
- `TargetPosition`

这样玩家输入、敌人 AI、网络回放和自动战斗都走同一套能力激活入口。

## 实现说明

### 目录划分

```text
Assets/Scripts/Combat
  Runtime
    Core          世界、向量、数学、统一属性/Tag 标识
    Actors        角色定义与运行时角色
    Attributes    属性集和属性修改器
    Tags          GameplayTag 容器
    Abilities     技能定义、技能实例、激活流程
    Effects       效果定义、持续效果、周期执行
    Timeline      技能时间轴和时间轴事件
    Hitboxes      命中定义和空间筛选
    Damage        伤害请求、结果、公式
    Cues          表现请求
    Events        类型安全事件总线和事件定义
    Data          定义注册表、Luban 适配
  UnityBridge     MonoBehaviour、ScriptableObject、UnityEvent、Vector3 转换
```

### Runtime 边界

`Runtime` 层不依赖 `MonoBehaviour`、`Transform`、`Animator`、`ParticleSystem` 或 UI。它只处理：

- 数值
- 状态
- 规则
- 时间
- 事件

这让框架可以用于单元测试、服务器校验、回放、离线模拟，也方便未来替换表现层。

### Tick 顺序

`CombatWorld.Tick(delta)` 当前顺序：

1. 累计世界时间。
2. 恢复角色体力。
3. 推进技能冷却。
4. 推进持续效果和周期效果。
5. 推进技能时间轴。

效果在时间轴前 Tick，意味着同一帧内先处理已经存在的持续状态，再处理本帧技能事件。需要严格帧序时，可以在 `CombatWorld` 中调整系统顺序。

### 能力系统

`AbilitySystem` 只负责激活门槛和生命周期：

- 是否已授予技能。
- 冷却是否结束。
- RequiredTags 是否满足。
- BlockedTags 是否存在。
- 体力是否足够。
- 激活时授予 Tag、应用自效果、启动时间轴。
- 时间轴结束后移除技能授予的 Tag 并发布结束事件。

它不直接计算伤害，也不直接播放表现。

### 效果系统

`EffectSystem` 支持：

- Instant Effect：`Duration <= 0` 时只执行 `ExecutionsOnApply`。
- Persistent Effect：持续修改属性、授予 Tag，结束时回滚。
- Periodic Effect：按 `Period` 执行周期伤害、治疗、套用效果或请求 Cue。
- Stack：`AddStack` 会刷新持续时间并提高层数。加法修改器按层数线性增加，乘法修改器按层数幂乘。

系统 Tick 时先复制目标列表，避免周期效果执行中新增/移除效果导致集合枚举失效。

### 属性系统

`AttributeSet` 分为：

- Base Value：角色基础属性。
- Current Value：血量、体力这类当前值。
- Modifier：效果提供的临时修改器。

读取公式：

```text
FinalValue = (BaseValue + AdditiveModifiers) * MultiplicativeModifiers
```

当前值变化统一通过 `CombatWorld.ChangeCurrentAttribute`，会发布 `AttributeChangedEvent`。体力消耗和自然恢复都走同一入口。

### 伤害系统

`DamageSystem` 当前公式：

```text
FinalDamage =
    BaseDamage
    * Source.AttackPower
    * Source.DamageDealtMultiplier
    * Target.DamageTakenMultiplier
    * (1 - DefenseScale)
```

暴击在防御后乘 `CriticalMultiplier`。无敌 Tag 会使伤害直接归零。生命归零后添加 `State.Dead` 并发布 `ActorDeathEvent`。

### 时间轴系统

`TimelineSystem` 用数据描述技能过程：

- `RequestCue`：请求表现。
- `ApplyEffectToSelf` / `ApplyEffectToTarget`：套效果。
- `DamageTarget`：直接伤害当前目标。
- `ActivateHitbox`：激活范围命中。
- `AddTag` / `RemoveTag`：改变状态。
- `MoveActor`：按偏移、瞄准方向或目标点移动。

事件按 `Time` 排序执行。`TimelineEventTarget` 决定事件作用于自己、当前目标或目标点。

### 命中系统

`HitboxSystem` 现在使用 Runtime 位置做半径筛选，并按队伍规则过滤：

- `HitEnemies`
- `HitAllies`
- `HitSelf`

命中后可以同时触发伤害、效果和 Cue。候选角色会先复制到缓冲列表，避免命中事件中移除角色造成枚举错误。

### Cue 系统

`CueSystem` 只发布 `CueRequestedEvent`。Cue 是表现层协议，不是逻辑。逻辑不关心 Cue 被映射成粒子、音效、动画事件、镜头震动还是 UI 提示。

### Luban 适配

`LubanCombatDefinitionFactory` 是生成配置和 Runtime 定义之间的唯一适配层。当前包含：

- `ActorCfg -> CombatActorDefinition`
- `BuffCfg + TbBuffEffect -> EffectDefinition`
- `StatType -> CombatAttributes`
- `ModOp -> AttributeModifierOperation`

后续新增技能表、命中盒表、时间轴表时，也应该新增适配方法，而不是让 Runtime 直接读取生成表结构。

### 设计约束

- Runtime 不依赖 Unity 表现对象。
- 数据定义只描述规则，不写表现逻辑。
- 业务入口统一走 Request/Definition/Registry。
- 表现统一走 Event/Cue。
- 配置适配集中在 Data 层。
- 系统之间通过 `CombatWorld` 协作，避免互相持有复杂依赖。
