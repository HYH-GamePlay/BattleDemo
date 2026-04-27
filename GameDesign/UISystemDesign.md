# UI系统设计文档

## 一、UI系统架构

### 1. 核心接口和类

#### IUIComp（UI组件接口）
- 定义UI组件的基本功能
- OpenUI<T>(): 打开UI面板
- CloseUI<T>(): 关闭UI面板

#### UIComp（UI组件实现）
- 管理所有UI面板
- 维护UI面板字典
- 处理UI打开和关闭逻辑

#### IUIData（UI数据接口）
- 定义UI数据的基本接口
- 所有UI数据类需要实现此接口

#### UIDataBase（UI数据基类）
- 提供UI数据的默认实现
- 作为UI数据类的基类

#### UIBase（UI基础类）
- UI的基础类，继承自UIBehaviour
- 提供UI的基础功能

#### UIPanelBase（UI面板基类）
- UI面板的基类，继承自UIBase
- 提供面板的生命周期管理
- 提供面板的打开、关闭动画流程

#### UIConfig（UI配置类）
- UI配置的ScriptableObject
- 定义UI的层级、预制体、脚本类型

#### UIGlobalConfig（UI全局配置）
- UI全局配置的单例
- 存储所有UI配置

#### EUILayer（UI层级枚举）
- UI的渲染层级
- Scene, SceneTips, Main, Dialog, Loading, Tips

### 2. UI层级结构

```
UIRoot
├── Scene (场景UI)
├── SceneTips (场景提示)
├── Main (主界面)
├── Dialog (对话框)
├── Loading (加载界面)
└── Tips (提示信息)
```

### 3. UI生命周期

1. **Init()**: 初始化UI组件
2. **OpenUI<T>()**: 打开UI面板
   - 从配置加载UI预制体
   - 实例化UI面板
   - 调用Init()
   - 调用Open(data)
3. **Open(data)**: 打开面板
   - 初始化面板数据
   - 播放打开动画
4. **CloseUI<T>()**: 关闭UI面板
   - 播放关闭动画
   - 销毁UI面板
5. **Close()**: 关闭面板
   - 播放关闭动画
   - 销毁面板
6. **UnInit()**: 反初始化UI组件

### 4. UI面板生命周期

1. **Init()**: 初始化面板
2. **Open(data)**: 打开面板
3. **Close()**: 关闭面板
4. **UnInit()**: 反初始化面板

### 5. UI动画流程

#### 打开动画流程
```
OnOpen(data)
  ↓
OnOpen(data) - 面板数据初始化
  ↓
OnOpenAnima() - 播放打开动画
  ↓
OnOpenEnd() - 打开动画结束
```

#### 关闭动画流程
```
OnClose()
  ↓
OnClose() - 面板清理
  ↓
OnCloseAnima() - 播放关闭动画
  ↓
OnCloseEnd() - 关闭动画结束
```

## 二、UI系统功能

### 1. UI管理功能

- [x] UI面板注册
- [x] UI面板打开/关闭
- [x] UI层级管理
- [x] UI数据传递

### 2. UI面板功能

- [x] 面板生命周期管理
- [x] 面板数据管理
- [x] 面板动画管理
- [x] 面板交互管理

### 3. UI数据功能

- [x] UI数据基类
- [x] UI数据传递
- [x] UI数据管理

## 三、UI面板分类

### 1. 主界面（MainUI）
- 角色信息
- 武器信息
- 技能信息
- 状态栏

### 2. 战斗界面（BattleUI）
- 血条显示
- 魔法条显示
- 技能冷却显示
- 准星显示

### 3. 菜单界面（MenuUI）
- 开始菜单
- 暂停菜单
- 设置菜单
- 退出菜单

### 4. 对话界面（DialogueUI）
- 对话显示
- 对话选项
- 对话触发

### 5. 任务界面（QuestUI）
- 任务列表
- 任务进度
- 任务奖励

### 6. 物品界面（InventoryUI）
- 物品栏
- 装备栏
- 物品使用
- 物品出售

### 7. 提示界面（ToastUI）
- 提示信息
- 提示动画

## 四、UI数据类

### 1. UIDataBase
- UI数据的基类
- 提供默认实现

### 2. 具体UI数据类
- 需要实现IUIData接口
- 定义UI需要的数据

## 五、UI配置

### 1. UIConfig
- UI配置的ScriptableObject基类
- 定义UI的层级、预制体、脚本类型
- **注意**: UIConfig是基类，具体的UI配置类应该继承UIConfig并实现虚方法

### 2. UIGlobalConfig
- UI全局配置的单例
- 存储所有UI配置

## 六、UI实现建议

### 1. UIPanelBase扩展
- 添加面板状态管理
- 添加面板事件系统
- 添加面板缓存系统

### 2. UIComp扩展
- 添加UI面板缓存
- 添加UI层级管理
- 添加UI优先级管理

### 3. UIBase扩展
- 添加UI事件系统
- 添加UI数据绑定
- 添加UI动画系统

### 4. UIDataBase扩展
- 添加UI数据验证
- 添加UI数据转换
- 添加UI数据保存

## 七、UI系统优势

1. **模块化**: UI系统模块化设计，易于扩展
2. **可配置**: UI配置化，易于调整
3. **可扩展**: 基于继承，易于扩展新面板
4. **可维护**: 代码结构清晰，易于维护
5. **高性能**: 使用对象池，减少GC

## 八、UI系统使用示例

### 1. 创建UI配置

```csharp
[CreateAssetMenu(fileName = "MainUIConfig", menuName = "UI/UIConfig", order = 1)]
public class MainUIConfig : UIConfig
{
    public override EUILayer Layer => EUILayer.Main;
    public override UnityEngine.Object Prefab => prefab?.Asset;
    public override System.Type ScriptType => typeof(MainUI);
}
```

### 2. 创建UI面板

```csharp
public class MainUI : UIPanelBase
{
    protected override void OnInit()
    {
        // 初始化UI
    }

    protected override void OnOpen(IUIData data)
    {
        // 打开UI，处理数据
    }

    protected override void OnClose()
    {
        // 关闭UI，清理资源
    }

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
}
```

### 3. 打开UI

```csharp
// 打开UI
UIComp.OpenUI<MainUI>();

// 打开UI并传递数据
var data = new MainUIData();
data.playerName = "艾瑞克";
UIComp.OpenUI<MainUI>(data);
```

### 4. 关闭UI

```csharp
// 关闭UI
UIComp.CloseUI<MainUI>();
```

## 九、UI系统开发计划

### Phase 1: 基础框架
- [x] 核心接口和类
- [x] UI层级管理
- [x] UI配置系统
- [x] UI面板基类

### Phase 2: UI面板实现
- [ ] MainUI（主界面）
- [ ] BattleUI（战斗界面）
- [ ] MenuUI（菜单界面）
- [ ] DialogueUI（对话界面）
- [ ] QuestUI（任务界面）
- [ ] InventoryUI（物品界面）
- [ ] ToastUI（提示界面）

### Phase 3: UI数据类
- [ ] UIDataBase
- [ ] MainUIData
- [ ] BattleUIData
- [ ] DialogueUIData
- [ ] QuestUIData
- [ ] InventoryUIData
- [ ] ToastUIData

### Phase 4: UI功能完善
- [ ] UI动画系统
- [ ] UI事件系统
- [ ] UI数据绑定
- [ ] UI缓存系统

### Phase 5: UI优化
- [ ] UI性能优化
- [ ] UI内存优化
- [ ] UI加载优化
- [ ] UI卸载优化

## 十、总结

UI系统采用模块化设计，基于继承和配置，易于扩展和维护。UI面板继承自UIPanelBase，实现生命周期管理、动画流程和交互功能。UI配置化，便于调整和扩展。UI系统支持UI层级管理、数据传递、动画播放等功能，为游戏提供完整的UI解决方案。
