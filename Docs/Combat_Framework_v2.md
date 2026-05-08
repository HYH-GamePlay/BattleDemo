# Combat Framework v2

## 0. 文档定位

本文是战斗框架设计文档，不代表当前工程代码已经按此实现。

本文目标是定义战斗框架的职责边界、运行时数据结构、流程顺序和扩展规则，作为后续重构、实现和 Review 的依据。

---

## 1. 架构目标

- 数据驱动：技能、效果、状态、命中、表现尽量由配置描述。
- Action 驱动：技能运行本质是 Action 时间轴调度。
- Tag 状态驱动：离散状态统一用 Tag 描述。
- Effect 规则驱动：伤害、治疗、Buff、Debuff、属性变化统一由 Effect 描述和执行。
- 无 `Skill.cs`：不为每个技能写独立脚本类。
- 无 `Ability` 类：技能实例状态归入 Action 运行时和 ActionBehaviorInfo。
- 技能完全配置化：技能逻辑由 `ActionData + EffectData + TagQuery + ExecutionData` 组合表达。
- 支持同步：核心逻辑基于固定帧、确定顺序、可复现输入。
- 支持回放：回放依赖输入、配置版本、随机种子和关键校验数据。
- 支持热更新：配置可热更，但运行中的 Action / Effect 使用稳定版本。

---

## 2. 核心原则

### 原则 1：技能不是代码

技能本质是 `ActionData`，不是 `Skill.cs`。

### 原则 2：Action 只负责调度

Action 负责时间推进和事件调度，不负责伤害计算、属性计算、状态计算。

### 原则 3：Effect 负责修改

Effect 负责对目标产生状态、属性、血量、护盾、位移等结果。

### 原则 4：Tag 负责描述状态

Tag 只描述状态，不承载业务逻辑。

### 原则 5：Behavior 负责系统逻辑

每个 Behavior 管理一个明确系统，避免跨系统侵入。

### 原则 6：BehaviorInfo 负责状态存储

BehaviorInfo 是可同步、可回放、可快照的运行时状态数据。

### 原则 7：运行时修改必须可追踪

Action、Effect、Tag、Modifier、Hitbox 等运行时对象必须有来源、句柄和生命周期，避免误删、漏删和不可回放。

---

## 3. 核心结构

```text
Actor
 ├── ActionBehavior
 ├── EffectBehavior
 ├── TagBehavior
 ├── MotionBehavior
 ├── CombatBehavior
 └── AnimationBehavior
```

`Actor` 是战斗实体容器。  
`Behavior` 是系统逻辑。  
`BehaviorInfo` 是系统状态数据。  

---

## 4. Actor

### 职责

- 维护 `ActorId`。
- 管理生命周期。
- 持有 Behavior 容器。
- 提供事件分发入口。
- 提供跨 Behavior 查询入口。

### 禁止

- 不写技能逻辑。
- 不写数值逻辑。
- 不写状态逻辑。
- 不直接处理动画、命中、Buff、伤害。

---

## 5. Behavior 基类

```csharp
public abstract class Behavior<TInfo>
{
    public Actor Owner;
    public TInfo Info;

    public virtual void Initialize() {}
    public virtual void Tick(int frame) {}
    public virtual void Dispose() {}
}
```

说明：

- 战斗逻辑优先使用固定帧 `frame`。
- 表现层可以使用 `deltaTime` 插值，但不能影响权威战斗结果。
- `Info` 中的数据需要尽量可序列化，用于同步、回放、热更恢复和调试。

---

## 6. 运行时上下文

### ActionContext

描述一次 Action 的来源。

```csharp
public struct ActionContext
{
    public long ActionRuntimeId;
    public int SourceActorId;
    public int ActionId;
    public int ConfigVersion;
    public int StartFrame;
    public uint RandomSeed;
}
```

### ActionRuntime

描述一次正在运行的 Action。

```csharp
public class ActionRuntime
{
    public ActionContext Context;
    public ActionData Data;
    public int CurrentFrame;
    public bool IsFinished;
    public bool IsCanceled;
}
```

### EffectSpec

描述一次即将施加的 Effect 请求。

```csharp
public struct EffectSpec
{
    public int EffectId;
    public int SourceActorId;
    public int TargetActorId;
    public long SourceActionRuntimeId;
    public int ConfigVersion;
    public int ApplyFrame;
}
```

### EffectRuntime

描述一个已经生效的持续型 Effect。

```csharp
public class EffectRuntime
{
    public long EffectRuntimeId;
    public EffectSpec Spec;
    public EffectData Data;
    public int StartFrame;
    public int EndFrame;
    public int StackCount;
}
```

### TagHandle

描述一次 Tag 添加来源。

```csharp
public struct TagHandle
{
    public long HandleId;
    public GameplayTag Tag;
    public int SourceActorId;
    public long SourceRuntimeId;
}
```

Tag 移除必须优先通过 `TagHandle`，避免误删其他来源添加的相同 Tag。

---

## 7. TagBehavior

### 职责

- Tag 添加、移除、引用计数。
- Tag Query。
- 状态描述。
- Tag 来源追踪。
- 生命周期清理。

### 禁止

- 不做数值计算。
- 不做 Timeline 调度。
- 不做 Action 调度。
- 不决定状态对应的业务结果。

### Tag 示例

```text
State.Dead
State.Stun
State.Casting
State.SuperArmor
State.Invincible
State.CanCombo
State.Grounded
State.Silence
```

### TagBehaviorInfo

```csharp
public class TagBehaviorInfo
{
    public Dictionary<GameplayTag, int> TagCounts;
    public Dictionary<long, TagHandle> Handles;
}
```

### Tag 规则

- 支持引用计数。
- 支持来源句柄。
- Tag 不负责逻辑。
- 所有离散状态统一使用 Tag。
- Action 添加的临时 Tag 在 Action 结束、取消、打断时必须自动清理。
- Effect 添加的 Tag 在 Effect 结束、驱散、覆盖时必须自动清理。

### TagQuery

用于技能释放条件、状态判断和 AI 判断。

```text
ALL:
    State.Grounded

NONE:
    State.Stun
    State.Dead
```

建议支持：

```text
ALL
ANY
NONE
```

---

## 8. ActionBehavior

### 职责

- Action 生命周期。
- Action Tick。
- Instruct 调度。
- InstructData 执行器分发。
- 输入缓存。
- Cancel。
- Chain。
- Action Link 窗口。
- Action 临时资源清理。

### 禁止

- 不做数值逻辑。
- 不做 Effect 内部逻辑。
- 不做动画状态逻辑。
- 不直接修改血量、属性、护盾。
- 不维护一套通用 Condition / Signal 脚本系统。

### Action 定义

技能本质：

```text
ActionData
```

不是：

```text
Skill.cs
```

说明：

- `ActionData` 是可序列化的数据类，当前项目序列化方案使用 MessagePack。
- `ActionData` 不保存 `ActionId`、`Version` 等配置身份信息。
- 配置 id、版本、等级、CD、资源消耗等属于 Excel 或外层 Action 配置表。
- `ActionData` 只描述一段 Action 内部的时间线指令。

### Action 结构

```text
ActionData
 └── Instruct[]
      ├── AnimationData
      ├── AddTagData
      ├── RemoveTagData
      ├── CollisionData
      ├── ApplyEffectData
      ├── MotionData
      ├── CreateBulletData
      ├── AudioData
      ├── CameraData
      ├── VfxData
      └── ActionLinkData
```

不再使用：

```text
Track / Clip / ActionEvent
Signal / SparkInstruct
Condition
```

### ActionData

```csharp
[MessagePackObject(true)]
public sealed class ActionData
{
    // 根据 instructs 的最大 end 计算得出。
    public uint length { get; set; }

    public List<Instruct> instructs { get; set; } = new();
}
```

### ActionRawData

`ActionData` 是编辑期和运行期对象结构。为了避免 MessagePack 直接序列化抽象 `InstructData` 带来的 AOT / Resolver 风险，落盘和热更包建议使用 `ActionRawData`。

```csharp
[MessagePackObject(true)]
public sealed class ActionRawData
{
    public uint length { get; set; }

    public uint[] begin { get; set; }
    public uint[] end { get; set; }

    public ushort[] instructTypes { get; set; }
    public byte[][] instructData { get; set; }
}
```

转换规则：

```text
ActionData
    ↓ ToRaw
ActionRawData
    ↓ MessagePack
bytes
```

运行时加载：

```text
bytes
    ↓ MessagePack
ActionRawData
    ↓ FromRaw
ActionData
```

### Instruct

`Instruct` 是 Action 时间线上的最小调度单元，只描述时间区间和指令数据。

```csharp
[MessagePackObject(true)]
public sealed class Instruct
{
    // 区间 [begin, end)
    public uint begin { get; set; }
    public uint end { get; set; }

    public InstructData data { get; set; }
}
```

运行语义：

```text
begin 到达：Executor.Enter
begin <= time < end：Executor.Execute
end 到达：Executor.Exit
Action 结束 / 取消 / 中断：Executor.Exit 并清理运行时资源
```

瞬时事件不需要单独的 Signal，用一帧 `Instruct` 表达：

```text
begin = 12
end = 13
data = CreateBulletData
```

持续窗口使用多帧 `Instruct` 表达：

```text
begin = 18
end = 32
data = ActionLinkData
```

### InstructData

`InstructData` 描述具体行为，Executor 根据 `id` 分发执行。

```csharp
[MessagePackObject(true)]
public abstract class InstructData
{
    [IgnoreMember]
    public abstract InstructType id { get; }

    // 执行目标，例如 Action 拥有者、当前目标、命中目标等。
    public ExecuteTarget et = ExecuteTarget.ActionOwner;
}
```

```csharp
public enum InstructType : ushort
{
    None = 0,

    Animation = 1,
    AddTag = 2,
    RemoveTag = 3,
    Collision = 4,
    ApplyEffect = 5,
    Motion = 6,
    CreateBullet = 7,
    Audio = 8,
    Camera = 9,
    Vfx = 10,
    ActionLink = 11,
}
```

示例：

```csharp
[MessagePackObject(true)]
public sealed class AnimationData : InstructData
{
    public override InstructType id => InstructType.Animation;

    public int animation;
}
```

```csharp
[MessagePackObject(true)]
public sealed class CollisionData : InstructData
{
    public override InstructType id => InstructType.Collision;

    public int hitbox;
    public int targetFilter;
    public List<int> effects = new();
}
```

```csharp
[MessagePackObject(true)]
public sealed class ApplyEffectData : InstructData
{
    public override InstructType id => InstructType.ApplyEffect;

    public List<int> effects = new();
}
```

### Condition 与 Signal

当前设计不保留通用 `Condition`。

原因：

- 通用 Condition 容易把 Action 配置变成隐式脚本语言。
- 大多数判断应下沉到明确的 InstructData 内部。
- 技能释放条件、CD、资源、TagQuery 属于 Excel 或外层 Action 配置表。
- 命中过滤、命中次数、目标类型属于 `CollisionData`。
- 连招输入和派生属于 `ActionLinkData`。

当前设计不保留 `Signal / SparkInstruct`。

原因：

- token / influence 机制容易导致配置关系隐式化。
- 谁发信号、谁接信号、接收范围是什么，配置量上来后难以追踪。
- 瞬时触发可以用一帧 `Instruct` 表达。
- 命中后触发的逻辑应由 `CollisionData`、`ApplyEffectData` 或明确的执行流程表达。

### Action 规则

- Action 只推进时间和调度 Instruct。
- Action 不计算伤害。
- Action 不计算属性。
- Action 不解释 Tag 的业务含义。
- Action 可以通过 Instruct 添加自身运行所需的临时 Tag，例如 `State.Casting`、`State.CanCombo`。
- Action 添加的 Tag 必须绑定 `ActionRuntimeId`。
- Action 被取消、中断或结束时，必须清理自身创建的临时 Tag、Hitbox、窗口状态。

### 连招系统

连招不依赖通用 `Condition`，而由 `ActionLinkData + ActionBehavior` 统一处理。

```csharp
[MessagePackObject(true)]
public sealed class ActionLinkData : InstructData
{
    public override InstructType id => InstructType.ActionLink;

    public List<ActionLinkBranchData> branches = new();
    public bool consumeInput = true;
}
```

```csharp
[MessagePackObject(true)]
public sealed class ActionLinkBranchData
{
    public int input;
    public int nextAction;
    public int priority;
    public ActionTransitionPolicy transition;
}
```

运行流程：

```text
ActionLinkData 片段进入窗口
    ↓
ActionBehavior 检查输入缓存
    ↓
匹配 branch.input
    ↓
调用 CanPlay(nextAction)
    ↓
成功后立即切换或记录 PendingAction
```

`CanPlay(nextAction)` 读取外层 Action 配置，统一检查 CD、资源、TagQuery、死亡、沉默等释放条件。

---

## 9. EffectBehavior

### 职责

- Effect 生命周期。
- Effect Tick。
- Modifier 管理。
- 状态修改。
- 持续型 Effect 管理。
- Buff / Debuff 叠层、刷新、移除。

### 禁止

- 不做 Action 调度。
- 不做动画控制。
- 不做命中检测。

### Effect 定义

Effect 是统一的状态和结果修改系统。

### Effect 包含

```text
Damage
Heal
DOT
HOT
Shield
KnockUp
KnockBack
Slow
Haste
SuperArmor
Silence
Tag 修改
Modifier 修改
```

### Buff

```text
Buff = 持续型 Effect
```

不存在独立 Buff 系统。

### EffectData

```csharp
public class EffectData
{
    public int DurationFrames;
    public int PeriodFrames;

    public List<GameplayTag> AddTags;
    public List<GameplayTag> RemoveTags;

    public List<ModifierData> Modifiers;
    public ExecutionData Execution;

    public StackPolicy StackPolicy;
}
```

### Effect 类型

```text
InstantEffect      立即生效
DurationEffect     持续存在
PeriodicEffect     按周期触发
InfiniteEffect     无限持续，直到被移除
```

### 叠层规则

Effect 必须配置明确的叠层策略。

```text
StackPolicy
 ├── MaxStack
 ├── SameSourcePolicy
 ├── DifferentSourcePolicy
 ├── DurationRefreshPolicy
 └── PeriodResetPolicy
```

常见策略：

```text
IgnoreNew
RefreshDuration
AddStack
ReplaceOld
Independent
```

---

## 10. Modifier 与 Stat

### Modifier

Modifier 描述对属性的修改。

```csharp
public enum ModifierOp
{
    Add,
    Multiply,
    Override
}
```

建议后续扩展为更明确的计算阶段：

```text
Base
Add
PercentAdd
Multiply
Override
FinalClamp
```

### Stat

Stat 是数值聚合结果，不是独立系统生命周期。

### Stat 禁止

- 不作为独立 Behavior。
- 不拥有独立 Tick。
- 不承载技能逻辑。

### Stat 规则

- Stat 由基础值和 Modifier 聚合得到。
- Modifier 来源必须可追踪。
- Effect 结束时必须移除对应 Modifier。
- 多个 Modifier 的计算顺序必须固定。
- 多个 Override 的优先级必须固定。
- 建议使用 dirty 标记，在需要读取或帧末统一重算。

示例计算顺序：

```text
FinalValue =
    (((Base + AddSum) * (1 + PercentAddSum)) * MultiplyProduct)
    -> ApplyOverride
    -> Clamp
```

---

## 11. CombatBehavior

### 职责

- Hitbox。
- Hurtbox。
- 目标筛选。
- 阵营判断。
- 命中流程。
- Damage Flow。
- 命中去重。
- 命中结果生成。

### 禁止

- 不做 Action 调度。
- 不做动画逻辑。
- 不直接播放表现。

### Hitbox

Hitbox 由 `CollisionData` 或相关 Instruct 创建和控制。

```text
CollisionData
HitboxData
```

### HitboxRuntime

```csharp
public class HitboxRuntime
{
    public long HitboxRuntimeId;
    public int SourceActorId;
    public long SourceActionRuntimeId;
    public int HitboxId;
    public HashSet<int> HitActors;
}
```

### Hitbox 规则

- Hitbox 必须绑定来源 Action。
- Action 结束、取消、中断时必须销毁所属 Hitbox。
- Hitbox 需要配置阵营过滤。
- Hitbox 需要配置是否多次命中同一目标。
- 多段命中需要配置命中间隔。
- 命中死亡、无敌、不可选中目标时的行为必须明确。

---

## 12. Damage Flow

命中类 Effect 推荐流程：

```text
Instruct
    ↓
CombatBehavior 生成命中结果
    ↓
EffectBehavior.ApplyEffect
    ↓
Execution 计算最终结果
    ↓
写入目标状态 / 属性 / 血量 / 护盾 / 位移请求
```

非命中类 Effect 推荐流程：

```text
Instruct
    ↓
EffectBehavior.ApplyEffect
    ↓
Execution 计算最终结果
    ↓
写入目标状态 / 属性 / 血量 / 护盾 / 位移请求
```

说明：

- CombatBehavior 负责“能不能命中”和“命中了谁”。
- Execution 负责“命中后产生多少结果”。
- EffectBehavior 负责“结果如何进入目标状态”。
- MotionBehavior 负责实际移动执行。
- AnimationBehavior 负责表现播放。

---

## 13. Execution

### 职责

- 最终伤害计算。
- 最终治疗计算。
- 最终护盾计算。
- 读取 Source / Target Stat。
- 生成可应用的结果。

### 示例

```text
DamageExecution
HealExecution
ShieldExecution
DotExecution
HotExecution
```

### Execution 规则

- Execution 不拥有生命周期。
- Execution 不调度 Action。
- Execution 不播放动画。
- Execution 不直接创建 Hitbox。
- Execution 可以读取 Stat、Tag、Effect 状态。
- Execution 输出结果对象，由对应 Behavior 应用。

---

## 14. MotionBehavior

### 职责

- Move。
- 位移请求执行。
- 击退、击飞、拉拽等运动结果处理。
- 移动同步。

### 禁止

- 不写技能逻辑。
- 不解释状态逻辑。
- 不计算伤害。

说明：

- Action 可以通过 `MotionData` Instruct 发起位移请求。
- Effect 可以通过 Execution 产生击退、击飞等位移请求。
- MotionBehavior 统一处理最终移动结果。

---

## 15. AnimationBehavior

### 职责

- 动画播放。
- 动画同步。
- 动画状态管理。
- 动画事件桥接。

### 禁止

- 不写技能逻辑。
- 不写数值逻辑。
- 不决定命中结果。

说明：

- Action 通过 `AnimationData` Instruct 调度动画播放。
- AnimationBehavior 负责表现执行。
- 动画事件不能作为权威战斗逻辑来源，权威战斗事件应来自 ActionData。

---

## 16. 技能运行流程

```text
PlayAction(actionId)
    ↓
检查 TagQuery / CD / 资源 / 输入条件
    ↓
创建 ActionContext
    ↓
创建 ActionRuntime
    ↓
进入 ActionBehaviorInfo
    ↓
按固定帧 Tick Action
    ↓
调度 Instruct
    ↓
Action 结束 / 取消 / 中断
    ↓
清理 Action 运行时资源
```

---

## 17. 每帧 Tick 顺序

建议固定顺序：

```text
1. 收集输入
2. ActionBehavior Tick
3. CombatBehavior Tick / 命中检测
4. EffectBehavior Tick / Effect 应用
5. Stat 聚合刷新
6. Tag 变更通知
7. MotionBehavior Tick
8. AnimationBehavior Tick
9. 帧末事件清理和校验
```

规则：

- 同一帧内的事件顺序必须稳定。
- 同一 Actor 多个事件的排序必须稳定。
- 跨 Actor 事件建议按 `ActorId` 或事件序号排序。
- 所有权威战斗逻辑基于固定帧。

---

## 18. 同步与回放

### 同步核心

```text
Input
Action
Tag
Effect
Stat
CombatResult
RandomSeed
ConfigVersion
```

### 同步规则

- 权威逻辑使用固定帧。
- 随机数必须使用可复现随机流。
- Action / Effect / Hitbox / Modifier 必须有稳定 RuntimeId。
- 配置必须带版本号。
- 同一输入和同一配置版本必须得到相同结果。
- 表现层不能反向影响权威逻辑。

### 回放记录

建议记录：

```text
FrameInput
ActionStart
ActionCancel
EffectApply
CombatResult
RandomSeed
ConfigVersion
StateChecksum
```

回放可以只记录输入，但开发期建议记录关键事件和校验值，方便定位不同步。

---

## 19. 热更新规则

### 配置热更新

- 新启动的 Action 使用最新配置版本。
- 已经运行中的 Action 使用创建时绑定的配置版本。
- 已经运行中的 Effect 使用创建时绑定的配置版本。
- 回放必须加载当时使用的配置版本。

### 禁止

- 禁止运行中 Action 自动切换到新配置。
- 禁止持续型 Effect 自动切换到新配置。
- 禁止回放使用不匹配的配置版本。

---

## 20. 技能编辑器

### 编辑器层

```text
Timeline Editor
```

### 导出

```text
ActionData
```

### 编辑器职责

- 编辑 Instruct 时间片段。
- 编辑 ActionLink 连招窗口。
- 编辑 Hitbox 时间段。
- 编辑 Effect 引用。
- 编辑 Tag 添加和移除。
- 编辑动画、音效、镜头等表现事件。

### 导出校验

导出时需要校验：

- 引用的 Effect 是否存在。
- 引用的 Tag 是否存在。
- Instruct 时间区间是否有效。
- Instruct 是否存在非法时间或不允许的重叠。
- Hitbox 是否有关闭时机。
- AddTag 是否有对应清理策略。
- ActionLink 是否指向合法 Action。
- 配置版本是否正确生成。

---

## 21. 待确认设计点

以下内容需要在实现前继续确认：

- CD、资源、充能、技能等级、解锁状态具体放在 `ActionBehaviorInfo` 的哪种结构中。
- Damage / Heal / Shield 是否全部通过 Execution，还是允许部分简单 Effect 直接应用。
- Tag 命名规范是否采用层级字符串，例如 `State.Stun`、`Action.CanCombo`。
- Modifier 是否需要优先级、来源类型和显示层分类。
- Motion 是完全逻辑同步，还是部分表现插值。
- Combat 命中结果是完全本地计算，还是服务端权威下发。
- 热更新配置版本如何存储和回滚。

---

## 22. 最小实现里程碑

### 阶段 1：骨架

- Actor。
- Behavior 基类。
- BehaviorInfo。
- TagBehavior。
- ActionBehavior。
- 固定帧 Tick。

### 阶段 2：技能调度

- ActionData。
- Instruct / InstructData。
- ActionRuntime。
- ActionLink。
- 临时 Tag 清理。

### 阶段 3：Effect 与数值

- EffectData。
- EffectRuntime。
- Modifier。
- Stat 聚合。
- Execution。

### 阶段 4：命中流程

- Hitbox。
- Hurtbox。
- CombatBehavior。
- Damage Flow。
- 命中去重。

### 阶段 5：同步与工具

- RuntimeId。
- ConfigVersion。
- Replay。
- StateChecksum。
- Timeline Editor 导出校验。
