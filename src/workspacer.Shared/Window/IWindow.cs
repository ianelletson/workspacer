﻿using System;

namespace workspacer
{
    public interface IWindow
    {
        IntPtr Handle { get; }
        string Title { get; }
        string Class { get; }
        IWindowLocation Location { get; }

        int ProcessId { get; }
        string ProcessFileName { get; }
        string ProcessName { get; }
        string ProcessDescription { get; }

        bool CanLayout { get; }

        bool IsFocused { get; }
        bool IsMinimized { get; }
        bool IsMaximized { get; }
        bool IsMouseMoving { get; }

        void Focus();
        void Hide();
        void ShowNormal();
        void ShowMaximized();
        void ShowMinimized();
        void ShowInCurrentState();

        void BringToTop();

        void Close();
    }
}
