using System;
using System.Drawing;
using System.Windows.Forms;
using ClipboardHistoryMini.Models;

namespace ClipboardHistoryMini.Controls
{
    public class ClipboardItemControl : Panel
    {
        private ClipboardItem _item;
        private Label _typeLabel;
        private Label _previewLabel;
        private Label _timeLabel;
        private Label _pinLabel;
        private PictureBox _thumbnailBox;

        public ClipboardItem Item
        {
            get => _item;
            set
            {
                _item = value;
                UpdateDisplay();
            }
        }

        public event EventHandler ItemClicked;
        public event EventHandler PinToggled;
        public event EventHandler DeleteRequested;

        public ClipboardItemControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Height = 80;
            this.Dock = DockStyle.Top;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Padding = new Padding(10);
            this.Cursor = Cursors.Hand;
            this.BackColor = Color.White;

            // Thumbnail/Icon
            _thumbnailBox = new PictureBox
            {
                Size = new Size(60, 60),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(_thumbnailBox);

            // Type label
            _typeLabel = new Label
            {
                Location = new Point(80, 10),
                Size = new Size(50, 20),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.Gray
            };
            this.Controls.Add(_typeLabel);

            // Preview label
            _previewLabel = new Label
            {
                Location = new Point(80, 30),
                Size = new Size(400, 40),
                Font = new Font("Segoe UI", 9F),
                AutoEllipsis = true
            };
            this.Controls.Add(_previewLabel);

            // Time label
            _timeLabel = new Label
            {
                Location = new Point(500, 10),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.TopRight
            };
            this.Controls.Add(_timeLabel);

            // Pin label
            _pinLabel = new Label
            {
                Location = new Point(500, 40),
                Size = new Size(30, 30),
                Font = new Font("Segoe UI", 14F),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            _pinLabel.Click += (s, e) =>
            {
                PinToggled?.Invoke(this, EventArgs.Empty);
            };
            this.Controls.Add(_pinLabel);

            // Delete button
            var btnDelete = new Button
            {
                Location = new Point(540, 40),
                Size = new Size(50, 30),
                Text = "✕",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.Red,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += (s, e) =>
            {
                DeleteRequested?.Invoke(this, EventArgs.Empty);
            };
            this.Controls.Add(btnDelete);

            // Hover effects
            this.MouseEnter += (s, e) => this.BackColor = Color.FromArgb(240, 240, 240);
            this.MouseLeave += (s, e) => this.BackColor = Color.White;
            this.Click += (s, e) => ItemClicked?.Invoke(this, e);
        }

        private void UpdateDisplay()
        {
            if (_item == null) return;

            // Type indicator
            _typeLabel.Text = _item.Type switch
            {
                ClipboardItemType.Image => "IMAGE",
                ClipboardItemType.RichText => "RICH TEXT",
                _ => "TEXT"
            };

            // Preview
            _previewLabel.Text = _item.GetPreview(100);

            // Timestamp
            var timeAgo = GetTimeAgo(_item.CopiedAt);
            _timeLabel.Text = timeAgo;

            // Pin indicator
            _pinLabel.Text = _item.IsPinned ? "📌" : "";

            // Thumbnail for images
            if (_item.Type == ClipboardItemType.Image)
            {
                var thumb = _item.GetThumbnail(60, 60);
                if (thumb != null)
                {
                    _thumbnailBox.Image = thumb;
                }
            }
            else
            {
                // Text icon
                _thumbnailBox.Image = CreateTextIcon();
            }
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var span = DateTime.Now - dateTime;

            if (span.TotalMinutes < 1)
                return "Just now";
            if (span.TotalMinutes < 60)
                return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24)
                return $"{(int)span.TotalHours}h ago";
            if (span.TotalDays < 7)
                return $"{(int)span.TotalDays}d ago";

            return dateTime.ToString("MM/dd/yyyy");
        }

        private Image CreateTextIcon()
        {
            var bmp = new Bitmap(60, 60);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(230, 230, 250));
                g.DrawString("📄", new Font("Segoe UI Emoji", 24F),
                    Brushes.DarkSlateBlue, new PointF(10, 10));
            }
            return bmp;
        }
    }
}