using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace StickyNotes;

internal sealed class TemporaryFrontHotkey : IDisposable
{
    internal const string Gesture = "Alt + Z";
    private const int HotkeyId = 1;
    private const int HotkeyMessage = 0x0312;
    private readonly Action activate;
    private readonly HwndSource source;
    private bool disposed;

    internal IntPtr Handle => source.Handle;

    internal TemporaryFrontHotkey(Action activate, uint modifiers = 0x0001, uint key = 0x5A)
    {
        this.activate = activate;
        // A message-only window keeps the shortcut alive even when every note is hidden.
        source = new HwndSource(new HwndSourceParameters("StickyNotes.TemporaryFrontHotkey")
        {
            ParentWindow = new IntPtr(-3), Width = 0, Height = 0, WindowStyle = 0
        });
        source.AddHook(OnMessage);
        if (!RegisterHotKey(source.Handle, HotkeyId, modifiers | 0x4000, key)) // MOD_NOREPEAT
        {
            var error = Marshal.GetLastWin32Error();
            source.RemoveHook(OnMessage);
            source.Dispose();
            throw new Win32Exception(error);
        }
    }

    private IntPtr OnMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (!disposed && message == HotkeyMessage && wParam.ToInt64() == HotkeyId)
        {
            handled = true;
            activate();
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        UnregisterHotKey(source.Handle, HotkeyId);
        source.RemoveHook(OnMessage);
        source.Dispose();
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(IntPtr hwnd, int id, uint modifiers, uint key);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(IntPtr hwnd, int id);
}
