using HuaZi.Library.Json;
using QuickTranslator.Class;
using QuickTranslator.Class.JsonConfigs;
using QuickTranslator.Utils;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HuaZi.Library.InputManager;
using static QuickTranslator.Class.AppLogger;
using Notifications.Wpf;

namespace QuickTranslator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GlobalKeyboardHook keyboardHook;
        private bool isInitComplete = false;
        private long? latestSpacePress = null;
        private int spacePressCount = 0;

        #region Load
        public void SetLoad(bool isLoad = true)
        {
            grid_Loading.Visibility = isLoad ? Visibility.Visible : Visibility.Hidden;
            grid_Main.Effect = isLoad ? new BlurEffect { Radius = 10 } : null;
            grid_Main.IsEnabled = !isLoad;
        }

        public void StartLoad() => SetLoad(true);
        public void EndLoad() => SetLoad(false);
        #endregion

        #region Init
        public async void Initialize()
        {
            try
            {
                StartLoad();

                #region 配置文件
                //初始化配置
                if (!File.Exists(AppInfo.ConfigPath))
                    Json.WriteJson(AppInfo.ConfigPath, new JsonAppConfig.Index());

                //读配置
                AppInfo.Config = Json.ReadJson<JsonAppConfig.Index>(AppInfo.ConfigPath);
                #endregion

                #region 加载语言列表
                //加载语言列表
                JsonLanguageList.Index languageList = await TranslateManager.GetLanguageList();
                comboBox_SourceLang.Items.Clear(); comboBox_TargetLang.Items.Clear();
                //添加Auto项
                comboBox_SourceLang.Items.Add(new ComboBoxItem { Content = "自动判断 (auto)", Tag = "auto" }); comboBox_TargetLang.Items.Add(new ComboBoxItem { Content = "自动判断 (auto)", Tag = "auto" });

                foreach (var lang in languageList.Data)
                {
                    comboBox_SourceLang.Items.Add(new ComboBoxItem
                    {
                        Content = $"{lang.Label} ({lang.Code})",
                        Tag = lang.Code,
                    });
                    comboBox_TargetLang.Items.Add(new ComboBoxItem
                    {
                        Content = $"{lang.Label} ({lang.Code})",
                        Tag = lang.Code,
                    });
                }

                //根据配置选择选项
                foreach (var item in comboBox_SourceLang.Items)
                    if (((ComboBoxItem)item).Tag.ToString() == AppInfo.Config.SourceLanguage)
                        comboBox_SourceLang.SelectedItem = item;
                foreach (var item in comboBox_TargetLang.Items)
                    if (((ComboBoxItem)item).Tag.ToString() == AppInfo.Config.TargetLanguage)
                        comboBox_TargetLang.SelectedItem = item;
                #endregion

                isInitComplete = true;
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", null!, ex);
            }
            finally
            {
                EndLoad();
            }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            //注册钩子
            keyboardHook = new GlobalKeyboardHook(this);

            Loaded += MainWindow_Loaded;
            keyboardHook.KeyDown += HookKeyDown;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) => Initialize();

        private async void HookKeyDown(object? sender, WpfKeyboardHookEventArgs e)
        {
            if (e.Key != Key.Space || !isInitComplete) return;

            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (latestSpacePress == null)
            {
                latestSpacePress = now;
                spacePressCount = 1;
                return;
            }

            long delta = now - latestSpacePress.Value;

            if (delta < 800)
            {
                spacePressCount++;
                logger.Info($"连击次数: {spacePressCount}, 间隔: {delta}ms");
            }
            else
            {
                spacePressCount = 1;
            }

            latestSpacePress = now;

            if (spacePressCount == 3)
            {
                spacePressCount = 0;

                await TranslateCurrentInputFieldAsync();
            }
        }

        private async Task TranslateCurrentInputFieldAsync()
        {
            try
            {
                

                await Task.Delay(50);

                void SendCtrlKey(byte key)
                {
                    HuaZi.Library.InputManager.InputManager.Keyboard.KeyDown(0x11);
                    HuaZi.Library.InputManager.InputManager.Keyboard.KeyPress(key);
                    HuaZi.Library.InputManager.InputManager.Keyboard.KeyUp(0x11);
                }

                string originalText;
                Clipboard.Clear();
                SendCtrlKey(0x41); // Ctrl+A
                Task.Delay(30).Wait();
                SendCtrlKey(0x43); // Ctrl+C
                Task.Delay(80).Wait();
                originalText = Clipboard.ContainsText() ? Clipboard.GetText() : string.Empty;

                if (string.IsNullOrWhiteSpace(originalText))
                {
                    logger.Warn("未获取到文本，可能是非文本输入框");
                    return;
                }

                logger.Info($"原文: {originalText}");

                new NotificationManager().Show(new NotificationContent
                {
                    Title = "快速翻译",
                    Message = "正在翻译...",
                    Type = NotificationType.Information
                });
                var result = await TranslateManager.GetTranslate(originalText,AppInfo.Config.SourceLanguage,AppInfo.Config.TargetLanguage);

                if (result?.Data?.Target?.Text == null)
                {
                    logger.Error("翻译失败");
                    return;
                }

                string translated = result.Data.Target.Text;
                logger.Info($"译文: {translated}");

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Clipboard.SetText(translated);
                    Task.Delay(30).Wait();
                    SendCtrlKey(0x56); // Ctrl+V
                });
            }
            catch (Exception ex)
            {
                logger.Error($"快速翻译过程中发生异常 : {ex}");
                ErrorReportDialog.Show("翻译失败", null!, ex);
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(EndLoad);
            }
        }

        //保存语言信息
        private void comboBox_SourceLang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitComplete)
            {
                AppInfo.Config.SourceLanguage = ((ComboBoxItem)comboBox_SourceLang.SelectedItem).Tag.ToString()!;
                AppInfo.Config.TargetLanguage = ((ComboBoxItem)comboBox_TargetLang.SelectedItem).Tag.ToString()!;

                Json.WriteJson(AppInfo.ConfigPath, AppInfo.Config);
            }
        }
    }
}