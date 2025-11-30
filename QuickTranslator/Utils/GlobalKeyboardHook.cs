using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace QuickTranslator.Utils
{
    public class GlobalKeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private readonly LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;
        private readonly Window _ownerWindow;

        /// <summary>
        /// 按键按下事件（在后台线程触发）
        /// </summary>
        public event EventHandler<WpfKeyboardHookEventArgs> KeyDown;

        /// <summary>
        /// 按键释放事件（在后台线程触发）
        /// </summary>
        public event EventHandler<WpfKeyboardHookEventArgs> KeyUp;

        public GlobalKeyboardHook(Window ownerWindow = null!)
        {
            _ownerWindow = ownerWindow ?? Application.Current.MainWindow;
            _proc = HookCallback;

            // 确保在 UI 线程安装钩子
            Application.Current.Dispatcher.Invoke(() =>
            {
                var helper = new WindowInteropHelper(_ownerWindow);
                var hWnd = helper.Handle;

                // 如果 MainWindow 尚未创建，使用当前进程主窗口
                if (hWnd == IntPtr.Zero)
                    hWnd = Process.GetCurrentProcess().MainWindowHandle;

                _hookID = SetHook(_proc);
                if (_hookID == IntPtr.Zero)
                    throw new InvalidOperationException("无法安装全局键盘钩子，可能需要管理员权限。");
            });
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using var process = Process.GetCurrentProcess();
            using var module = process.MainModule;
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(module!.ModuleName), 0);
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                // 转换为 WPF Key（包含对扩展键的正确处理）
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                // 获取当前修饰键状态（Ctrl/Shift/Alt/Windows）
                ModifierKeys modifiers = GetCurrentModifierKeys();

                bool isKeyDown = wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN;
                bool isKeyUp = wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP;

                if (isKeyDown)
                    KeyDown?.Invoke(this, new WpfKeyboardHookEventArgs(key, modifiers, false));

                if (isKeyUp)
                    KeyUp?.Invoke(this, new WpfKeyboardHookEventArgs(key, modifiers, true));

                // 如果要阻止该按键继续传递给其他窗口，返回 (IntPtr)1
                // return (IntPtr)1;
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// 获取当前键盘修饰键状态（与 Keyboard.Modifiers 完全一致）
        /// </summary>
        private static ModifierKeys GetCurrentModifierKeys()
        {
            ModifierKeys mods = ModifierKeys.None;

            if ((GetKeyState(VK_LSHIFT) & 0x8000) != 0 || (GetKeyState(VK_RSHIFT) & 0x8000) != 0)
                mods |= ModifierKeys.Shift;

            if ((GetKeyState(VK_LCONTROL) & 0x8000) != 0 || (GetKeyState(VK_RCONTROL) & 0x8000) != 0)
                mods |= ModifierKeys.Control;

            if ((GetKeyState(VK_LMENU) & 0x8000) != 0 || (GetKeyState(VK_RMENU) & 0x8000) != 0)
                mods |= ModifierKeys.Alt;

            if ((GetKeyState(VK_LWIN) & 0x8000) != 0 || (GetKeyState(VK_RWIN) & 0x8000) != 0)
                mods |= ModifierKeys.Windows;

            return mods;
        }

        public void Dispose()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }

        #region Win32 API

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        private const int VK_LSHIFT = 0xA0;
        private const int VK_RSHIFT = 0xA1;
        private const int VK_LCONTROL = 0xA2;
        private const int VK_RCONTROL = 0xA3;
        private const int VK_LMENU = 0xA4;
        private const int VK_RMENU = 0xA5;
        private const int VK_LWIN = 0x5B;
        private const int VK_RWIN = 0x5C;

        #endregion
    }

    /// <summary>
    /// 纯 WPF 全局键盘事件参数
    /// </summary>
    public class WpfKeyboardHookEventArgs : EventArgs
    {
        public Key Key { get; }
        public ModifierKeys Modifiers { get; }
        public bool IsKeyUp { get; }

        public WpfKeyboardHookEventArgs(Key key, ModifierKeys modifiers, bool isKeyUp)
        {
            Key = key;
            Modifiers = modifiers;
            IsKeyUp = isKeyUp;
        }
    }
}