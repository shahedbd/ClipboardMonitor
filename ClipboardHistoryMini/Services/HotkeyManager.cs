using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClipboardHistoryMini.Services
{
    public class HotkeyManager : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 9000;

        // Modifiers
        private const uint MOD_NONE = 0x0000;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        private IntPtr _windowHandle;
        private bool _isRegistered = false;

        public event EventHandler HotkeyPressed;

        public HotkeyManager(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
        }

        public bool RegisterHotkey(Keys key, bool ctrl = false, bool shift = false, bool alt = false, bool win = false)
        {
            UnregisterHotkey();

            uint modifiers = MOD_NONE;
            if (ctrl) modifiers |= MOD_CONTROL;
            if (shift) modifiers |= MOD_SHIFT;
            if (alt) modifiers |= MOD_ALT;
            if (win) modifiers |= MOD_WIN;

            _isRegistered = RegisterHotKey(_windowHandle, HOTKEY_ID, modifiers, (uint)key);
            return _isRegistered;
        }

        public void UnregisterHotkey()
        {
            if (_isRegistered)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
                _isRegistered = false;
            }
        }

        public bool ProcessHotkey(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            UnregisterHotkey();
        }
    }
}