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

namespace QuickTranslator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GlobalKeyboardHook keyboardHook;
        private bool isInitComplete = false;

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
                comboBox_SourceLang.Items.Clear();comboBox_TargetLang.Items.Clear();
                //添加Auto项
                comboBox_SourceLang.Items.Add(new ComboBoxItem { Content = "自动判断 (auto)", Tag = "auto" });comboBox_TargetLang.Items.Add(new ComboBoxItem { Content = "自动判断 (auto)", Tag = "auto" });

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

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) => Initialize();

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