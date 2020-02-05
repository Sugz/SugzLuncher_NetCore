/*
    Solution to find Desktop: 
    https://www.codeproject.com/Questions/1036180/Can-I-somehow-set-the-Parent-of-SHELLDLL-DefView-h
    https://www.codeproject.com/Articles/856020/Draw-Behind-Desktop-Icons-in-Windows-plus?fid=1875575&df=90&mpp=25&prof=True&sort=Position&view=Normal&spc=Relaxed&fr=26

    Stick window on bottom:
    https://stackoverflow.com/a/56906138/3971575
*/



using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace SugzLuncher.Helpers
{
    /// <summary>
    /// Helper class to set a Window Z order. 
    /// </summary>
    public class WindowZOrder : IDisposable
    {

        #region Win API

        public enum ZOrder
        {
            HWND_BOTTOM = 1,
            HWND_NOTOPMOST = -2,
            HWND_TOP = 0,
            HWND_TOPMOST = -1,
            None = 2,
        }


        class NativeMethods
        {
            public const int WM_WINDOWPOSCHANGING = 0x0046;

            public const uint SWP_NOSIZE = 0x0001;
            public const uint SWP_NOMOVE = 0x0002;
            public const uint SWP_NOZORDER = 0x0004;
            public const uint SWP_NOACTIVATE = 0x0010;

            public const int WmSpawnWorker = 0x052C;

            [StructLayout(LayoutKind.Sequential)]
            public struct WINDOWPOS
            {
                public IntPtr hwnd;
                public IntPtr hwndInsertAfter;
                public int x;
                public int y;
                public int cx;
                public int cy;
                public uint flags;
            }


            [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

            [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

            [DllImport("user32.dll", EntryPoint = "GetDesktopWindow", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetDesktopWindow();

            [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, IntPtr lParam);

            [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr SetWindowLongPtr(IntPtr hwnd, int nIndex, IntPtr dwNewLong);

            [DllImport("user32.dll", EntryPoint = "SetWindowPos", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
        } 

        #endregion Win API



        #region Fields

        private readonly Window _Window;
        private bool _Disposed;
        private ZOrder _CurrentZOrder;
        private ZOrder _oldZOrder = ZOrder.None;
        private bool _IsDesktopChild;

        #endregion Fields



        #region Constructor

        public WindowZOrder(Window window, ZOrder zOrder, bool isDesktopChild)
        {
            _Window = window;
            _CurrentZOrder = zOrder;
            _IsDesktopChild = isDesktopChild;

            if (window.IsLoaded)
                OnWindowLoaded(window, null);
            else
                window.Loaded += OnWindowLoaded;

            window.Closing += OnWindowClosing;
        }

        #endregion Constructor



        #region Public Methods

        public void SetZOrder(ZOrder zOrder)
        {
            _CurrentZOrder = zOrder;

            NativeMethods.SetWindowPos(
                new WindowInteropHelper(_Window).Handle,
                new IntPtr((int)zOrder),
                0,
                0,
                0,
                0,
                NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOACTIVATE);
        }

        #endregion Public Methods



        #region Private Methods

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (_IsDesktopChild)
                SetWindowAsDesktopChild();

            SetZOrder(_CurrentZOrder);

            var source = HwndSource.FromHwnd(new WindowInteropHelper(_Window).Handle);
            source?.AddHook(WndProc);
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            var source = HwndSource.FromHwnd(new WindowInteropHelper(_Window).Handle);
            source?.RemoveHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_WINDOWPOSCHANGING && _oldZOrder == _CurrentZOrder)
            {
                var windowPos = Marshal.PtrToStructure<NativeMethods.WINDOWPOS>(lParam);
                windowPos.flags |= NativeMethods.SWP_NOZORDER;
                Marshal.StructureToPtr(windowPos, lParam, false);
            }

            _oldZOrder = _CurrentZOrder;
            

            return IntPtr.Zero;
        }

        private void SetWindowAsDesktopChild()
        {
            NativeMethods.SetWindowLongPtr(new WindowInteropHelper(_Window).Handle, -8, GetDesktopListView());
        }

        private IntPtr GetDesktopListView()
        {
            IntPtr hDesktop;
            IntPtr hProgman = NativeMethods.FindWindow("Progman", "Program Manager");
            if (hProgman != IntPtr.Zero)
            {
                hDesktop = NativeMethods.FindWindowEx(hProgman, IntPtr.Zero, "SHELLDLL_DefView", null);

                if (hDesktop != IntPtr.Zero)
                    return hDesktop;
            }

            //If we reach this point, that means that "SHELLDLL_DefView" is NOT a sibling
            //of "Progman"! Now we gotta spawn "WorkerW" from "Progman"(Windows 7 and beyond)
            //(shhhh... this windows message is NOT DOCUMENTED!)
            
            NativeMethods.SendMessage(hProgman, NativeMethods.WmSpawnWorker, 0, new IntPtr(0));

            hDesktop = NativeMethods.GetDesktopWindow();
            IntPtr hShellViewWin;
            IntPtr hWorkerW = IntPtr.Zero;
            do
            {
                hWorkerW = NativeMethods.FindWindowEx(hDesktop, hWorkerW, "WorkerW", null);
                hShellViewWin = NativeMethods.FindWindowEx(hWorkerW, IntPtr.Zero, "SHELLDLL_DefView", null);
            }
            while (hShellViewWin == IntPtr.Zero && hWorkerW != IntPtr.Zero);

            return hShellViewWin;
        }

        #endregion Private Methods



        #region IDisposable

        ~WindowZOrder()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed)
                return;

            _Window.Loaded -= OnWindowLoaded;
            _Window.Closing -= OnWindowClosing;

            _Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable


    }
}
