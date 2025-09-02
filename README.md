# AudioEffectKit

Unity音效管理模块，提供完整的音效播放、管理和扩展系统，支持3D音效、分类管理、动态效果和高性能对象池。

## 📋 目录

- [特性](#特性)
- [安装](#安装)
- [快速开始](#快速开始)
- [核心组件](#核心组件)
- [使用指南](#使用指南)
- [API参考](#api参考)
- [扩展开发](#扩展开发)
- [最佳实践](#最佳实践)

## ✨ 特性

### 🎵 音效管理
- **单例AudioManager**: 统一的音效管理入口
- **分类管理**: 支持SFX、Music、Voice、UI、Ambient、Combat等分类
- **音量控制**: 主音量、分类音量独立控制
- **并发限制**: 可配置的最大同时播放音效数量
- **音效句柄**: AudioHandle提供完整的播放控制

### 🌍 3D音效系统
- **空间音效**: 支持3D位置音效播放
- **AudioEmitter组件**: 便捷的3D音效发射器
- **AudioListener3D**: 增强的3D音频监听器
- **衰减模式**: 可配置的音效衰减模式

### ⚡ 高性能优化
- **对象池系统**: AudioSourcePool管理AudioSource复用
- **预热机制**: 支持对象池预热减少运行时分配
- **内存管理**: 自动清理无效音效句柄
- **性能监控**: 实时监控活跃音效数量

### 🔧 配置系统
- **AudioConfig**: ScriptableObject配置资产
- **可视化编辑器**: 自定义Inspector编辑器
- **运行时验证**: 配置有效性检查
- **默认配置**: 一键创建默认配置

### 🎭 音效扩展
- **IAudioEffect接口**: 标准化的音效处理接口
- **优先级系统**: 效果处理优先级控制
- **分类过滤**: 按音效分类应用不同效果
- **实时处理**: 播放时和更新时的效果处理

### 🎼 高级功能
- **淡入淡出**: 音量渐变效果支持
- **音调随机化**: 音调变化增加音效丰富度
- **循环播放**: 音乐和环境音效循环支持
- **播放进度控制**: 精确的播放进度控制

## 📦 安装

### 通过Package Manager安装

1. 打开Unity Editor
2. 进入 `Window > Package Manager`
3. 点击 `+` 按钮选择 `Add package from git URL`
4. 输入包的Git URL或选择本地路径
5. 点击 `Add` 完成安装

### 依赖项

```json
{
  "com.mygamedev.core": "1.0.0",
  "com.cysharp.unitask": "2.5.10"
}
```

## 🚀 快速开始

### 1. 创建音效配置

```csharp
// 在Assets右键菜单中选择 Audio > Audio Config
// 或通过代码创建
var config = AudioConfig.CreateDefault();
```

### 2. 初始化音效管理器

```csharp
// 在游戏启动时初始化
AudioManager.Instance.Initialize();

// 加载音效配置
AudioManager.Instance.LoadAudioDatabase(audioConfig);
```

### 3. 播放音效

```csharp
// 播放2D音效
var handle = AudioManager.Instance.PlaySound("button_click");

// 播放3D音效
var handle = AudioManager.Instance.PlaySound("explosion", transform.position);

// 播放一次性音效
AudioManager.Instance.PlayOneShot("coin_collect", transform.position);
```

### 4. 使用AudioEmitter组件

```csharp
public class WeaponController : MonoBehaviour
{
    private AudioEmitter audioEmitter;
    
    void Start()
    {
        audioEmitter = GetComponent<AudioEmitter>();
        audioEmitter.AddSound("weapon_fire");
        audioEmitter.AddSound("weapon_reload");
    }
    
    void Fire()
    {
        audioEmitter.PlaySound("weapon_fire");
    }
}
```

## 🧱 核心组件

### AudioManager
统一的音效管理器，负责：
- 音效播放和控制
- 分类音量管理  
- 对象池管理
- 音效扩展管理

### AudioHandle
音效播放句柄，提供：
- 播放状态查询
- 音量、音调控制
- 播放进度控制
- 淡入淡出效果

### AudioConfig
音效配置资产，包含：
- 音效剪辑数据库
- 分类默认音量
- 播放参数配置
- 性能优化设置

### AudioEmitter
3D音效发射组件，支持：
- 多音效管理
- 随机播放
- 音调随机化
- 位置音效

### AudioClipData
音效剪辑数据类，定义：
- 音效基本属性
- 播放参数
- 分类信息
- 3D音效设置

## 📖 使用指南

### 音效分类管理

```csharp
// 设置分类音量
AudioManager.Instance.SetCategoryVolume(AudioCategory.SFX, 0.8f);
AudioManager.Instance.SetCategoryVolume(AudioCategory.Music, 0.6f);

// 暂停/恢复分类
AudioManager.Instance.PauseCategory(AudioCategory.Music);
AudioManager.Instance.ResumeCategory(AudioCategory.Music);

// 停止分类中所有音效
AudioManager.Instance.StopCategory(AudioCategory.SFX);
```

### 音效句柄控制

```csharp
var handle = AudioManager.Instance.PlaySound("background_music");

// 音量控制
handle.Volume = 0.5f;

// 音调控制
handle.Pitch = 1.2f;

// 位置控制
handle.Position = player.transform.position;

// 播放进度
handle.Progress = 0.5f; // 跳转到50%位置

// 淡出效果
handle.FadeOut(2.0f);

// 完成回调
handle.SetOnCompleteCallback(() => Debug.Log("音效播放完成"));
```

### AudioEmitter高级用法

```csharp
public class FootstepEmitter : MonoBehaviour
{
    [SerializeField] private AudioEmitter footstepEmitter;
    [SerializeField] private string[] footstepSounds;
    
    void Start()
    {
        footstepEmitter.SetSounds(footstepSounds.ToList());
    }
    
    void OnFootstep()
    {
        // 随机播放脚步声
        footstepEmitter.PlayRandomSound();
    }
    
    void OnSurfaceChange(SurfaceType surface)
    {
        // 根据地面类型切换音效
        footstepEmitter.ClearSounds();
        footstepEmitter.AddSound($"footstep_{surface.ToString().ToLower()}");
    }
}
```

## 🔌 扩展开发

### 创建自定义音效效果

```csharp
public class ReverbEffect : IAudioEffect
{
    private float reverbStrength = 0.5f;
    
    public void OnAudioPlay(AudioSource source, AudioClipData clipData)
    {
        // 音效开始播放时的处理
        if (clipData.Category == AudioCategory.Ambient)
        {
            source.reverbZoneMix = reverbStrength;
        }
    }
    
    public void OnAudioStop(AudioSource source)
    {
        // 音效停止时的清理
        source.reverbZoneMix = 0f;
    }
    
    public void OnAudioUpdate(AudioSource source, float deltaTime)
    {
        // 每帧更新处理
        // 可以实现动态效果变化
    }
    
    public bool CanProcess(AudioCategory category)
    {
        return category == AudioCategory.Ambient || category == AudioCategory.SFX;
    }
    
    public int GetPriority()
    {
        return 100; // 优先级，数值越小越先处理
    }
    
    public string GetEffectName()
    {
        return "Reverb Effect";
    }
}

// 注册效果
AudioManager.Instance.RegisterAudioEffect<ReverbEffect>();
```

### 自定义AudioClipData

```csharp
[System.Serializable]
public class CustomAudioClipData : AudioClipData
{
    [SerializeField] private float customParameter;
    [SerializeField] private AnimationCurve volumeCurve;
    
    public override void ApplyToAudioSource(AudioSource source)
    {
        base.ApplyToAudioSource(source);
        
        // 应用自定义参数
        source.dopplerLevel = customParameter;
    }
    
    protected override bool ValidateInternal()
    {
        return base.ValidateInternal() && customParameter >= 0f;
    }
}
```

## 💡 最佳实践

### 性能优化
1. **合理设置并发数量限制**，避免过多音效同时播放
2. **使用对象池预热**，减少运行时分配开销
3. **及时停止不需要的音效**，释放AudioSource资源
4. **合理使用3D音效**，避免不必要的空间计算

### 资源管理
1. **统一使用AudioConfig管理音效数据库**
2. **按分类组织音效**，便于批量控制
3. **使用合适的音频压缩格式**，平衡质量和大小
4. **预加载常用音效**，减少运行时加载

### 开发规范
1. **为音效句柄设置合适的完成回调**
2. **在适当时机调用Initialize初始化管理器**
3. **使用AudioEmitter组件**管理GameObject相关音效
4. **实现IAudioEffect接口**添加自定义效果

### 调试技巧
1. 使用`AudioManager.Instance.ActiveSoundCount`监控活跃音效数量
2. 通过`AudioHandle.PlayTime`和`AudioHandle.Progress`跟踪播放状态
3. 在开发阶段启用音效数据验证
4. 使用Unity Profiler分析音频性能

## 📚 API参考

### AudioManager主要方法

| 方法 | 描述 |
|------|------|
| `Initialize()` | 初始化音效管理器 |
| `PlaySound(string, Vector3)` | 播放音效，返回AudioHandle |
| `PlayMusic(string, bool)` | 播放音乐，支持循环 |
| `PlayOneShot(string, Vector3)` | 播放一次性音效 |
| `StopAllSounds()` | 停止所有音效 |
| `SetCategoryVolume(AudioCategory, float)` | 设置分类音量 |
| `RegisterAudioEffect<T>()` | 注册音效效果 |

### AudioHandle主要属性

| 属性 | 类型 | 描述 |
|------|------|------|
| `IsValid` | bool | 句柄是否有效 |
| `IsPlaying` | bool | 是否正在播放 |
| `Volume` | float | 音量(0-1) |
| `Pitch` | float | 音调(0.1-3) |
| `Position` | Vector3 | 3D位置 |
| `Progress` | float | 播放进度(0-1) |

---

**作者**: EricZhao  
**邮箱**: kaka12300wu@gmail.com  
**版本**: 1.0.0  
**Unity版本要求**: 2022.3.22f1+