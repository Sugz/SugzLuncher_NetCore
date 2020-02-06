using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SugzLuncher.ViewModels;
using SugzLuncher.Helpers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Interop;
using System.Diagnostics;
using System.Collections;

namespace SugzLuncher.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        readonly KeyboardHook _KeyboardHook = new KeyboardHook();
        readonly List<KeyboardHook.VKeys> _PressedKeys = new List<KeyboardHook.VKeys>();
        readonly WindowZOrder _ZOrder;


        public MainWindow()
        {
            InitializeComponent();

            _ZOrder = new WindowZOrder(this, WindowZOrder.ZOrder.HWND_BOTTOM, true);

            Loaded += OnLoaded;
            Closed += OnClosed;

            _KeyboardHook.Install();
            _KeyboardHook.KeyDown += OnKeyboardHookKeyDown;
            _KeyboardHook.KeyUp += OnKeyboardHookKeyUp;

            MainList.MouseEnter += (s, e) => SetOnTop(true);
            MainList.Closed += (s, e) => SetOnTop(false);
        }


        

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetBinding(LeftProperty, new Binding("WindowLocation.X"));
            SetBinding(TopProperty, new Binding("WindowLocation.Y"));
        }

        private void OnClosed(object sender, EventArgs e)
        {
            _KeyboardHook.Uninstall();
        }


        private void OnKeyboardHookKeyDown(KeyboardHook.VKeys key)
        {
            if (_PressedKeys.IndexOf(key) < 0)
                _PressedKeys.Add(key);
        }

        private void OnKeyboardHookKeyUp(KeyboardHook.VKeys key)
        {
            _PressedKeys.Remove(key);
            if (_PressedKeys.Count == 1 &&
                _PressedKeys.First() == KeyboardHook.VKeys.LWIN &&
                key == KeyboardHook.VKeys.LMENU)
            {
                SetOnTop(true);
            }
        }

        private void SetOnTop(bool value)
        {
            if (value)
            {
                _ZOrder.SetZOrder(WindowZOrder.ZOrder.HWND_TOPMOST);
                WindowBlur.EnableBlur(this, true);
                MainList.ShowBackground(true);
            }
            else
            {
                _ZOrder.SetZOrder(WindowZOrder.ZOrder.HWND_BOTTOM);
                WindowBlur.EnableBlur(this, false);
            }
        }

    }


}