# Animation Framework

这套动画框架位于 `Assets/Scripts/GameAnimation`，定位是角色表现层。它负责动画播放、分层融合、打断仲裁、Root Motion 输出和动画事件转发；技能、伤害等玩法规则不写进动画系统。

## 如何使用

### 1. 角色 Prefab 组件

在角色 Prefab 上添加：

- `Animator`
- `AnimancerComponent`
- `CharacterAnimationActor`
- 可选：`RootMotionDriver`
- 可选：`AnimationEventBridge`

`CharacterAnimationActor` 是对外门面。玩法层、输入层或 AI 只需要调用它，不需要直接操作 Animancer。

```csharp
var result = animationActor.PlayAnimation(100101);
if (!result.Accepted)
{
    // 根据 result.FailureReason 做降级处理。
}
```

移动动画输入使用：

```csharp
var input = AnimationLocomotionInput.FromWorldVelocity(
    velocity,
    animationActor.transform,
    isGrounded: true,
    isLockedOn: false);

animationActor.SetLocomotionInput(input);
```

外部玩法系统可以通过 `CharacterAnimationActor.PlayAnimation(animationId)` 请求动画播放，动画系统只返回播放结果和动画事件。

### 2. 创建 CharacterAnimationProfile

在 Project 窗口创建：

`Create/Game Animation/Character Animation Profile`

Profile 是动画数据入口，包含三类配置：

- `Layers`：定义动画层、Animancer layer index、AvatarMask、是否 Additive、默认权重和默认淡入时长。
- `Locomotion`：定义移动混合方式。支持 `LinearSpeed`、`Directional2D`、`FallbackAnimation`。
- `Animations`：定义动作动画条目，每个条目使用稳定的 `animationId`。

推荐默认层级：

| Layer | Index | 用途 |
| --- | ---: | --- |
| `Locomotion` | 0 | 待机、走、跑、锁定移动 |
| `Action` | 1 | 攻击、翻滚、技能等全身动作 |
| `UpperBody` | 2 | 上半身射击、施法、受击叠加 |
| `Reaction` | 3 | 受击、击飞、处决等高优先级表现 |
| `Additive` | 4 | 呼吸、瞄准、轻微姿态修正 |

### 3. 配置 Locomotion

如果是普通 ARPG 移动，使用 `LinearSpeed`：

- 在 `linearSpeedMixer` 中配置 idle/walk/run/sprint clips。
- threshold 建议用真实速度，例如 `0, 1.8, 4.2, 6.0`。
- 运行时调用 `SetLocomotionInput`，框架会平滑 speed 参数。

如果是锁定视角或八方向移动，使用 `Directional2D`：

- 在 `directionalMixer` 中配置前后左右和斜向 clips。
- threshold 使用本地坐标速度：`x` 表示横向，`y` 表示前后。

### 4. 播放动作动画

简单播放：

```csharp
animationActor.PlayAnimation(200101);
```

带覆盖参数播放：

```csharp
var command = AnimationCommand.Create(200101);
command.hasLayerOverride = true;
command.layerOverride = AnimationLayerType.UpperBody;
command.hasPriorityOverride = true;
command.priorityOverride = 50;
command.requiredCancelGroupId = 1;

animationActor.PlayAnimation(command);
```

动作能否播放由当前层状态判断：

- 当前层空闲：允许播放。
- 当前动画可打断：新动画优先级大于等于当前优先级才允许。
- 当前动画不可打断：新动画必须优先级更高。
- 如果 `requiredCancelGroupId` 不为 0，当前动画必须处在对应 cancel window 内。
- 如果 `ignoreInterruption` 为 true，强制播放，通常只给死亡、处决、剧情控制使用。

### 5. 动画事件

框架支持两种事件来源：

- 数据事件：在 `AnimationDefinition.eventMarkers` 中按 normalized time 配置。
- Unity Animation Event：动画 clip 上调用 `AnimationEventBridge.DispatchAnimationEvent(int eventId)`。

监听事件：

```csharp
animationActor.AnimationEventTriggered += OnAnimationEvent;

void OnAnimationEvent(AnimationEventContext context)
{
    if (context.EventType == AnimationEventType.HitboxWindow)
    {
        // 表现层只转发语义事件，真正的命中/伤害仍由战斗系统处理。
    }
}
```

### 6. Root Motion

`AnimationDefinition.rootMotionPolicy` 和 `LocomotionDefinition.rootMotionPolicy` 控制 Root Motion：

- `Ignore`：忽略 Animator delta，适合完全由逻辑层移动。
- `AccumulateForConsumer`：缓存 delta，由角色控制器在合适时机消费。
- `ApplyToTransform`：动画层直接移动 Transform，适合处决、剧情、纯表现对象。

消费 Root Motion：

```csharp
var delta = animationActor.ConsumeRootMotion();
controller.Move(delta.DeltaPosition);
transform.rotation *= delta.DeltaRotation;
```

## 如何拓展

### 新增动画层

在 `AnimationLayerType` 中增加枚举值，然后在 Profile 的 `Layers` 中配置 layer index 和 AvatarMask。不要直接在业务代码里写 Animancer layer index，业务代码只认 `AnimationLayerType`。

### 新增动作类型

新增一条 `AnimationDefinition`，配置：

- `animationId`
- `clip`
- `layer`
- `priority`
- `canBeInterrupted`
- `cancelWindows`
- `eventMarkers`

业务系统只传 `animationId`，不关心 clip、fade、layer、mask。

### 新增动画事件类型

在 `AnimationEventType` 中增加明确语义，例如 `ParryWindow`、`ChargeComplete`、`ProjectileSpawn`。事件载荷使用 `eventId` 和 `payloadId`，避免用不透明字符串。

### 接入玩法系统

推荐方向是玩法系统输出动画请求或角色状态，表现层监听后调用 `CharacterAnimationActor`：

- 技能激活：玩法系统触发 `PlayAnimation(abilityAnimationId)`。
- 命中窗口：动画事件发出 `HitboxWindow`，桥接层再通知玩法系统处理判定。
- 受击/死亡：玩法事件转成 Reaction/Death animation id。

不要让动画系统直接计算伤害、查目标或修改属性。

### 接入 IK / 程序化修正

新增独立组件，例如 `CharacterAnimationIkController`，监听 `CharacterAnimationActor.AnimationEventTriggered` 或读取当前动作层状态。IK 不应写进播放仲裁器，避免以后手部 IK、脚底 IK、瞄准 IK 互相污染。

### 自定义 Locomotion

如果 `LinearSpeed` 和 `Directional2D` 不够，可以新增 locomotion controller：

- 保留 `CharacterAnimationActor.SetLocomotionInput` 作为输入口。
- 新 controller 只负责把输入映射到 Animancer mixer 参数。
- 不改变动作播放、打断、Root Motion、事件调度模块。

## 实现说明

### 目录结构

```text
Assets/Scripts/GameAnimation
  Core
    AnimationCommand.cs
    AnimationEventContext.cs
    AnimationLayerType.cs
    AnimationLocomotionInput.cs
    AnimationPlaybackResult.cs
    AnimationRootMotionPolicy.cs
  Data
    CharacterAnimationProfile.cs
    AnimationDefinition.cs
    AnimationLayerDefinition.cs
    LocomotionDefinition.cs
    AnimationEventMarker.cs
    AnimationCancelWindow.cs
  Runtime
    AnimationLayerPlaybackState.cs
    AnimationEventScheduler.cs
  UnityBridge
    CharacterAnimationActor.cs
    RootMotionDriver.cs
    AnimationEventBridge.cs
```

### 模块职责

`Core` 是对外协议层：命令、结果、事件上下文、Root Motion 策略、层枚举。

`Data` 是配置层：Profile、动画条目、层定义、移动混合、事件点和取消窗口。

`Runtime` 是纯运行时规则：每层当前动画状态、优先级/打断判断、事件点调度。

`UnityBridge` 是 Unity/Animancer 桥接：真正播放 clip/mixer，收集 Root Motion，接收 Unity Animation Event。

### 设计边界

动画框架只处理表现，不拥有玩法规则。它可以告诉外部“动画播放到 hitbox window 事件点了”，但不会自己做命中判定；它可以播放受击动画，但不会自己扣血；它可以缓存 Root Motion，但最终移动由角色控制器或逻辑层决定。

这样后续扩展技能、武器、Buff、怪物 AI、网络同步时，动画系统不会变成第二套玩法系统。
