using System.Runtime.InteropServices;
using BusBooking.Client.Theme;

namespace BusBooking.Client.Controls;

/// <summary>
/// Helper bo góc mượt mà cho Form:
/// - Windows 11 (Build >= 22000): Ưu tiên sử dụng Windows DWM API DwmSetWindowAttribute (DWMWA_WINDOW_CORNER_PREFERENCE).
/// - Windows 10 hoặc fallback: Cắt Region bo tròn mượt mà và tự động tính toán lại khi Resize.
/// </summary>
public static class FormRoundingHelper
{
    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

    public enum DwmWindowCornerPreference
    {
        Default = 0,
        DoNotRound = 1,
        Round = 2,
        RoundSmall = 3
    }

    [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        int dwAttribute,
        ref int pvAttribute,
        int cbAttribute);

    /// <summary>
    /// Kích hoạt bo góc cho Form (tự động nhận diện Windows 11 DWM hoặc fallback Windows 10 Region).
    /// </summary>
    public static void Apply(Form form, int radius = 14)
    {
        if (form == null) return;

        form.Load += (_, _) => ApplyRoundingInternal(form, radius);
        if (form.IsHandleCreated)
        {
            ApplyRoundingInternal(form, radius);
        }
    }

    private static void ApplyRoundingInternal(Form form, int radius)
    {
        if (form.IsDisposed || !form.IsHandleCreated) return;

        // 1. Thử kích hoạt bo góc phần cứng qua DWM trên Windows 11
        try
        {
            if (Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 22000)
            {
                int preference = (int)DwmWindowCornerPreference.Round;
                int hr = DwmSetWindowAttribute(form.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
                if (hr == 0)
                {
                    return; // Đã kích hoạt DWM bo góc thành công
                }
            }
        }
        catch
        {
            // Bỏ qua lỗi DWM trên các phiên bản Windows cũ
        }

        // 2. Fallback cho Windows 10 hoặc Form không viền (None)
        ApplyRegionRounding(form, radius);
    }

    /// <summary>
    /// Fallback cắt Region bo góc vật lý kèm tự động cập nhật khi Form Resize.
    /// </summary>
    public static void ApplyRegionRounding(Form form, int radius)
    {
        if (form == null || form.IsDisposed) return;

        void UpdateRegion()
        {
            if (form.IsDisposed || form.Width <= 0 || form.Height <= 0) return;
            var rect = new Rectangle(0, 0, form.Width, form.Height);
            using var path = AppTheme.CreateRoundedRect(rect, radius);
            form.Region?.Dispose();
            form.Region = new Region(path);
        }

        form.Resize += (_, _) => UpdateRegion();
        UpdateRegion();
    }
}
