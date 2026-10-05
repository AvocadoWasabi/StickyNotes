using System.Runtime.InteropServices;
using System.Windows.Interop;
using Forms = System.Windows.Forms;

namespace StickyNotes;

internal static class MonitorLayout
{
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr window, out Rect rect);
    [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr window, IntPtr after, int x, int y, int cx, int cy, uint flags);

    public static void Restore(Window window, NotePlacement placement)
    {
        var handle = new WindowInteropHelper(window).Handle;
        var screen = Forms.Screen.AllScreens.FirstOrDefault(s => s.DeviceName == placement.Monitor) ?? Forms.Screen.FromHandle(handle);
        var area = screen.WorkingArea;
        var x = placement.HasPixelPosition ? placement.PixelLeft : area.Left + 80;
        var y = placement.HasPixelPosition ? placement.PixelTop : area.Top + 80;
        if (GetWindowRect(handle, out var rect))
        {
            x = Math.Clamp(x, area.Left, Math.Max(area.Left, area.Right - Math.Min(rect.Right - rect.Left, area.Width)));
            y = Math.Clamp(y, area.Top, Math.Max(area.Top, area.Bottom - Math.Min(rect.Bottom - rect.Top, area.Height)));
        }
        SetWindowPos(handle, IntPtr.Zero, x, y, 0, 0, 0x0001 | 0x0004 | 0x0010);
    }

    public static void Capture(Window window, NotePlacement placement)
    {
        var handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero || !GetWindowRect(handle, out var rect)) return;
        placement.PixelLeft = rect.Left; placement.PixelTop = rect.Top;
        placement.HasPixelPosition = true;
        placement.Monitor = Forms.Screen.FromHandle(handle).DeviceName;
        placement.Left = window.Left; placement.Top = window.Top;
        placement.Width = window.ActualWidth; placement.Height = window.ActualHeight;
    }
}
