using ClipboardHistoryMini.Data;
using ClipboardHistoryMini.Service;
using ClipboardHistoryMini.UIService;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ClipboardHistoryMini
{
    public partial class frmAbout : ModernFormBase
    {
        private TopBarDesignService _topBarService;
        private Panel mainPanel;
        private Panel headerPanel;
        private Panel contentPanel;
        private Panel footerPanel;

        public frmAbout()
        {
            //InitializeComponent();
            SetupForm();
            CreateModernAboutUI();
        }

        private void SetupForm()
        {
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(248, 249, 250);

            // Apply custom top bar following Windows 11 design principles
            _topBarService = new TopBarDesignService();
            _topBarService.ApplyTopBar(
                form: this,
                title: "About Clipboard History Mini",
                height: 50,
                backgroundColor: Color.FromArgb(99, 102, 241),
                titleColor: Color.White,
                showControlButtons: true
            );
        }

        private void CreateModernAboutUI()
        {
            // Main container panel with modern styling
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(0, 50, 0, 0) // Account for top bar
            };
            this.Controls.Add(mainPanel);

            CreateHeaderSection();
            CreateContentSection();
            CreateFooterSection();
        }

        private void CreateHeaderSection()
        {
            headerPanel = new Panel
            {
                Height = 180,
                Dock = DockStyle.Top,
                BackColor = Color.White,
                Padding = new Padding(40, 30, 40, 30)
            };

            // App icon with modern shadow effect
            PictureBox appIcon = new PictureBox
            {
                Size = new Size(80, 80),
                Location = new Point(40, 30),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            // Create a modern app icon placeholder
            Bitmap iconBitmap = CreateModernIcon();
            appIcon.Image = iconBitmap;

            // App name with modern typography
            Label appName = new Label
            {
                Text = "Clipboard History Mini",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(140, 35),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Version info with subtle styling
            Label versionLabel = new Label
            {
                Text = CommonData.strApplicationVersion,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(140, 75),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Developer info
            Label developerLabel = new Label
            {
                Text = "Developed by Zero Byte Software Solutions",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(140, 100),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            headerPanel.Controls.AddRange(new Control[] { appIcon, appName, versionLabel, developerLabel });
            mainPanel.Controls.Add(headerPanel);
        }

        private void CreateContentSection()
        {
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(40, 20, 40, 20)
            };

            // Description section with modern card design
            Panel descriptionCard = CreateModernCard("About This Application",
                "A lightweight clipboard manager for Windows — quickly access, search, and reuse your recent copied texts and images.\n\n" +
                "Perfect for developers, writers, designers, and anyone who copies and pastes all day. " +
                "With a simple, minimal design and system tray access, Clipboard History Mini keeps your workflow smooth and efficient.",
                0);

            // Features section
            Panel featuresCard = CreateModernCard("Key Features",
                "• View clipboard history in one clean, scrollable list\n" +
                "• Copy back any previous text or image instantly\n" +
                "• Pin important items so they're never lost\n" +
                "• Search and filter by keyword or content type\n" +
                "• 100% offline — privacy-first design\n" +
                "• Minimal memory and CPU usage",
                180);

            // System requirements
            Panel systemCard = CreateModernCard("System Requirements",
                "• Windows 10 or later\n" +
                "• .NET 8.0 Runtime\n" +
                "• 50 MB available disk space\n" +
                "• 128 MB RAM minimum",
                360);

            contentPanel.Controls.AddRange(new Control[] { descriptionCard, featuresCard, systemCard });
            mainPanel.Controls.Add(contentPanel);
        }

        private void CreateFooterSection()
        {
            footerPanel = new Panel
            {
                Height = 80,
                Dock = DockStyle.Bottom,
                BackColor = Color.White,
                Padding = new Padding(40, 20, 40, 20)
            };

            // Website link button with modern styling
            Button websiteButton = CreateModernButton("Visit Website", Color.FromArgb(99, 102, 241));
            websiteButton.Location = new Point(40, 20);
            websiteButton.Click += (s, e) => Process.Start(new ProcessStartInfo("https://zerobytebd.com/") { UseShellExecute = true });

            // Email contact button
            Button emailButton = CreateModernButton("Contact Support", Color.FromArgb(16, 185, 129));
            emailButton.Location = new Point(200, 20);
            emailButton.Click += (s, e) => Process.Start(new ProcessStartInfo("mailto:shahedbddev@gmail.com") { UseShellExecute = true });

            // Close button
            Button closeButton = CreateModernButton("Close", Color.FromArgb(107, 114, 128));
            closeButton.Location = new Point(360, 20);
            closeButton.Click += (s, e) => this.Close();

            // Copyright label
            Label copyrightLabel = new Label
            {
                Text = "© 2025 Zero Byte. All rights reserved.",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(107, 114, 128),
                Location = new Point(520, 30),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            footerPanel.Controls.AddRange(new Control[] { websiteButton, emailButton, closeButton, copyrightLabel });
            mainPanel.Controls.Add(footerPanel);
        }

        private Panel CreateModernCard(string title, string content, int yPosition)
        {
            Panel card = new Panel
            {
                Size = new Size(720, 150),
                Location = new Point(0, yPosition),
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            // Add subtle shadow effect
            card.Paint += (s, e) =>
            {
                Rectangle rect = new Rectangle(0, 0, card.Width, card.Height);
                using (GraphicsPath path = GetRoundedRectanglePath(rect, 8))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (SolidBrush brush = new SolidBrush(Color.White))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                }
            };

            Label titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(20, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            Label contentLabel = new Label
            {
                Text = content,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(20, 50),
                Size = new Size(680, 80),
                BackColor = Color.Transparent
            };

            card.Controls.AddRange(new Control[] { titleLabel, contentLabel });
            return card;
        }

        private Button CreateModernButton(string text, Color backgroundColor)
        {
            Button button = new Button
            {
                Text = text,
                Size = new Size(140, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = backgroundColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(backgroundColor, 0.1f);
            button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(backgroundColor, 0.1f);

            // Add rounded corners
            button.Paint += (s, e) =>
            {
                Rectangle rect = new Rectangle(0, 0, button.Width, button.Height);
                using (GraphicsPath path = GetRoundedRectanglePath(rect, 6))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (SolidBrush brush = new SolidBrush(button.BackColor))
                    {
                        e.Graphics.FillPath(brush, path);
                    }

                    TextRenderer.DrawText(e.Graphics, button.Text, button.Font, rect,
                        button.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            };

            return button;
        }

        private Bitmap CreateModernIcon()
        {
            Bitmap icon = new Bitmap(80, 80);
            using (Graphics g = Graphics.FromImage(icon))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Create gradient background
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new Rectangle(0, 0, 80, 80),
                    Color.FromArgb(99, 102, 241),
                    Color.FromArgb(139, 92, 246),
                    LinearGradientMode.Vertical))
                {
                    g.FillEllipse(brush, 0, 0, 80, 80);
                }

                // Draw clipboard icon
                using (Pen pen = new Pen(Color.White, 3))
                {
                    // Clipboard body
                    g.DrawRectangle(pen, 20, 25, 40, 45);
                    // Clipboard clip
                    g.DrawRectangle(pen, 30, 15, 20, 15);
                    // Lines representing text
                    g.DrawLine(pen, 25, 35, 55, 35);
                    g.DrawLine(pen, 25, 45, 50, 45);
                    g.DrawLine(pen, 25, 55, 45, 55);
                }
            }
            return icon;
        }

        private GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            return path;
        }
    }
}
