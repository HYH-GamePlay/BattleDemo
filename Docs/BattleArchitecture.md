# Battle 架构约定

本文档记录当前 Battle 逻辑层的设计约定，后续开发默认按这里执行。

## 目标

- 战斗逻辑层与 Unity 表现层解耦。
- `CombatWorld` 只做容器、生命周期、查询入口和调度。
- 具体战斗规则放在 `Ability` 或后续业务执行单元里。
- RIL 翻译层负责把逻辑状态同步成动画、特效、音效、UI 等表现。
- 核心层要为后续大世界加载、回放和同步预留空间。

## 核心概念

### CombatWorld

`CombatWorld` 是逻辑运行时入口。

它负责：

- Actor 生命周期
- Ability 生命周期
- CombatInfo 注册、查询和版本标记
- 固定 Phase Tick 调度

它不负责具体战斗业务，例如伤害计算、Buff 结算、命中判定或死亡处理。

### ActorId

`ActorId` 是运行时逻辑实体身份。

它用于：

- Ability 归属
- 目标选择
- 运行时 Actor 引用

它不等于 Unity `GameObject`，也不等于静态配置 id。

### IAbility

`IAbility` / `AbilityBase` 是逻辑执行单元。

Ability 可以：

- 通过 `CombatWorld` 读取或修改 Info
- 持有自己的运行时状态
- 在某个 `CombatPhase` 中执行

Ability 不直接操作 Unity 表现对象。

### ICombatInfo

`ICombatInfo` 是纯数据标记。

原则：

- 不写业务逻辑
- 默认不持有 Unity 对象引用
- 不强制绑定 `ActorId`
- 由业务层或 RIL 决定它怎么索引、归属和同步

### InfoHandle

`InfoHandle` 标识一份运行时 `ICombatInfo` 实例。

RIL 或业务索引可以持有 `InfoHandle`，并通过 `CombatWorld` 读取对应 Info。

### AbilityHandle

`AbilityHandle` 标识一份运行时 Ability 实例。

它用于：

- 动态挂载 / 卸载 Ability
- 区分同类配置产生的不同 Ability 实例
- 让外部保存句柄，而不是直接保存对象引用

### CombatPhase

`CombatPhase` 用来固定同帧执行顺序。

当前阶段：

- `PreUpdate`
- `Input`
- `Ability`
- `Movement`
- `Hit`
- `Damage`
- `State`
- `Presentation`
- `PostUpdate`

### CombatTime

`CombatTime` 是单帧 Tick 上下文，包含：

- `Frame`
- `DeltaTime`
- `ElapsedTime`

它是值类型，用于 Tick 传递。

## Tag

Tag 借鉴 GAS 的设计，主要用于技能控制。

当前文件：

- `TagId`
- `TagSet`
- `TagContainerInfo`
- `TagRequirement`
- `TagAbility`

职责划分：

- `TagContainerInfo` 只保存标签容器数据。
- `TagAbility` 负责增删查和条件判断。
- `TagRequirement` 是规则对象，描述 `RequiredAll`、`RequiredAny`、`BlockedAny`。
- `TagId` / `TagSet` 是基础类型。

推荐用途：

- 角色状态：`State.Stun`、`State.Dead`、`Form.Empowered`
- 技能状态：`Ability.NormalAttack`、`Ability.Charging`
- 控制规则：需要标签、阻塞标签、取消标签

当前查询模型：

- `RequiredAll`
- `RequiredAny`
- `BlockedAny`

后续方向：

- Tag 查找后面需要演进成树形或层级查找模型。
- 当前实现先保持扁平集合判断。
- 暂时不实现树查找，等真实技能控制需求明确后再扩展。

## Effect

Effect 借鉴 GAS 的协议思想，但不照搬 Unreal GAS 实现。

当前文件：

- `EffectData`
- `EffectSpec`
- `EffectRuntimeInfo`
- `EffectApplicationInfo`
- `EffectDurationPolicy`
- `EffectAbility`

职责划分：

- `EffectData`：静态效果模板。
- `EffectSpec`：一次效果应用上下文，包含来源、目标、来源 Ability、等级、层数、随机种子等。
- `EffectRuntimeInfo`：持续或无限效果的运行时状态。
- `EffectApplicationInfo`：待应用效果队列。
- `EffectAbility`：后续用于解释和执行 Effect。

Effect 只描述“应该发生什么”，具体如何结算仍放在 Ability 或后续业务执行单元中。

未来可能包含：

- 属性修改
- 授予 Tag
- 移除 Tag
- 持续时间
- 周期
- 层数上限

## 数据流

```text
Input / AI
 -> CombatWorld.Tick
 -> Ability 读取或修改 ICombatInfo
 -> Info version 变化
 -> RIL 根据已知 InfoHandle 读取变化
 -> Unity 表现层更新
```

## 约束

### World 不暴露全量 Info

外部代码不应该从 `CombatWorld` 枚举所有 `ICombatInfo`。

RIL 应该使用自己的索引和已知 `InfoHandle`。

### Info 是运行时实例

Info 是运行时数据，不是静态配置。

### 归属关系由外部决定

Core 不强制 `ICombatInfo` 归属于某个 `ActorId`。

如果某个功能需要 Actor -> Info 的关系，在业务层或专用 Info 中维护索引。

### InfoVersion 是脏标记

Info 版本号用于脏检测。

Ability 修改某份 Info 后，需要调用 `MarkInfoDirty`。

RIL 可以比较版本号，判断某个已知 Info 是否需要重新翻译。

### World 不写业务规则

不要把伤害计算、Buff Tick、命中结算或死亡逻辑放进 `CombatWorld`。

## 当前文件

- [CombatWorld.cs](../Assets/Scripts/Battle/Core/CombatWorld.cs)
- [CombatInfoStore.cs](../Assets/Scripts/Battle/Core/CombatInfoStore.cs)
- [ActorId.cs](../Assets/Scripts/Battle/Core/ActorId.cs)
- [AbilityHandle.cs](../Assets/Scripts/Battle/Core/AbilityHandle.cs)
- [InfoHandle.cs](../Assets/Scripts/Battle/Core/InfoHandle.cs)
- [CombatPhase.cs](../Assets/Scripts/Battle/Core/CombatPhase.cs)
- [CombatTime.cs](../Assets/Scripts/Battle/Core/CombatTime.cs)
- [IAbility.cs](../Assets/Scripts/Battle/Ability/IAbility.cs)
- [AbilityBase.cs](../Assets/Scripts/Battle/Ability/AbilityBase.cs)
- [ICombatInfo.cs](../Assets/Scripts/Battle/CombatInfo/ICombatInfo.cs)
- [TagAbility.cs](../Assets/Scripts/Battle/Ability/Tag/TagAbility.cs)
- [TagRequirement.cs](../Assets/Scripts/Battle/Ability/Tag/TagRequirement.cs)
- [TagId.cs](../Assets/Scripts/Battle/CombatInfo/Tag/TagId.cs)
- [TagSet.cs](../Assets/Scripts/Battle/CombatInfo/Tag/TagSet.cs)
- [TagContainerInfo.cs](../Assets/Scripts/Battle/CombatInfo/Tag/TagContainerInfo.cs)
- [EffectAbility.cs](../Assets/Scripts/Battle/Ability/Effects/EffectAbility.cs)
- [EffectSpec.cs](../Assets/Scripts/Battle/Ability/Effects/EffectSpec.cs)
- [EffectData.cs](../Assets/Scripts/Battle/CombatInfo/Effect/EffectData.cs)
- [EffectRuntimeInfo.cs](../Assets/Scripts/Battle/CombatInfo/Effect/EffectRuntimeInfo.cs)
- [EffectApplicationInfo.cs](../Assets/Scripts/Battle/CombatInfo/Effect/EffectApplicationInfo.cs)
- [EffectDurationPolicy.cs](../Assets/Scripts/Battle/CombatInfo/Effect/EffectDurationPolicy.cs)

## 后续建议

- 增加基础 Info，例如空间、属性、表现意图。
- 建立最小 RIL 索引，把 Unity View 绑定到已知 `InfoHandle`。
- 基于 Tag 做 Ability 激活协议。
- 在 `CombatWorld` 外实现 Effect 结算。
