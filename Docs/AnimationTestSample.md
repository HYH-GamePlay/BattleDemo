# GameAnimation 测试样例

## 测试场景

`Assets/Res/Scenes/AnimationTest.unity`

## 使用方法

### 方式一：自动配置（推荐）

1. 打开 `AnimationTest.unity` 场景
2. 场景中的 `TestCharacter` 物体已挂载 `AnimationTestSetup` 组件
3. 运行场景，组件会自动创建所有需要的动画组件和 Profile

### 方式二：手动配置

1. 创建一个 GameObject
2. 添加以下组件：
   - `Animator`
   - `AnimancerComponent`
   - `CharacterAnimationActor`
   - `RootMotionDriver`（可选）
   - `AnimationTestBootstrap`
3. 创建 `CharacterAnimationProfile` 并配置动画
4. 将 Profile 赋值给 `CharacterAnimationActor`

## 控制键位

| 按键 | 功能 |
|------|------|
| WASD | 移动 |
| Shift | 跑步 |
| J | 攻击 |
| Space | 跳跃 |

## 程序化动画

测试样例使用 `TestAnimationFactory` 在运行时创建简单的程序化动画：

- `Idle` - 轻微呼吸起伏
- `Walk` - 行走弹跳
- `Run` - 跑步弹跳（更明显）
- `Attack` - 前刺攻击动作
- `Jump` - 跳跃弧线动作

## 文件结构

```
Assets/Scripts/GameAnimation/Samples/
├── AnimationTestSetup.cs        # 自动配置测试组件
├── AnimationTestBootstrap.cs    # 手动配置测试组件
├── TestAnimationFactory.cs      # 程序化动画创建
└── TestAnimationProfileFactory.cs # 运行时 Profile 创建
```

## 动画 ID 定义

| ID | 动画 |
|----|------|
| 1001 | Idle |
| 1002 | Walk |
| 1003 | Run |
| 2001 | Attack |
| 2002 | Jump |
