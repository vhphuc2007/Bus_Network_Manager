using System.ComponentModel;
using BusBooking.Client.Theme;
using FontAwesome.Sharp;

namespace BusBooking.Client.Controls;

/// <summary>
/// Ô nhập liệu bo góc hiện đại chuẩn WinForms GDI+ kế thừa RoundedPanel,
/// hỗ trợ Icon FontAwesome, hiệu ứng viền khi Focus và tương thích Visual Studio Designer.
/// </summary>
public class RoundedTextBox : RoundedPanel
{
    private readonly TextBox _inner;
    private readonly IconPictureBox _iconBox;

    public RoundedTextBox()
    {
        BorderRadius = 10;
        BorderColor = Color.FromArgb(226, 232, 240);
        FillColor = Color.White;
        Padding = new Padding(12, 6, 12, 6);
        Size = new Size(260, 42);
        Cursor = Cursors.IBeam;

        _iconBox = new IconPictureBox
        {
            Dock = DockStyle.Left,
            Width = 26,
            IconSize = 16,
            IconColor = AppTheme.TextMuted,
            BackColor = Color.Transparent,
            Visible = false,
            SizeMode = PictureBoxSizeMode.CenterImage,
            Cursor = Cursors.IBeam
        };

        var innerHolder = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(4, 5, 2, 0),
            Cursor = Cursors.IBeam
        };

        _inner = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            Font = AppTheme.BodyFont,
            ForeColor = AppTheme.TextPrimary,
            BackColor = Color.White
        };

        _inner.GotFocus += (_, _) =>
        {
            BorderColor = AppTheme.Primary;
            BorderSize = 2;
            _iconBox.IconColor = AppTheme.Primary;
        };
        _inner.LostFocus += (_, _) =>
        {
            BorderColor = Color.FromArgb(226, 232, 240);
            BorderSize = 1;
            _iconBox.IconColor = AppTheme.TextMuted;
        };
        _inner.TextChanged += (s, e) => TextChanged?.Invoke(this, e);
        _inner.KeyDown += (s, e) => KeyDown?.Invoke(this, e);

        Click += (_, _) => _inner.Focus();
        innerHolder.Click += (_, _) => _inner.Focus();
        _iconBox.Click += (_, _) => _inner.Focus();

        innerHolder.Controls.Add(_inner);
        Controls.Add(innerHolder);
        Controls.Add(_iconBox);
    }

    public new event EventHandler? TextChanged;
    public new event KeyEventHandler? KeyDown;

    [Category("Appearance")]
    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override string Text
    {
        get => _inner.Text;
        set => _inner.Text = value ?? string.Empty;
    }

    [Category("Appearance")]
    [DefaultValue("")]
    [Description("Văn bản gợi ý khi chưa nhập dữ liệu.")]
    public string PlaceholderText
    {
        get => _inner.PlaceholderText;
        set => _inner.PlaceholderText = value;
    }

    [Category("Behavior")]
    [DefaultValue(false)]
    [Description("Bật ẩn ký tự mật khẩu.")]
    public bool UseSystemPasswordChar
    {
        get => _inner.UseSystemPasswordChar;
        set => _inner.UseSystemPasswordChar = value;
    }

    [Category("Behavior")]
    [DefaultValue(false)]
    [Description("Chỉ cho phép đọc, không cho sửa nội dung.")]
    public bool ReadOnly
    {
        get => _inner.ReadOnly;
        set
        {
            _inner.ReadOnly = value;
            FillColor = value ? AppTheme.SubtleBg : Color.White;
            _inner.BackColor = FillColor;
            _iconBox.BackColor = Color.Transparent;
            if (_inner.Parent != null) _inner.Parent.BackColor = FillColor;
        }
    }

    [Category("Appearance")]
    [Description("Icon FontAwesome ở đầu ô nhập liệu.")]
    public IconChar? Icon
    {
        get => _iconBox.Visible ? _iconBox.IconChar : null;
        set
        {
            if (value.HasValue)
            {
                _iconBox.IconChar = value.Value;
                _iconBox.Visible = true;
            }
            else
            {
                _iconBox.Visible = false;
            }
        }
    }

    public void FocusInput() => _inner.Focus();
}
