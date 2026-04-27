# UI系统使用指南

## 一、UI系统架构

UI系统采用模块化设计，基于继承和配置，易于扩展和维护。

### 核心组件

1. **UIComp**: UI组件，管理所有UI面板
2. **UIBase**: UI基础类，提供UI的基础功能
3. **UIPanelBase**: UI面板基类，提供面板的生命周期管理
4. **UIDataBase**: UI数据基类，提供UI数据的默认实现
5. **UIConfig**: UI配置类，定义UI的层级、预制体、脚本类型
6. **UIGlobalConfig**: UI全局配置，存储所有UI配置

## 二、创建新UI面板

### 1. 创建UI数据类

```csharp
using GameCore.Core.Comp.UI;

public class MyUIData : UIDataBase
{
    public string Title { get; set; }
    public string Content { get; set; }
}
```

### 2. 创建UI配置类

```csharp
using GameCore.Core.Comp.UI;

[CreateAssetMenu(fileName = "MyUIConfig", menuName = "UI/UIConfig", order = 1)]
public class MyUIConfig : UIConfig
{
    public override EUILayer Layer => EUILayer.Main;
    public override UnityEngine.Object Prefab => prefab?.Asset;
    public override System.Type ScriptType => typeof(MyUI);
}
```

### 3. 创建UI面板类

```csharp
using GameCore.Core.Comp.UI;

public class MyUI : UIPanelBase
{
    protected override void OnInit()
    {
        // 初始化UI
        Debug.Log($"MyUI initialized: {UIName}");
    }

    protected override void OnOpen(IUIData data)
    {
        // 打开UI，处理数据
        var myData = data as MyUIData;
        if (myData != null)
        {
            Debug.Log($"Title: {myData.Title}");
            Debug.Log($"Content: {myData.Content}");
        }
    }

    protected override void OnClose()
    {
        // 关闭UI，清理资源
        Debug.Log($"MyUI closed: {UIName}");
    }

    protected override UniTask OnCloseAnima()
    {
        // 播放关闭动画
        return UniTask.CompletedTask;
    }

    protected override void OnCloseEnd()
    {
        // 关闭动画结束
    }

    protected override void OnUnInit()
    {
        // 反初始化
    }

    protected override void OnDestroy()
    {
        // 销毁
    }
}
```

### 4. 注册UI配置

将创建的UI配置文件放入 `Assets/Res/UI/Common/Config` 目录，并在 `UIGlobalConfig` 中注册。

## 三、使用UI系统

### 1. 在代码中打开UI

```csharp
// 打开UI
UIComp.OpenUI<MyUI>();

// 打开UI并传递数据
var data = new MyUIData
{
    Title = "标题",
    Content = "内容"
};
UIComp.OpenUI<MyUI>(data);
```

### 2. 在代码中关闭UI

```csharp
// 关闭UI
UIComp.CloseUI<MyUI>();
```

### 3. 获取UI面板

```csharp
// 获取UI面板
var myUI = UIComp.GetUI<MyUI>();
if (myUI != null)
{
    // 使用UI面板
}
```

### 4. 检查UI状态

```csharp
// 检查UI是否已打开
if (UIComp.IsUIOpened<MyUI>())
{
    // UI已打开
}
```

## 四、UI生命周期

### 打开流程

```
OpenUI<T>(data)
  ↓
从配置加载UI预制体
  ↓
实例化UI对象
  ↓
添加UI面板组件
  ↓
Init() - 初始化UI面板
  ↓
Open(data) - 打开UI面板
  ↓
OnOpen(data) - 处理数据
  ↓
OnCloseAnima() - 播放关闭动画
  ↓
OnCloseEnd() - 关闭动画结束
  ↓
销毁UI对象
```

### 关闭流程

```
CloseUI<T>()
  ↓
获取UI面板
  ↓
Close() - 关闭UI面板
  ↓
OnClose() - 清理资源
  ↓
OnCloseAnima() - 播放关闭动画
  ↓
OnCloseEnd() - 关闭动画结束
  ↓
销毁UI对象
```

## 五、UI层级

UI系统支持多个UI层级：

- **Scene**: 场景UI
- **SceneTips**: 场景提示
- **Main**: 主界面
- **Dialog**: 对话框
- **Loading**: 加载界面
- **Tips**: 提示信息

UI系统会自动处理UI的层级关系，确保UI按照正确的顺序渲染。

## 六、UI数据传递

UI数据通过 `IUIData` 接口传递，所有UI数据类需要继承自 `UIDataBase`。

### 数据验证

UIDataBase 提供数据验证功能：

```csharp
public class MyUIData : UIDataBase
{
    public string Title { get; set; }

    public override void Validate()
    {
        base.Validate();
        if (string.IsNullOrEmpty(Title))
        {
            Debug.LogWarning("Title is null or empty!");
            Invalidate();
        }
    }
}
```

## 七、UI动画

UI面板支持打开和关闭动画：

```csharp
protected override UniTask OnOpenAnima()
{
    // 播放打开动画
    return UniTask.CompletedTask;
}

protected override UniTask OnCloseAnima()
{
    // 播放关闭动画
    return UniTask.CompletedTask;
}
```

## 八、UI事件

UI面板支持自定义事件：

```csharp
public class MyUI : UIPanelBase
{
    private event Action OnButtonClick;

    protected override void OnOpen(IUIData data)
    {
        // 绑定事件
        OnButtonClick += HandleButtonClick;
    }

    protected override void OnClose()
    {
        // 解绑事件
        OnButtonClick -= HandleButtonClick;
    }

    private void HandleButtonClick()
    {
        Debug.Log("Button clicked!");
    }
}
```

## 九、UI最佳实践

### 1. 使用配置文件

所有UI都应该使用配置文件，避免硬编码。

### 2. 合理使用生命周期

- `OnInit()`: 初始化UI，只执行一次
- `OnOpen(data)`: 打开UI，处理数据
- `OnClose()`: 关闭UI，清理资源
- `OnUnInit()`: 反初始化，只执行一次
- `OnDestroy()`: 销毁，只执行一次

### 3. 避免内存泄漏

- 及时解绑事件
- 及时清理资源
- 及时关闭UI

### 4. 使用异步任务

使用 `UniTask` 处理异步操作，避免阻塞主线程。

## 十、示例代码

### 完整的UI面板示例

```csharp
using GameCore.Core.Comp.UI;

public class MainUI : UIPanelBase
{
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _contentText;

    protected override void OnInit()
    {
        // 获取UI元素
        _titleText = transform.Find("Title").GetComponent<TextMeshProUGUI>();
        _contentText = transform.Find("Content").GetComponent<TextMeshProUGUI>();

        // 初始化UI
        Debug.Log($"MainUI initialized: {UIName}");
    }

    protected override void OnOpen(IUIData data)
    {
        // 处理数据
        var mainData = data as MainUIData;
        if (mainData != null)
        {
            _titleText.text = mainData.PlayerName;
            _contentText.text = $"Level: {mainData.PlayerLevel}";
        }
    }

    protected override void OnClose()
    {
        // 清理资源
        _titleText = null;
        _contentText = null;
        Debug.Log($"MainUI closed: {UIName}");
    }

    protected override UniTask OnCloseAnima()
    {
        // 播放关闭动画
        return UniTask.CompletedTask;
    }

    protected override void OnCloseEnd()
    {
        // 关闭动画结束
    }

    protected override void OnUnInit()
    {
        // 反初始化
    }

    protected override void OnDestroy()
    {
        // 销毁
    }
}
```

## 十一、常见问题

### 1. UI无法打开

- 检查UI配置是否正确
- 检查UI预制体是否存在
- 检查UI脚本类型是否正确

### 2. UI无法关闭

- 检查UI是否已打开
- 检查UI是否正在关闭
- 检查UI是否已销毁

### 3. UI数据传递失败

- 检查数据类型是否匹配
- 检查数据是否有效
- 检查数据是否已验证

## 十二、总结

UI系统采用模块化设计，易于扩展和维护。通过配置文件管理UI，通过继承扩展新UI，通过生命周期管理UI状态。合理使用UI系统，可以提高开发效率，减少bug。
