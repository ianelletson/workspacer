﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace workspacer
{
    public delegate void WindowFocusedDelegate();

    public class WindowsWindow : IWindow
    {
        private static Logger Logger = Logger.Create();

        private IntPtr _handle;
        private bool _didManualHide;

        public WindowFocusedDelegate WindowFocused;

        private int _processId;
        private string _processName;
        private string _processFileName;

        public WindowsWindow(IntPtr handle)
        {
            _handle = handle;

            try
            {
                uint processId;
                Win32.GetWindowThreadProcessId(_handle, out processId);

                _processId = (int)processId;

                var process = Process.GetProcesses().FirstOrDefault(p => p.Id == _processId);
                _processName = process.ProcessName;
                _processFileName = Path.GetFileName(process.MainModule.FileName);
                ProcessDescription = process.MainModule.FileVersionInfo.FileDescription;
            }
            catch (Exception)
            {
                _processId = -1;
                _processName = string.Empty;
                _processFileName = string.Empty;
                ProcessDescription = string.Empty;
            }
        }

        public bool DidManualHide => _didManualHide;

        public string Title
        {
            get
            {
                var buffer = new StringBuilder(255);
                Win32.GetWindowText(_handle, buffer, buffer.Capacity + 1);
                return buffer.ToString();
            }
        }

        public string ProcessDescription { get; private set; }

        public IntPtr Handle => _handle;

        public string Class
        {
            get
            {
                var buffer = new StringBuilder(255);
                Win32.GetClassName(_handle, buffer, buffer.Capacity + 1);
                return buffer.ToString();            }
        }

        public IWindowLocation Location
        {
            get
            {
                Win32.Rect rect = new Win32.Rect();
                Win32.GetWindowRect(_handle, ref rect);

                WindowState state = WindowState.Normal;
                if (IsMinimized)
                {
                    state = WindowState.Minimized;
                }
                else if (IsMaximized)
                {
                    state = WindowState.Maximized;
                }

                return new WindowLocation(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, state);
            }
        }

        public int ProcessId => _processId;
        public string ProcessFileName => _processFileName;
        public string ProcessName => _processName;

        public bool CanLayout
        {
            get
            {
                return _didManualHide || 
                    (!Win32Helper.IsCloaked(_handle) &&
                       Win32Helper.IsAppWindow(_handle) &&
                       Win32Helper.IsAltTabWindow(_handle));
            }
        }


        public bool IsFocused => Win32.GetForegroundWindow() == _handle;
        public bool IsMinimized => Win32.IsIconic(_handle);
        public bool IsMaximized => Win32.IsZoomed(_handle);
        public bool IsMouseMoving { get; internal set; }

        public void Focus()
        {
            if (!IsFocused)
            {
                Logger.Debug("[{0}] :: Focus", this);
                Win32Helper.ForceForegroundWindow(_handle);
                WindowFocused?.Invoke();
            }
        }

        public void Hide()
        {
            Logger.Trace("[{0}] :: Hide", this);
            if (CanLayout)
            {
                _didManualHide = true;
            }
            Win32.ShowWindow(_handle, Win32.SW.SW_HIDE);
        }

        public void ShowNormal()
        {
            _didManualHide = false;
            Logger.Trace("[{0}] :: ShowNormal", this);
            Win32.ShowWindow(_handle, Win32.SW.SW_SHOWNOACTIVATE);
        }

        public void ShowMaximized()
        {
            _didManualHide = false;
            Logger.Trace("[{0}] :: ShowMaximized", this);
            Win32.ShowWindow(_handle, Win32.SW.SW_SHOWMAXIMIZED);
        }

        public void ShowMinimized()
        {
            _didManualHide = false;
            Logger.Trace("[{0}] :: ShowMinimized", this);
            Win32.ShowWindow(_handle, Win32.SW.SW_SHOWMINIMIZED);
        }

        public void ShowInCurrentState()
        {
            if (IsMinimized)
            {
                ShowMinimized();
            }
            else if (IsMaximized)
            {
                ShowMaximized();
            }
            else
            {
                ShowNormal();
            }
        }

        public void BringToTop()
        {
            Win32.BringWindowToTop(_handle);
        }

        public void Close()
        {
            Logger.Debug("[{0}] :: Close", this);
            Win32Helper.QuitApplication(_handle);
        }

        public override string ToString()
        {
            return $"[{Handle}][{Title}][{Class}][{ProcessName}]";
        }
    }
}
