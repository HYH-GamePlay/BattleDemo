# EventComp和AudioComp使用指南

## 一、EventComp事件组件

### 1. 核心功能

#### 添加事件监听器（带参数）
```csharp
// 添加事件监听器
EventComp.AddListener<int>(EventId.PlayerLevelUp, OnPlayerLevelUp);

private void OnPlayerLevelUp(int level)
{
    Debug.Log($"Player level up to: {level}");
}
```

#### 添加事件监听器（无参数）
```csharp
// 添加事件监听器
EventComp.AddListener(EventId.GameStart, OnGameStart);

private void OnGameStart()
{
    Debug.Log("Game started!");
}
```

#### 移除事件监听器
```csharp
// 移除事件监听器
EventComp.RemoveListener<int>(EventId.PlayerLevelUp, OnPlayerLevelUp);
EventComp.RemoveListener(EventId.GameStart, OnGameStart);
```

#### 广播事件
```csharp
// 广播事件（带参数）
EventComp.Broadcast(EventId.PlayerLevelUp, 10);

// 广播事件（无参数）
EventComp.Broadcast(EventId.GameStart);
```

### 2. 事件管理

#### 清理指定事件
```csharp
// 清理指定事件的所有监听器
EventComp.ClearEvent(EventId.PlayerLevelUp);
```

#### 清理所有事件
```csharp
// 清理所有事件监听器
EventComp.ClearAllEvents();
```

#### 检查事件状态
```csharp
// 检查事件是否有监听器
if (EventComp.HasListener(EventId.PlayerLevelUp))
{
    Debug.Log("Event has listeners!");
}

// 获取事件监听器数量
int count = EventComp.GetListenerCount(EventId.PlayerLevelUp);

// 获取所有事件数量
int eventCount = EventComp.GetEventCount();
```

### 3. 完整示例

```csharp
using GameCore.Core.Comp.Event;

public class EventExample : MonoBehaviour
{
    private void Start()
    {
        // 初始化
        EventComp.Instance.Init();

        // 添加事件监听器
        EventComp.AddListener<int>(EventId.PlayerLevelUp, OnPlayerLevelUp);
        EventComp.AddListener(EventId.GameStart, OnGameStart);
    }

    private void OnDestroy()
    {
        // 移除事件监听器
        EventComp.RemoveListener<int>(EventId.PlayerLevelUp, OnPlayerLevelUp);
        EventComp.RemoveListener(EventId.GameStart, OnGameStart);
    }

    private void OnPlayerLevelUp(int level)
    {
        Debug.Log($"Player level up to: {level}");
    }

    private void OnGameStart()
    {
        Debug.Log("Game started!");
    }

    public void LevelUp()
    {
        // 广播事件
        EventComp.Broadcast(EventId.PlayerLevelUp, 10);
    }

    public void StartGame()
    {
        // 广播事件
        EventComp.Broadcast(EventId.GameStart);
    }
}
```

---

## 二、AudioComp音频组件

### 1. 核心功能

#### 播放背景音乐
```csharp
// 播放背景音乐
AudioComp.PlayBGM("BGM_Main");

// 播放背景音乐（异步）
AudioComp.PlayBGMAsync("BGM_Main", () =>
{
    Debug.Log("BGM started!");
});
```

#### 控制背景音乐
```csharp
// 停止背景音乐
AudioComp.StopBGM();

// 暂停背景音乐
AudioComp.PauseBGM();

// 恢复背景音乐
AudioComp.ResumeBGM();
```

#### 播放音效
```csharp
// 播放音效
AudioComp.PlaySfx("SFX_ButtonClick");

// 播放音效（指定位置）
AudioComp.PlaySfxAtPosition("SFX_Explosion", transform.position);
```

#### 停止音效
```csharp
// 停止所有音效
AudioComp.StopAllSfx();
```

### 2. 音量控制

#### 设置音量
```csharp
// 设置总音量
AudioComp.SetVolume(0.8f);

// 设置背景音乐音量
AudioComp.SetBGMVolume(0.5f);

// 设置音效音量
AudioComp.SetSFXVolume(0.7f);
```

#### 获取音量
```csharp
// 获取背景音乐音量
float bgmVolume = AudioComp.GetBGMVolume();

// 获取音效音量
float sfxVolume = AudioComp.GetSFXVolume();
```

### 3. 音频管理

#### 预加载音频
```csharp
// 预加载音频
AudioClip bgmClip = Resources.Load<AudioClip>("Audio/BGM_Main");
AudioComp.PreloadAudio("BGM_Main", bgmClip);
```

#### 卸载音频
```csharp
// 卸载音频
AudioComp.UnloadAudio("BGM_Main");
```

#### 检查音频状态
```csharp
// 检查音频是否已加载
if (AudioComp.IsAudioLoaded("BGM_Main"))
{
    Debug.Log("Audio is loaded!");
}

// 检查背景音乐是否正在播放
if (AudioComp.IsBGMPlaying())
{
    Debug.Log("BGM is playing!");
}

// 获取已加载音频数量
int count = AudioComp.GetLoadedAudioCount();
```

### 4. 完整示例

```csharp
using GameCore.Core.Comp.Audio;
using UnityEngine;

public class AudioExample : MonoBehaviour
{
    private void Start()
    {
        // 初始化
        AudioComp.Instance.Init();

        // 预加载音频
        AudioClip bgmClip = Resources.Load<AudioClip>("Audio/BGM_Main");
        AudioComp.PreloadAudio("BGM_Main", bgmClip);

        AudioClip sfxClip = Resources.Load<AudioClip>("Audio/SFX_ButtonClick");
        AudioComp.PreloadAudio("SFX_ButtonClick", sfxClip);

        // 设置音量
        AudioComp.SetBGMVolume(0.5f);
        AudioComp.SetSFXVolume(0.8f);

        // 播放背景音乐
        AudioComp.PlayBGM("BGM_Main");
    }

    private void OnDestroy()
    {
        // 停止所有音频
        AudioComp.StopBGM();
        AudioComp.StopAllSfx();
    }

    public void OnButtonClick()
    {
        // 播放音效
        AudioComp.PlaySfx("SFX_ButtonClick");
    }

    public void PlayExplosion(Vector3 position)
    {
        // 播放音效（指定位置）
        AudioComp.PlaySfxAtPosition("SFX_Explosion", position);
    }
}
```

---

## 三、事件ID定义示例

```csharp
namespace GameLogic.Defines
{
    public static class EventId
    {
        // 玩家事件
        public const int PlayerLevelUp = 1000;
        public const int PlayerDeath = 1001;
        public const int PlayerRespawn = 1002;

        // 游戏事件
        public const int GameStart = 2000;
        public const int GamePause = 2001;
        public const int GameResume = 2002;
        public const int GameOver = 2003;

        // UI事件
        public const int UIOpen = 3000;
        public const int UIClose = 3001;

        // 战斗事件
        public const int BattleStart = 4000;
        public const int BattleEnd = 4001;
        public const int EnemyDeath = 4002;
        public const int BossDeath = 4003;
    }
}
```

---

## 四、音频资源管理建议

### 1. 音频命名规范

```
Audio/
├── BGM/
│   ├── BGM_Main.mp3
│   ├── BGM_Battle.mp3
│   └── BGM_Menu.mp3
├── SFX/
│   ├── SFX_ButtonClick.mp3
│   ├── SFX_Explosion.mp3
│   └── SFX_Footstep.mp3
└── Voice/
    ├── Voice_NPC_01.mp3
    └── Voice_NPC_02.mp3
```

### 2. 音频预加载

```csharp
// 在游戏启动时预加载常用音频
public void PreloadCommonAudio()
{
    // 预加载背景音乐
    AudioClip bgmMain = Resources.Load<AudioClip>("Audio/BGM/BGM_Main");
    AudioComp.PreloadAudio("BGM_Main", bgmMain);

    // 预加载常用音效
    AudioClip sfxClick = Resources.Load<AudioClip>("Audio/SFX/SFX_ButtonClick");
    AudioComp.PreloadAudio("SFX_ButtonClick", sfxClick);
}
```

### 3. 音频卸载

```csharp
// 在场景切换时卸载不需要的音频
public void UnloadSceneAudio()
{
    AudioComp.UnloadAudio("BGM_Main");
    AudioComp.UnloadAudio("SFX_ButtonClick");
}
```

---

## 五、性能优化建议

### 1. EventComp优化

- 及时移除不需要的事件监听器
- 避免在事件处理器中执行耗时操作
- 使用对象池管理事件数据

### 2. AudioComp优化

- 预加载常用音频
- 及时卸载不需要的音频
- 控制同时播放的音效数量
- 使用音频压缩格式

---

## 六、总结

### EventComp特性

1. **类型安全**: 支持泛型，类型安全
2. **灵活**: 支持带参数和无参数的事件
3. **高效**: 使用字典管理，查找快速
4. **易用**: API简洁，易于使用

### AudioComp特性

1. **功能完整**: 支持BGM、SFX、音量控制等
2. **性能优化**: 使用音频源池，减少创建开销
3. **易用**: API简洁，易于使用
4. **灵活**: 支持预加载、卸载、位置播放等

EventComp和AudioComp现在已经完善，可以支持游戏的所有事件和音频需求！
