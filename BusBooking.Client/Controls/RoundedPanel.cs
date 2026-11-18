using System.ComponentModel;
using System.Drawing.Drawing2D;
using BusBooking.Client.Helpers;
using BusBooking.Client.Theme;

namespace BusBooking.Client.Controls;

/// <summary>
/// Panel bo góc hiện đại chuẩn WinForms GDI+ với hỗ trợ Visual Studio Properties,
/// cắt Region vật lý chống lộ 4 góc vuông và vẽ viền AntiAlias mượt mà.
/// </summary>
public class RoundedPanel : Panel
{
    private int _borderRadius = AppTheme.CardRadius;
    private Color _fillColor = AppTheme.CardSurface;
    private Color _borderColor = AppTheme.CardBorder;
    private int _borderSize = 1;
    private Color _leftAccentColor = Color.Transparent;

    public RoundedPanel()
    {
        DoubleBuffered = true;
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true);
        BackColor = Color.Transparent;
        Padding = new Padding(16);
    }

    [Category("Appearance")]
    [DefaultValue(12)]
    [Description("Bán kính bo tròn góc của panel (pixel).")]
    public int BorderRadius
    {
        get => _borderRadius;
        set
        {
            _borderRadius = Math.Max(0, value);
            UpdateRegion();
            Invalidate();
        }
    }

    [Browsable(false)]
    public int CornerRadius
    {
        get => BorderRadius;
        set => BorderRadius = value;
    }

    [Category("Appearance")]
    [DefaultValue(typeof(Color), "White")]
    [Description("Màu nền bề mặt của panel bo góc.")]
    public Color FillColor
    {
        get => _fillColor;
        set
        {
            _fillColor = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    public Color SurfaceColor
    {
        get => FillColor;
        set => FillColor = value;
    }

    [Category("Appearance")]
    [DefaultValue(typeof(Color), "226, 232, 240")]
    [Description("Màu đường viền bo góc.")]
    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            _borderColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(1)]
    [Description("Độ dày đường viền bo góc (pixel).")]
    public int BorderSize
    {
        get => _borderSize;
        set
        {
            _borderSize = Math.Max(0, value);
            Invalidate();
        }
    }

    [Browsable(false)]
    public int BorderThickness
    {
        get => BorderSize;
        set => BorderSize = value;
    }

    [Category("Appearance")]
    [DefaultValue(typeof(Color), "Transparent")]
    [Description("Màu thanh dải nhấn bên trái.")]
    public Color LeftAccentColor
    {
        get => _leftAccentColor;
        set
        {
            _leftAccentColor = value;
            Invalidate();
        }
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateRegion();
    }

    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);
        Invalidate();
    }

    protected override void OnParentBackColorChanged(EventArgs e)
    {
        base.OnParentBackColorChanged(e);
        Invalidate();
    }

    public void UpdateRegion()
    {
        if (Width <= 0 || Height <= 0) return;

        if (_borderRadius <= 0)
        {
            Region?.Dispose();
            Region = null;
            return;
        }

        var rect = new Rectangle(0, 0, Width, Height);
        using var path = AppTheme.CreateRoundedRect(rect, _borderRadius);
        Region?.Dispose();
        Region = new Region(path);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // Hoàn toàn không gọi base để chống nháy hình
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // 1. Tô sạch 4 góc vuông bằng màu nền thực tế của container cha để bo tròn không bị đen hay lộ viền vuông
        Color parentBg = UiHelper.GetEffectiveBackColor(Parent);
        using (var parentBrush = new SolidBrush(parentBg))
        {
            e.Graphics.FillRectangle(parentBrush, ClientRectangle);
        }

        // 2. Vẽ nền bề mặt bo tròn
        var rect = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        using var path = AppTheme.CreateRoundedRect(rect, _borderRadius);
        using (var bgBrush = new SolidBrush(_fillColor))
        {
            e.Graphics.FillPath(bgBrush, path);
        }

        // 3. Dải màu nhấn bên trái (nếu có)
        if (_leftAccentColor != Color.Transparent && _leftAccentColor.A > 0)
        {
            var prevClip = e.Graphics.Clip;
            e.Graphics.SetClip(path);
            using var accentBrush = new SolidBrush(_leftAccentColor);
            e.Graphics.FillRectangle(accentBrush, new Rectangle(0, 0, 5, Height));
            e.Graphics.Clip = prevClip;
        }

        // 4. Vẽ viền bo tròn hoặc viền mượt khử răng cưa mép cắt
        if (_borderSize > 0 && _borderColor != Color.Transparent && _borderColor.A > 0)
        {
            using var pen = new Pen(_borderColor, _borderSize);
            e.Graphics.DrawPath(pen, path);
        }
        else
        {
            using var smoothPen = new Pen(parentBg, 1f);
            e.Graphics.DrawPath(smoothPen, path);
        }
    }
}
