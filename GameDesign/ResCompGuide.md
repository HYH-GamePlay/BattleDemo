# ResComp资源组件使用指南

## 一、ResComp简介

ResComp是基于YooAsset的资源管理组件，提供完整的资源加载、卸载、缓存等功能。

## 二、核心功能

### 1. 资源加载

#### 同步加载
```csharp
// 加载资源
var texture = ResComp.Load<Texture>("Textures/Icon");
var prefab = ResComp.Load<GameObject>("Prefabs/UI/MainUI");
```

#### 异步加载（回调版本）
```csharp
// 异步加载资源
ResComp.LoadAsync<Texture>("Textures/Icon", (texture) =>
{
    if (texture != null)
    {
        Debug.Log("Texture loaded successfully!");
    }
    else
    {
        Debug.LogError("Failed to load texture!");
    }
});
```

#### 异步加载（UniTask版本）
```csharp
// 异步加载资源
var texture = await ResComp.LoadAsync<Texture>("Textures/Icon");
if (texture != null)
{
    Debug.Log("Texture loaded successfully!");
}
else
{
    Debug.LogError("Failed to load texture!");
}
```

### 2. 资源释放

#### 释放指定资源
```csharp
// 释放资源
ResComp.Release("Textures/Icon");
ResComp.Release("Prefabs/UI/MainUI");
```

#### 释放GameObject
```csharp
// 释放GameObject
ResComp.Release(gameObject);
```

#### 释放所有资源
```csharp
// 释放所有资源
ResComp.ReleaseAll();
```

### 3. 资源预加载

#### 预加载资源
```csharp
// 预加载资源
ResComp.PreloadAsset("Textures/Icon", 0);
ResComp.PreloadAsset("Prefabs/UI/MainUI", 1);
```

#### 异步预加载资源
```csharp
// 异步预加载资源
ResComp.PreloadAssetAsync("Textures/Icon", (object) =>
{
    Debug.Log("Asset preloaded successfully!");
}, 0);
```

### 4. 资源管理

#### 卸载未使用的资源
```csharp
// 卸载未使用的资源
ResComp.UnloadUnusedAssets();
```

#### 检查资源加载状态
```csharp
// 获取资源加载状态
string status = ResComp.GetAssetLoadStatus("Textures/Icon");
Debug.Log($"Asset status: {status}");
```

#### 检查资源是否已加载
```csharp
// 检查资源是否已加载
if (ResComp.IsAssetLoaded("Textures/Icon"))
{
    Debug.Log("Asset is loaded!");
}
```

#### 获取已加载资源数量
```csharp
// 获取已加载资源数量
int count = ResComp.GetLoadedAssetCount();
Debug.Log($"Loaded assets: {count}");
```

#### 获取YooAssetManager实例
```csharp
// 获取YooAssetManager实例
var yooAssetManager = ResComp.GetYooAssetManager();
```

## 三、使用示例

### 1. 初始化

```csharp
// 在GameEntry中初始化
private void Init()
{
    ServiceLocator.Register<IResComp>(new ResComp());
    ResComp.Instance.Init();
}
```

### 2. 加载UI预制体

```csharp
// 加载UI预制体
ResComp.LoadAsync<GameObject>("Prefabs/UI/MainUI", (prefab) =>
{
    if (prefab != null)
    {
        var uiObject = GameObject.Instantiate(prefab);
        uiObject.SetActive(true);
    }
});
```

### 3. 使用UniTask加载资源

```csharp
// 使用UniTask加载资源
public async void LoadResourceExample()
{
    var texture = await ResComp.LoadAsync<Texture>("Textures/Icon");
    if (texture != null)
    {
        Debug.Log("Texture loaded!");
        // 使用纹理
    }
}
```

### 4. 释放资源示例

```csharp
// 释放资源
public void ReleaseResourceExample()
{
    // 释放指定资源
    ResComp.Release("Textures/Icon");

    // 释放GameObject
    ResComp.Release(gameObject);

    // 卸载未使用的资源
    ResComp.UnloadUnusedAssets();
}
```

## 四、资源路径规范

### 1. 资源路径格式

```
Assets/Res/[资源类型]/[资源名称]
```

示例：
- `Assets/Res/Textures/Icon.png`
- `Assets/Res/Prefabs/UI/MainUI.prefab`
- `Assets/Res/Audio/Music/BGM.mp3`

### 2. 资源类型分类

- `Textures`: 纹理资源
- `Prefabs`: 预制体资源
- `Audio`: 音频资源
- `Models`: 模型资源
- `Materials`: 材质资源
- `Animations`: 动画资源

## 五、缓存机制

### 1. 资源缓存

ResComp会自动缓存已加载的资源：

- 同步加载的资源会被缓存
- 异步加载的资源会被缓存
- GameObject类型的资源也会被缓存

### 2. 缓存优势

- 减少重复加载时间
- 提高资源访问速度
- 降低内存占用

### 3. 缓存清理

- 手动调用 `Release()` 清理缓存
- 手动调用 `ReleaseAll()` 清理所有缓存
- 调用 `UnloadUnusedAssets()` 卸载未使用的资源

## 六、性能优化建议

### 1. 合理使用预加载

```csharp
// 预加载常用资源
ResComp.PreloadAsset("Textures/Icon", 0);
ResComp.PreloadAsset("Prefabs/UI/MainUI", 1);
```

### 2. 及时释放资源

```csharp
// 使用完资源后及时释放
ResComp.Release("Textures/Icon");
```

### 3. 卸载未使用的资源

```csharp
// 定期卸载未使用的资源
ResComp.UnloadUnusedAssets();
```

### 4. 使用缓存

```csharp
// 优先使用缓存资源
if (ResComp.IsAssetLoaded("Textures/Icon"))
{
    var texture = ResComp.Load<Texture>("Textures/Icon");
}
```

## 七、错误处理

### 1. 空路径检查

```csharp
// 检查路径是否为空
if (string.IsNullOrEmpty(path))
{
    Debug.LogError("Resource path is empty!");
    return null;
}
```

### 2. 加载失败处理

```csharp
// 异步加载失败处理
ResComp.LoadAsync<Texture>("Textures/Icon", (texture) =>
{
    if (texture == null)
    {
        Debug.LogError("Failed to load texture!");
        // 处理加载失败
    }
});
```

### 3. 异常捕获

```csharp
try
{
    var texture = ResComp.Load<Texture>("Textures/Icon");
    if (texture == null)
    {
        Debug.LogError("Failed to load texture!");
    }
}
catch (Exception e)
{
    Debug.LogError($"Failed to load texture: {e.Message}");
}
```

## 八、注意事项

### 1. 资源释放

- 使用完资源后及时释放
- 释放GameObject时注意不要重复释放
- 定期调用 `UnloadUnusedAssets()` 清理内存

### 2. 资源路径

- 确保资源路径正确
- 资源路径区分大小写
- 资源路径不要包含扩展名（YooAsset会自动处理）

### 3. 异步加载

- 异步加载时注意回调的执行时机
- 使用UniTask时注意await的使用
- 避免在异步加载中阻塞主线程

### 4. 缓存管理

- 缓存会占用内存
- 定期清理缓存
- 及时释放不再使用的资源

## 九、完整示例

```csharp
using GameCore.Core.Comp.Res;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ResourceExample : MonoBehaviour
{
    private void Start()
    {
        // 初始化
        ResComp.Instance.Init();

        // 同步加载
        var texture = ResComp.Load<Texture>("Textures/Icon");
        if (texture != null)
        {
            Debug.Log("Texture loaded successfully!");
        }

        // 异步加载（回调版本）
        ResComp.LoadAsync<GameObject>("Prefabs/UI/MainUI", (prefab) =>
        {
            if (prefab != null)
            {
                var uiObject = GameObject.Instantiate(prefab);
                uiObject.SetActive(true);
            }
        });

        // 异步加载（UniTask版本）
        LoadTextureUniTask();

        // 预加载资源
        ResComp.PreloadAsset("Textures/Icon", 0);

        // 释放资源
        // ResComp.Release("Textures/Icon");
    }

    private async void LoadTextureUniTask()
    {
        var texture = await ResComp.LoadAsync<Texture>("Textures/Icon");
        if (texture != null)
        {
            Debug.Log("Texture loaded successfully!");
            // 使用纹理
        }
    }

    private void OnDestroy()
    {
        // 释放资源
        ResComp.ReleaseAll();
    }
}
```

## 十、总结

ResComp提供了完整的资源管理功能，使用YooAsset作为底层资源管理框架。支持同步加载、异步加载、缓存管理、预加载等功能，可以满足游戏开发中的各种资源需求。

**核心优势**:
1. 基于YooAsset，功能强大
2. 支持同步和异步加载
3. 自动缓存管理
4. 完善的错误处理
5. 性能优化

**使用建议**:
1. 合理使用预加载
2. 及时释放资源
3. 定期清理缓存
4. 注意错误处理
