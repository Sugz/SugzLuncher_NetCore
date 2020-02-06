using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace SugzLuncher.Helpers
{
	public static class WindowBlur
	{

		class NativeMethods
		{
			[DllImport("user32.dll")]
			internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

			[StructLayout(LayoutKind.Sequential)]
			internal struct WindowCompositionAttributeData
			{
				public WindowCompositionAttribute Attribute;
				public IntPtr Data;
				public int SizeOfData;
			}

			internal enum WindowCompositionAttribute
			{
				// ...
				WCA_ACCENT_POLICY = 19
				// ...
			}

			internal enum AccentState
			{
				ACCENT_DISABLED = 0,
				ACCENT_ENABLE_GRADIENT = 1,
				ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
				ACCENT_ENABLE_BLURBEHIND = 3,
				ACCENT_INVALID_STATE = 4
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct AccentPolicy
			{
				public AccentState AccentState;
				public int AccentFlags;
				public int GradientColor;
				public int AnimationId;
			}
		}


		public static void EnableBlur(Window window, bool value)
		{
			var windowHelper = new WindowInteropHelper(window);

			var accent = new NativeMethods.AccentPolicy();
			var accentStructSize = Marshal.SizeOf(accent);
			accent.AccentState = value ? NativeMethods.AccentState.ACCENT_ENABLE_BLURBEHIND : NativeMethods.AccentState.ACCENT_DISABLED;

			var accentPtr = Marshal.AllocHGlobal(accentStructSize);
			Marshal.StructureToPtr(accent, accentPtr, false);

			var data = new NativeMethods.WindowCompositionAttributeData
			{
				Attribute = NativeMethods.WindowCompositionAttribute.WCA_ACCENT_POLICY,
				SizeOfData = accentStructSize,
				Data = accentPtr
			};

			NativeMethods.SetWindowCompositionAttribute(windowHelper.Handle, ref data);

			Marshal.FreeHGlobal(accentPtr);
		}
	}
}
