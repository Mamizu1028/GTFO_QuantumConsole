using BepInEx.Logging;
using Globals;
using Hikaria.ES;
using Hikaria.QC.Localization;
using Hikaria.QC.UI;
using Hikaria.QC.Utilities;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TheArchive.Core.Attributes.Feature;
using TheArchive.Core.Attributes.Feature.Members;
using TheArchive.Core.Attributes.Feature.Patches;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.FeaturesAPI.Settings;
using TheArchive.Interfaces;
using TheArchive.Loader;
using TheArchive.Utilities;
using UnityEngine;

namespace Hikaria.QC;

[EnableFeatureByDefault]
[DisallowInGameToggle]
[DoNotSaveToConfig]
internal class QuantumConsoleSettings : Feature
{
    public override string Name => "量子终端设置";

    public override Type[] ExternalLocalizedTypes => new[]
    {
        typeof(BepInEx.Logging.LogLevel), typeof(LogLevel), typeof(AutoScrollOptions), typeof(SortOrder)
    };

    public override bool InlineSettingsIntoParentMenu => true;

    public new static IArchiveLogger FeatureLogger { get; set; }

    [FeatureConfig]
    public static QuantumSettings Settings { get; set; }

    public class QuantumSettings
    {
        [FSIdentifier("BIELogLevel")]
        [FSDisplayName("BepInEx 日志监听级别")]
        public List<BepInEx.Logging.LogLevel> BIEListenLevel { get; set; } = new();
        [FSDisplayName("按键绑定")]
        public QuantumKeySettings KeySettings { get; set; } = new();
        [FSDisplayName("主题设置")]
        public QuantumThemeSettings ThemeSettings { get; set; } = new();
        [FSDisplayName("控制台设置")]
        public QuantumConsolePreferenceSettings PreferenceSettings { get; set; } = new();
    }

    public override void OnFeatureSettingChanged(FeatureSetting setting)
    {
        if (setting.Identifier == "BIELogLevel")
        {
            BIELogListener.BIELogLevel = (setting.GetValue() as List<BepInEx.Logging.LogLevel>).ToFlags();
        }
    }

    public class QuantumThemeSettings
    {
        [FSDisplayName("时间戳格式")]
        public string TimestampFormat { get; set; } = "[{0:00}:{1:00}:{2:00}]";
        [FSDisplayName("指令日志格式")]
        public string CommandLogFormat { get; set; } = "> {0}";

        public static implicit operator QuantumTheme(QuantumThemeSettings settings)
        {
            var theme = QuantumTheme.DefaultTheme();
            theme.TimestampFormat = settings.TimestampFormat;
            theme.CommandLogFormat = settings.CommandLogFormat;
            return theme;
        }
    }

    public class QuantumConsolePreferenceSettings
    {
        [FSHeader("日志设置")]
        [FSDisplayName("详细错误日志")]
        public bool VerboseErrors { get; set; } = false;
        [FSDisplayName("详细记录日志级别")]
        public List<LogLevel> VerboseLogging { get; set; } = new();
        [FSDisplayName("Unity日志监听级别")]
        public List<LogLevel> LoggingLevel { get; set; } = new();

        [FSDisplayName("自动激活日志级别")]
        public List<LogLevel> OpenOnLogLevel { get; set; } = new();
        [FSDisplayName("监听Unity日志")]
        public bool InterceptDebugLogger { get; set; } = true;
        [FSDisplayName("非激活时监听")]
        public bool InterceptWhilstInactive { get; set; } = true;
        [FSDisplayName("添加时间戳前缀")]
        public bool PrependTimestamps { get; set; } = true;

        [FSDisplayName("日志最大容量")]
        public int MaxStoredLogs { get; set; } = 1024;
        [FSDisplayName("日志文本大小")]
        public int LogFontSize { get; set; } = 14;
        [FSDisplayName("显示初始化日志")]
        public bool ShowInitLogs { get; set; } = true;

        [FSHeader("控制台设置")]
        [FSDisplayName("启动时激活")]
        public bool ActivateOnStartup { get; set; } = false;
        [FSDisplayName("激活时自动追焦")]
        public bool FocusOnActivate { get; set; } = true;
        [FSDisplayName("提交后关闭")]
        public bool CloseOnSubmit { get; set; } = false;
        [FSDisplayName("自动滚动模式")]
        public AutoScrollOptions AutoScroll { get; set; } = AutoScrollOptions.OnInvoke;
        [FSIgnore]
        [FSDisplayName("缓动模式")]
        public EnhancedScroller.TweenType TweenType { get; set; } = EnhancedScroller.TweenType.easeOutSine;
        [FSDisplayName("缓动时长")]
        [FSSlider(0.1f, 1f, FSSlider.SliderStyle.FloatTwoDecimal)]
        public float TweenTime { get; set; } = 0.5f;
        [FSDisplayName("缓动无缝衔接")]
        public bool SeamlessTween { get; set; } = true;

        [FSHeader("指令设置")]
        [FSDisplayName("自动补全")]
        public bool EnableAutocomplete { get; set; } = true;
        [FSDisplayName("指令提示")]
        public bool ShowPopupDisplay { get; set; } = true;
        [FSDisplayName("指令提示排序")]
        public SortOrder SuggestionDisplayOrder { get; set; } = SortOrder.Descending;
        [FSDisplayName("指令提示文本大小")]
        public int SuggestionFontSize { get; set; } = 16;
        [FSDisplayName("指令提示最大容量")]
        public int MaxSuggestionDisplaySize { get; set; } = 20;
        [FSDisplayName("模糊搜索")]
        public bool UseFuzzySearch { get; set; } = true;
        [FSDisplayName("搜索区分大小写")]
        public bool CaseSensitiveSearch { get; set; } = false;
        [FSDisplayName("折叠重载指令建议")]
        public bool CollapseSuggestionOverloads { get; set; } = true;

        [FSHeader("任务设置")]
        [FSDisplayName("显示当前任务")]
        public bool ShowCurrentJobs { get; set; } = true;
        [FSDisplayName("异步时阻塞")]
        public bool BlockOnAsync { get; set; } = false;

        [FSHeader("指令历史设置")]
        [FSDisplayName("保存指令历史")]
        public bool StoreCommandHistory { get; set; } = true;
        [FSDisplayName("保存重复指令")]
        public bool StoreDuplicateCommands { get; set; } = true;
        [FSDisplayName("保存相邻重复指令")]
        public bool StoreAdjacentDuplicateCommands { get; set; } = false;
        [FSDisplayName("指令历史大小")]
        public int CommandHistorySize { get; set; } = 30;

        public static implicit operator QuantumConsolePreferences(QuantumConsolePreferenceSettings settings)
        {
            var pref = new QuantumConsolePreferences();

            pref.VerboseErrors = settings.VerboseErrors;
            pref.VerboseLogging = settings.VerboseLogging.ToFlags();
            pref.LoggingLevel = settings.LoggingLevel.ToFlags();

            pref.OpenOnLogLevel = settings.OpenOnLogLevel.ToFlags();
            pref.InterceptDebugLogger = settings.InterceptDebugLogger;
            pref.InterceptWhilstInactive = settings.InterceptWhilstInactive;
            pref.PrependTimestamps = settings.PrependTimestamps;

            pref.ActivateOnStartup = settings.ActivateOnStartup;
            pref.InitialiseOnStartup = settings.InterceptWhilstInactive;
            pref.FocusOnActivate = settings.FocusOnActivate;
            pref.CloseOnSubmit = settings.CloseOnSubmit;
            pref.AutoScroll = settings.AutoScroll;
            pref.TweenType = settings.TweenType;
            pref.TweenTime = settings.TweenTime;
            pref.SeamlessTween = settings.SeamlessTween;

            pref.EnableAutocomplete = settings.EnableAutocomplete;
            pref.ShowPopupDisplay = settings.ShowPopupDisplay;
            pref.SuggestionDisplayOrder = settings.SuggestionDisplayOrder;
            pref.MaxSuggestionDisplaySize = settings.MaxSuggestionDisplaySize;
            pref.UseFuzzySearch = settings.UseFuzzySearch;
            pref.CaseSensitiveSearch = settings.CaseSensitiveSearch;
            pref.CollapseSuggestionOverloads = settings.CollapseSuggestionOverloads;

            pref.ShowCurrentJobs = settings.ShowCurrentJobs;
            pref.BlockOnAsync = settings.BlockOnAsync;

            pref.StoreCommandHistory = settings.StoreCommandHistory;
            pref.StoreDuplicateCommands = settings.StoreDuplicateCommands;
            pref.StoreAdjacentDuplicateCommands = settings.StoreAdjacentDuplicateCommands;
            pref.CommandHistorySize = settings.CommandHistorySize;

            pref.MaxStoredLogs = settings.MaxStoredLogs;
            pref.ShowInitLogs = settings.ShowInitLogs;

            pref.LogFontSize = settings.LogFontSize;
            pref.SuggestionFontSize = settings.SuggestionFontSize;
            return pref;
        }
    }

    public class QuantumKeySettings
    {
        [FSHeader("按键绑定")]
        [FSDisplayName("提交命令")]
        public KeyCode SubmitCommandKey { get; set; } = KeyCode.Return;
        [FSDisplayName("显示控制台")]
        public KeyCombo ShowConsoleKey { get; set; } = KeyCode.None;
        [FSDisplayName("隐藏控制台")]
        public KeyCombo HideConsoleKey { get; set; } = KeyCode.None;
        [FSDisplayName("切换控制台状态")]
        public KeyCombo ToggleConsoleVisibilityKey { get; set; } = KeyCode.BackQuote;
        [FSDisplayName("放大")]
        public KeyCombo ZoomInKey { get; set; } = new KeyCombo { Key = KeyCode.Equals, Ctrl = true };
        [FSDisplayName("缩小")]
        public KeyCombo ZoomOutKey { get; set; } = new KeyCombo { Key = KeyCode.Minus, Ctrl = true };
        [FSDisplayName("拖动")]
        public KeyCombo DragConsoleKey { get; set; } = new KeyCombo { Key = KeyCode.Mouse0, Shift = true };
        [FSDisplayName("选择下一条建议")]
        public KeyCombo SelectNextSuggestionKey { get; set; } = KeyCode.Tab;
        [FSDisplayName("选择上一条建议")]
        public KeyCombo SelectPreviousSuggestionKey { get; set; } = new KeyCombo { Key = KeyCode.Tab, Shift = true };
        [FSDisplayName("下一条命令")]
        public KeyCode NextCommandKey { get; set; } = KeyCode.UpArrow;
        [FSDisplayName("上一条命令")]
        public KeyCode PreviousCommandKey { get; set; } = KeyCode.DownArrow;
        [FSDisplayName("取消执行")]
        public KeyCombo CancelActionsKey { get; set; } = new KeyCombo { Key = KeyCode.C, Ctrl = true };

        public static implicit operator QuantumKeyConfig(QuantumKeySettings settings)
        {
            var config = new QuantumKeyConfig();
            config.SubmitCommandKey = settings.SubmitCommandKey;
            config.ShowConsoleKey = settings.ShowConsoleKey;
            config.HideConsoleKey = settings.HideConsoleKey;
            config.ToggleConsoleVisibilityKey = settings.ToggleConsoleVisibilityKey;
            config.ZoomInKey = settings.ZoomInKey;
            config.ZoomOutKey = settings.ZoomOutKey;
            config.DragConsoleKey = settings.DragConsoleKey;
            config.SelectNextSuggestionKey = settings.SelectNextSuggestionKey;
            config.SelectPreviousSuggestionKey = settings.SelectPreviousSuggestionKey;
            config.NextCommandKey = settings.NextCommandKey;
            config.PreviousCommandKey = settings.PreviousCommandKey;
            config.CancelActionsKey = settings.CancelActionsKey;
            return config;
        }
    }

    public class KeyCombo
    {
        [FSDisplayName("按键")]
        public KeyCode Key { get; set; } = KeyCode.None;
        [FSDisplayName("Ctrl")]
        public bool Ctrl { get; set; } = false;
        [FSDisplayName("Alt")]
        public bool Alt { get; set; } = false;
        [FSDisplayName("Shift")]
        public bool Shift { get; set; } = false;

        public static implicit operator KeyCombo(KeyCode key)
        {
            return new KeyCombo { Key = key };
        }

        public static implicit operator ModifierKeyCombo(KeyCombo combo)
        {
            return new ModifierKeyCombo() { Key = combo.Key, Alt = combo.Alt, Ctrl = combo.Ctrl, Shift = combo.Shift };
        }
    }

    public override void Init()
    {
        BIELogListener.BIELogLevel = Settings.BIEListenLevel.ToFlags();
        BIELogListener.Init();
        CommandLocalizationManager.Init();
    }

    public override void OnEnable()
    {
        BIELogListener.Instance.OnEnable();
    }

    public override void OnDisable()
    {
        BIELogListener.Instance.OnDisable();
    }

    private static Dictionary<string, UnityEngine.Object> s_AssetLookup = new();

    public static UnityEngine.Object GetLoadedAsset(string path)
    {
        return s_AssetLookup[path.ToUpper()];
    }


    private class BIELogListener : ILogListener
    {
        public static BIELogListener Instance { get; private set; }

        public static void Init()
        {
            if (Instance != null)
                return;

            Instance = new();
        }

        public void OnEnable()
        {
            if (!BepInEx.Logging.Logger.Listeners.Contains(this))
            {
                BepInEx.Logging.Logger.Listeners.Add(this);
            }
        }

        public void OnDisable()
        {
            if (BepInEx.Logging.Logger.Listeners.Contains(this))
            {
                BepInEx.Logging.Logger.Listeners.Remove(this);
            }
        }

        public void Dispose()
        {
            if (BepInEx.Logging.Logger.Listeners.Contains(this))
            {
                BepInEx.Logging.Logger.Listeners.Remove(this);
            }
        }

        public void LogEvent(object sender, LogEventArgs eventArgs)
        {
            if (eventArgs.Source.SourceName == "Unity" // 不使用 BepInEx 重定向的 Unity 日志，因为其不包含 StackTrace
                || eventArgs.Source.SourceName == "Hikaria.QC") // 排除自身日志避免出现死循环
                return;

            if (QuantumConsole.Instance == null)
                return;

            QuantumConsole.Instance.LogToConsole($"[{eventArgs.Source.SourceName}] {eventArgs.Data}", FromBIELogLevel(eventArgs.Level), true);
        }

        private LogLevel FromBIELogLevel(BepInEx.Logging.LogLevel level)
        {
            return level.GetHighestLevel() switch
            {
                BepInEx.Logging.LogLevel.Info => LogLevel.Info,
                BepInEx.Logging.LogLevel.Debug => LogLevel.Debug,
                BepInEx.Logging.LogLevel.Message => LogLevel.Message,
                BepInEx.Logging.LogLevel.Error => LogLevel.Error,
                BepInEx.Logging.LogLevel.Fatal => LogLevel.Fatal,
                BepInEx.Logging.LogLevel.Warning => LogLevel.Warning,
                _ => LogLevel.Debug,
            };
        }

        public BepInEx.Logging.LogLevel LogLevelFilter => BIELogLevel;
        public static BepInEx.Logging.LogLevel BIELogLevel;
    }

    [ArchivePatch(typeof(GlobalSetup), nameof(GlobalSetup.Awake))]
    private class GlobalSetup__Awake__Patch
    {
        static bool _initialized = false;

        private static void Prefix()
        {
            if (!_initialized)
            {
                LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<QuantumConsole>();
                LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<DraggableUI>();
                LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<BlurShaderController>();
                LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<DynamicCanvasScaler>();
                LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<ResizableUI>();
                LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<ZoomUIController>();
                LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<SuggestionDisplay>();
                LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<LogCellView>();

                LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<TypeFormatter>();
                LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<TypeColorFormatter>();
                LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<CollectionFormatter>();

                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets/quantumconsole");
                AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
                string[] array = assetBundle.AllAssetNames();
                foreach (string text in array)
                {
                    UnityEngine.Object obj = assetBundle.LoadAsset(text);
                    if (obj != null)
                    {
                        s_AssetLookup.Add(text.ToUpper(), obj);
                    }
                }
                var console = UnityEngine.Object.Instantiate(GetLoadedAsset("Assets/Plugins/QFSW/Quantum Console/Source/Prefabs/Quantum Console.prefab").Cast<GameObject>()).AddComponent<QuantumConsole>();
                _initialized = true;
            }
        }
    }

    [ArchivePatch(typeof(PlayerChatManager), nameof(PlayerChatManager.UpdateTextChatInput))]
    private class PlayerChatManager__UpdateTextChatInput__Patch
    {
        private static bool Prefix()
        {
            if (QuantumConsole.Instance?.IsActive ?? false)
            {
                PlayerChatManager.ExitChatIfInChatMode();
                return false;
            }
            return true;
        }
    }

    [ArchivePatch(typeof(LG_TERM_PlayerInteracting), nameof(LG_TERM_PlayerInteracting.ParseInput))]
    private class LG_TERM_PlayerInteracting__ParseInput__Patch
    {
        private static bool Prefix()
        {
            if (QuantumConsole.Instance?.IsActive ?? false)
            {
                return false;
            }
            return true;
        }
    }

    [ArchivePatch(typeof(Cursor), nameof(Cursor.visible), null, ArchivePatch.PatchMethodType.Setter)]
    private class Cursor__set_visible__Patch
    {
        private static void Prefix(ref bool value)
        {
            if (QuantumConsole.Instance?.IsActive ?? false)
            {
                value = true;
            }
        }
    }

    [ArchivePatch(typeof(Cursor), nameof(Cursor.lockState), null, ArchivePatch.PatchMethodType.Setter)]
    private class Cursor__set_lockState__Patch
    {
        private static void Prefix(ref CursorLockMode value)
        {
            if (QuantumConsole.Instance?.IsActive ?? false)
            {
                value = CursorLockMode.None;
            }
        }
    }

    [ArchivePatch(typeof(InputMapper), nameof(InputMapper.DoGetAxis))]
    private class InputMapper__DoGetAxis__Patch
    {
        private static bool Prefix(ref float __result)
        {
            if (Cursor.lockState == CursorLockMode.None || (QuantumConsole.Instance?.IsActive ?? false))
            {
                __result = 0f;
                return false;
            }
            return true;
        }
    }

    [ArchivePatch(typeof(InputMapper), nameof(InputMapper.DoGetButton))]
    private class InputMapper__DoGetButton__Patch
    {
        private static bool Prefix(ref bool __result)
        {
            if (Cursor.lockState == CursorLockMode.None || (QuantumConsole.Instance?.IsActive ?? false))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [ArchivePatch(typeof(InputMapper), nameof(InputMapper.DoGetButtonDown))]
    private class InputMapper__DoGetButtonDown__Patch
    {
        private static bool Prefix(ref bool __result)
        {
            if (Cursor.lockState == CursorLockMode.None || (QuantumConsole.Instance?.IsActive ?? false))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [ArchivePatch(typeof(InputMapper), nameof(InputMapper.DoGetButtonUp))]
    private class InputMapper__DoGetButtonUp__Patch
    {
        private static bool Prefix(ref bool __result)
        {
            if (Cursor.lockState == CursorLockMode.None || (QuantumConsole.Instance?.IsActive ?? false))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}