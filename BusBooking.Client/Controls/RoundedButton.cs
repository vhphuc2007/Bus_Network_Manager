using System.ComponentModel;
using System.Drawing.Drawing2D;
using BusBooking.Client.Helpers;
using BusBooking.Client.Theme;
using FontAwesome.Sharp;

namespace BusBooking.Client.Controls;

/// <summary>
/// Nút bấm bo tròn góc hiện đại chuẩn WinForms GDI+ với hỗ trợ Visual Studio Properties,
/// cắt Region vật lý chống click thừa ngoài góc, vẽ đè viền AntiAlias mượt mà và hỗ trợ icon FontAwesome.
/// </summary>
public class RoundedButton : Button
{
    private int _borderRadius = 10;
    private Color _fillColor = AppTheme.Primary;
    private Color _hoverColor = AppTheme.PrimaryHover;
    private Color _borderColor = Color.Transparent;
    private int _borderSize = 0;

    private bool _hovered;
    private bool _pressed;
    private IconChar? _icon;
    private int _iconSize = 15;
    private Bitmap? _cachedIconBitmap;
    private Color _cachedIconColor;
    private string? _savedText;
    private int _badgeCount;

    public RoundedButton()
    {
        DoubleBuffered = true;
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseDownBackColor = Color.Transparent;
        FlatAppearance.MouseOverBackColor = Color.Transparent;
        FlatAppearance.CheckedBackColor = Color.Transparent;
        BackColor = Color.Transparent;
        ForeColor = Color.White;
        Font = AppTheme.BodyBoldFont;
        Cursor = Cursors.Hand;
        Size = new Size(140, 40);
    }

    [Category("Appearance")]
    [DefaultValue(10)]
    [Description("Bán kính bo tròn góc của nút bấm (pixel).")]
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
    [DefaultValue(typeof(Color), "79, 70, 229")]
    [Description("Màu nền chính của nút khi ở trạng thái bình thường.")]
    public Color FillColor
    {
        get => _fillColor;
        set
        {
            _fillColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(typeof(Color), "67, 56, 202")]
    [Description("Màu nền của nút khi rê chuột qua (Hover).")]
    public Color HoverColor
    {
        get => _hoverColor;
        set
        {
            _hoverColor = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(typeof(Color), "Transparent")]
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
    [DefaultValue(0)]
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
    public int BorderWidth
    {
        get => BorderSize;
        set => BorderSize = value;
    }

    [Category("Appearance")]
    [DefaultValue(0)]
    [Description("Số lượng thông báo badge trên nút.")]
    public int BadgeCount
    {
        get => _badgeCount;
        set
        {
            int normalized = Math.Max(0, value);
            if (_badgeCount == normalized) return;
            _badgeCount = normalized;
            Invalidate();
        }
    }

    [Category("Appearance")]
    [Description("Icon FontAwesome hiển thị trên nút.")]
    public IconChar? Icon
    {
        get => _icon;
        set
        {
            if (_icon == value) return;
            _icon = value;
            RebuildIconCache();
            Invalidate();
        }
    }

    [Category("Appearance")]
    [DefaultValue(15)]
    [Description("Kích thước của icon trên nút.")]
    public int IconSize
    {
        get => _iconSize;
        set
        {
            if (_iconSize == value) return;
            _iconSize = value;
            RebuildIconCache();
            Invalidate();
        }
    }

    public void SetLoading(bool loading, string loadingText = "Đang xử lý...")
    {
        if (loading)
        {
            _savedText ??= Text;
            Text = loadingText;
            Enabled = false;
        }
        else
        {
            if (_savedText != null)
            {
                Text = _savedText;
                _savedText = null;
            }
            Enabled = true;
        }
    }

    private void RebuildIconCache()
    {
        _cachedIconBitmap?.Dispose();
        _cachedIconBitmap = null;

        if (_icon.HasValue)
        {
            _cachedIconColor = ForeColor;
            _cachedIconBitmap = _icon.Value.ToBitmap(ForeColor, _iconSize);
        }
    }

    protected override void OnForeColorChanged(EventArgs e)
    {
        base.OnForeColorChanged(e);
        if (_icon.HasValue && _cachedIconColor != ForeColor)
        {
            RebuildIconCache();
        }
    }

    protected override bool ShowFocusCues => false;

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

    protected override void OnMouseEnter(EventArgs e)
    {
        _hovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hovered = false;
        _pressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        _pressed = true;
        Invalidate();
        base.OnMouseDown(mevent);
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        _pressed = false;
        Invalidate();
        base.OnMouseUp(mevent);
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // Hoàn toàn không gọi base để tránh WinForms vẽ khung chữ nhật mặc định
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // 1. Tô sạch 4 góc vuông bằng màu nền thực tế của container cha để bo tròn không bị đen hay lộ viền vuông
        Color parentBg = UiHelper.GetEffectiveBackColor(Parent);
        using (var parentBrush = new SolidBrush(parentBg))
        {
            pevent.Graphics.FillRectangle(parentBrush, ClientRectangle);
        }

        // 2. Vẽ thân nút bo tròn
        var rect = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        using var path = AppTheme.CreateRoundedRect(rect, _borderRadius);

        Color activeFill = !Enabled
            ? Color.FromArgb(203, 213, 225)
            : _pressed
                ? ControlPaint.Dark(_hoverColor, 0.08f)
                : _hovered
                    ? _hoverColor
                    : _fillColor;

        using (var fillBrush = new SolidBrush(activeFill))
        {
            pevent.Graphics.FillPath(fillBrush, path);
        }

        // 3. Khử răng cưa mép cắt bằng đường viền mượt
        if (_borderSize > 0 && _borderColor != Color.Transparent && _borderColor.A > 0)
        {
            using var pen = new Pen(_borderColor, _borderSize);
            pevent.Graphics.DrawPath(pen, path);
        }
        else
        {
            using var smoothPen = new Pen(parentBg, 1f);
            pevent.Graphics.DrawPath(smoothPen, path);
        }

        // 4. Vẽ icon và văn bản
        Color textColor = Enabled ? ForeColor : Color.FromArgb(100, 116, 139);
        if (_icon.HasValue && (_cachedIconBitmap == null || _cachedIconColor != textColor))
        {
            _cachedIconBitmap?.Dispose();
            _cachedIconColor = textColor;
            _cachedIconBitmap = _icon.Value.ToBitmap(textColor, _iconSize);
        }

        var textSize = TextRenderer.MeasureText(pevent.Graphics, Text, Font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        int gap = (_cachedIconBitmap != null && !string.IsNullOrEmpty(Text)) ? 7 : 0;
        int totalWidth = (_cachedIconBitmap?.Width ?? 0) + gap + textSize.Width;

        bool isLeftAligned = TextAlign is ContentAlignment.MiddleLeft
            or ContentAlignment.TopLeft
            or ContentAlignment.BottomLeft;
        int startX = isLeftAligned
            ? 16
            : Math.Max(8, (Width - totalWidth) / 2);

        if (_cachedIconBitmap != null)
        {
            int iconY = (Height - _cachedIconBitmap.Height) / 2;
            pevent.Graphics.DrawImage(_cachedIconBitmap, startX, iconY);
            startX += _cachedIconBitmap.Width + gap;
        }

        var textRect = new Rectangle(startX, 0, Math.Max(1, Width - startX - (_badgeCount > 0 ? 44 : 6)), Height);
        TextRenderer.DrawText(
            pevent.Graphics,
            Text,
            Font,
            textRect,
            textColor,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

        // 5. Huy hiệu số lượng (Badge)
        if (_badgeCount > 0)
        {
            string badgeText = _badgeCount > 99 ? "99+" : _badgeCount.ToString();
            var bSize = TextRenderer.MeasureText(pevent.Graphics, badgeText, AppTheme.CaptionBoldFont, Size.Empty, TextFormatFlags.NoPadding);
            int pillW = Math.Max(22, bSize.Width + 12);
            int pillH = 20;
            var pillRect = new Rectangle(Width - pillW - 12, (Height - pillH) / 2, pillW, pillH);
            using var pillPath = AppTheme.CreateRoundedRect(pillRect, pillH / 2);
            using var pillBrush = new SolidBrush(AppTheme.Accent);
            pevent.Graphics.FillPath(pillBrush, pillPath);

            TextRenderer.DrawText(
                pevent.Graphics,
                badgeText,
                AppTheme.CaptionBoldFont,
                pillRect,
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cachedIconBitmap?.Dispose();
        }
        base.Dispose(disposing);
    }
}
