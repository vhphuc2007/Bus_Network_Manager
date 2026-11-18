using System.Drawing.Drawing2D;
using BusBooking.Client.Helpers;
using BusBooking.Client.Theme;
using FontAwesome.Sharp;

namespace BusBooking.Client.Controls;

public class DoubleBufferedPanel : Panel
{
    public DoubleBufferedPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
    }
}

public class DoubleBufferedFlowLayoutPanel : FlowLayoutPanel
{
    public DoubleBufferedFlowLayoutPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
    }
}

/// <summary>
/// Thẻ trắng bo góc 12px chuẩn SaaS kèm viền mảnh #E2E8F0 và tùy chọn dải màu nhấn bên trái.
/// Kế thừa RoundedPanel chuẩn GDI+ với hỗ trợ cắt Region và chống lộ 4 góc vuông nền.
/// </summary>
public class RoundedCardPanel : RoundedPanel
{
}

/// <summary>
/// Nút bấm bo góc hiện đại có hover mượt, hỗ trợ icon FontAwesome.Sharp và trạng thái Loading.
/// Kế thừa RoundedButton chuẩn GDI+ với hỗ trợ cắt Region và chống lộ 4 góc vuông nền.
/// </summary>
public class ModernButton : RoundedButton
{
}

/// <summary>
/// Ô nhập liệu bo góc hiện đại kèm icon bên trái và hiệu ứng viền xanh khi focus.
/// Kế thừa RoundedTextBox chuẩn GDI+.
/// </summary>
public class ModernTextBox : RoundedTextBox
{
}

/// <summary>
/// Nhãn badge bo tròn theo màu ngữ nghĩa (trạng thái vé, ghế trống, kênh bán).
/// </summary>
public class BadgeLabel : Control
{
    private Color _badgeBackColor = AppTheme.InfoBg;

    public Color BadgeBackColor
    {
        get => _badgeBackColor;
        set { _badgeBackColor = value; Invalidate(); }
    }

    public BadgeLabel()
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
        ForeColor = AppTheme.InfoFg;
        Font = AppTheme.CaptionBoldFont;
        Size = new Size(110, 26);
    }

    public void SetSemanticStatus(string statusText)
    {
        Text = statusText;
        var (back, fore) = AppTheme.GetStatusColors(statusText);
        _badgeBackColor = back;
        ForeColor = fore;
        UpdatePreferredWidth();
        Invalidate();
    }

    public void SetColors(string text, Color back, Color fore)
    {
        Text = text;
        _badgeBackColor = back;
        ForeColor = fore;
        UpdatePreferredWidth();
        Invalidate();
    }

    private void UpdatePreferredWidth()
    {
        var size = TextRenderer.MeasureText(Text, Font);
        Width = Math.Max(76, size.Width + 20);
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
        var rect = new Rectangle(0, 0, Width, Height);
        using var path = AppTheme.CreateRoundedRect(rect, Height / 2);
        Region?.Dispose();
        Region = new Region(path);
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
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

        var rect = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        using var path = AppTheme.CreateRoundedRect(rect, Height / 2);
        using var brush = new SolidBrush(_badgeBackColor);
        e.Graphics.FillPath(brush, path);

        using var smoothPen = new Pen(parentBg, 1f);
        e.Graphics.DrawPath(smoothPen, path);

        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            rect,
            ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
    }
}

/// <summary>
/// Thẻ KPI bo góc kèm dải màu nhấn bên trái và icon FontAwesome.Sharp.
/// </summary>
public class KpiCard : RoundedCardPanel
{
    private readonly Label _lblTitle;
    private readonly Label _lblValue;
    private readonly Label _lblSub;
    private readonly IconPictureBox _iconBox;
    private readonly RoundedCardPanel _iconWrap;

    public KpiCard(string title, string value, string subtitle, IconChar icon, Color accentColor)
    {
        LeftAccentColor = accentColor;
        Size = new Size(255, 112);
        Padding = new Padding(18, 14, 16, 12);

        var pastel = AppTheme.Tint(accentColor, 0.88);
        _iconWrap = new RoundedCardPanel
        {
            Size = new Size(42, 42),
            CornerRadius = 10,
            SurfaceColor = pastel,
            BorderThickness = 0,
            Padding = new Padding(6)
        };
        _iconBox = new IconPictureBox
        {
            Dock = DockStyle.Fill,
            IconChar = icon,
            IconColor = accentColor,
            IconSize = 20,
            BackColor = Color.Transparent,
            SizeMode = PictureBoxSizeMode.CenterImage
        };
        _iconWrap.Controls.Add(_iconBox);

        _lblTitle = new Label
        {
            Text = title,
            Font = AppTheme.CaptionBoldFont,
            ForeColor = AppTheme.TextMuted,
            BackColor = AppTheme.CardSurface,
            UseMnemonic = false,
            AutoSize = true,
            Location = new Point(18, 14)
        };

        _lblValue = new Label
        {
            Text = value,
            Font = AppTheme.DisplayFont,
            ForeColor = AppTheme.TextPrimary,
            BackColor = AppTheme.CardSurface,
            UseMnemonic = false,
            AutoSize = false,
            AutoEllipsis = true,
            Size = new Size(185, 38),
            Location = new Point(18, 38)
        };

        _lblSub = new Label
        {
            Text = subtitle,
            Font = AppTheme.CaptionFont,
            ForeColor = AppTheme.TextMuted,
            BackColor = AppTheme.CardSurface,
            UseMnemonic = false,
            AutoSize = false,
            AutoEllipsis = true,
            Size = new Size(210, 22),
            Location = new Point(18, 80)
        };

        Controls.Add(_lblTitle);
        Controls.Add(_lblValue);
        Controls.Add(_lblSub);
        Controls.Add(_iconWrap);

        Resize += (_, _) => PositionIcon();
        PositionIcon();
    }

    private void PositionIcon()
    {
        _iconWrap.Location = new Point(Math.Max(140, Width - _iconWrap.Width - 16), 14);
        _lblValue.Width = Math.Max(100, _iconWrap.Left - 24);
        _lblSub.Width = Math.Max(120, Width - 34);
    }

    public void UpdateMetric(string value, string? subtitle = null)
    {
        _lblValue.Text = value;
        if (subtitle != null)
            _lblSub.Text = subtitle;
    }
}
