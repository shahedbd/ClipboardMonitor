namespace ClipboardHistoryMini.Service;

using System.Drawing;
using System.Windows.Forms;


    /// <summary>
    /// Lightweight service for customizing only the window top bar
    /// </summary>
    public class TopBarDesignService
    {
        private Form _form;
        private Panel _topPanel;
        private Label _titleLabel;
        private PictureBox _iconBox;
        private Button _closeButton;
        private Button _minimizeButton;
        private Button _maximizeButton;

        // Window dragging
        private bool _dragging = false;
        private Point _dragCursorPoint;
        private Point _dragFormPoint;

        /// <summary>
        /// Apply custom top bar to a form
        /// </summary>
        /// <param name="form">The form to apply top bar to</param>
        /// <param name="title">Title text</param>
        /// <param name="height">Top bar height (default: 50)</param>
        /// <param name="backgroundColor">Background color (default: Indigo)</param>
        /// <param name="titleColor">Title text color (default: White)</param>
        /// <param name="icon">Window icon (optional)</param>
        /// <param name="showControlButtons">Show minimize/maximize/close buttons (default: true)</param>
        public void ApplyTopBar(
            Form form,
            string title,
            int height = 50,
            Color? backgroundColor = null,
            Color? titleColor = null,
            Icon icon = null,
            bool showControlButtons = true)
        {
            _form = form;

            // Default colors
            Color bgColor = backgroundColor ?? Color.FromArgb(99, 102, 241);
            Color txtColor = titleColor ?? Color.White;

            // Remove default border
            _form.FormBorderStyle = FormBorderStyle.None;

            // Create top bar panel
            _topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = height,
                BackColor = bgColor
            };

            int leftPosition = 15;

            // Add icon if provided
            if (icon != null)
            {
                _iconBox = new PictureBox
                {
                    Image = icon.ToBitmap(),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Size = new Size(20, 20),
                    Location = new Point(15, (height - 20) / 2)
                };
                _topPanel.Controls.Add(_iconBox);
                leftPosition = 45;
            }

            // Add title
            _titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = txtColor,
                AutoSize = true,
                Location = new Point(leftPosition, (height - 20) / 2)
            };
            _topPanel.Controls.Add(_titleLabel);

            // Add control buttons if requested
            if (showControlButtons)
            {
                CreateControlButtons(height, bgColor);
            }

            // Add to form
            _form.Controls.Add(_topPanel);

            // Make draggable
            MakeTopBarDraggable();
        }

        /// <summary>
        /// Get the top bar panel to add custom controls
        /// </summary>
        public Panel GetTopBarPanel()
        {
            return _topPanel;
        }

        /// <summary>
        /// Update the title text dynamically
        /// </summary>
        public void UpdateTitle(string newTitle)
        {
            if (_titleLabel != null)
            {
                _titleLabel.Text = newTitle;
            }
        }

        /// <summary>
        /// Update the icon dynamically
        /// </summary>
        public void UpdateIcon(Icon newIcon)
        {
            if (_iconBox != null && newIcon != null)
            {
                _iconBox.Image = newIcon.ToBitmap();
            }
        }

        private void CreateControlButtons(int barHeight, Color bgColor)
        {
            // Close button
            _closeButton = CreateButton("×", Color.FromArgb(239, 68, 68));
            _closeButton.Location = new Point(_form.Width - 50, (barHeight - 35) / 2);
            _closeButton.Click += (s, e) => _form.Close();

            // Maximize button
            _maximizeButton = CreateButton("□", GetContrastColor(bgColor));
            _maximizeButton.Location = new Point(_form.Width - 95, (barHeight - 35) / 2);
            _maximizeButton.Click += (s, e) =>
            {
                _form.WindowState = _form.WindowState == FormWindowState.Maximized
                    ? FormWindowState.Normal
                    : FormWindowState.Maximized;
            };

            // Minimize button
            _minimizeButton = CreateButton("−", GetContrastColor(bgColor));
            _minimizeButton.Location = new Point(_form.Width - 140, (barHeight - 35) / 2);
            _minimizeButton.Click += (s, e) => _form.WindowState = FormWindowState.Minimized;

            _topPanel.Controls.AddRange(new Control[] { _closeButton, _maximizeButton, _minimizeButton });

            // Reposition on resize
            _form.Resize += (s, e) =>
            {
                _closeButton.Location = new Point(_form.Width - 50, (barHeight - 35) / 2);
                _maximizeButton.Location = new Point(_form.Width - 95, (barHeight - 35) / 2);
                _minimizeButton.Location = new Point(_form.Width - 140, (barHeight - 35) / 2);
            };
        }

        private Button CreateButton(string text, Color hoverColor)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(35, 35),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 220, 220),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 255, 255, 255);

            btn.MouseEnter += (s, e) => btn.ForeColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.ForeColor = Color.FromArgb(220, 220, 220);

            return btn;
        }

        private void MakeTopBarDraggable()
        {
            _topPanel.MouseDown += TopPanel_MouseDown;
            _topPanel.MouseMove += TopPanel_MouseMove;
            _topPanel.MouseUp += TopPanel_MouseUp;

            if (_titleLabel != null)
            {
                _titleLabel.MouseDown += TopPanel_MouseDown;
                _titleLabel.MouseMove += TopPanel_MouseMove;
                _titleLabel.MouseUp += TopPanel_MouseUp;
            }

            if (_iconBox != null)
            {
                _iconBox.MouseDown += TopPanel_MouseDown;
                _iconBox.MouseMove += TopPanel_MouseMove;
                _iconBox.MouseUp += TopPanel_MouseUp;
            }
        }

        private void TopPanel_MouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;
            _dragCursorPoint = Cursor.Position;
            _dragFormPoint = _form.Location;
        }

        private void TopPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(_dragCursorPoint));
                _form.Location = Point.Add(_dragFormPoint, new Size(diff));
            }
        }

        private void TopPanel_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private Color GetContrastColor(Color bgColor)
        {
            double brightness = (0.299 * bgColor.R + 0.587 * bgColor.G + 0.114 * bgColor.B) / 255;
            return brightness > 0.5 ? Color.FromArgb(60, 60, 60) : Color.White;
        }
    }

    // ============================================
    // EXAMPLE USAGE
    // ============================================

    // Base form with double buffering
    public class TopBarFormBase : Form
    {
        public TopBarFormBase()
        {
            this.DoubleBuffered = true;
        }
    }

    // Example 1: Simple top bar
    public class SimpleForm : TopBarFormBase
    {
        private TopBarDesignService _topBarService;

        public SimpleForm()
        {
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Apply top bar only
            _topBarService = new TopBarDesignService();
            _topBarService.ApplyTopBar(
                form: this,
                title: "Simple Window",
                height: 50,
                backgroundColor: Color.FromArgb(99, 102, 241),
                titleColor: Color.White
            );

            // Add your content directly to the form
            var lblContent = new Label
            {
                Text = "Your content here",
                Font = new Font("Segoe UI", 16),
                AutoSize = true,
                Location = new Point(50, 100)
            };
            this.Controls.Add(lblContent);
        }
    }

    // Example 2: Custom styled top bar
    public class CustomForm : TopBarFormBase
    {
        private TopBarDesignService _topBarService;

        public CustomForm()
        {
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Icon = SystemIcons.Application; // Set your icon

            // Apply custom colored top bar
            _topBarService = new TopBarDesignService();
            _topBarService.ApplyTopBar(
                form: this,
                title: "Custom Style Window",
                height: 60,
                backgroundColor: Color.FromArgb(17, 24, 39), // Dark
                titleColor: Color.White,
                icon: this.Icon
            );

            // Add custom button to the top bar
            var topBar = _topBarService.GetTopBarPanel();
            var btnCustom = new Button
            {
                Text = "Settings",
                Location = new Point(topBar.Width - 250, 15),
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnCustom.FlatAppearance.BorderSize = 0;
            topBar.Controls.Add(btnCustom);

            // Reposition on resize
            this.Resize += (s, e) =>
            {
                btnCustom.Location = new Point(topBar.Width - 250, 15);
            };
        }
    }

    // Example 3: Top bar without control buttons
    public class MinimalForm : TopBarFormBase
    {
        public MinimalForm()
        {
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            var topBarService = new TopBarDesignService();
            topBarService.ApplyTopBar(
                form: this,
                title: "Minimal - No Controls",
                height: 45,
                backgroundColor: Color.FromArgb(139, 92, 246),
                showControlButtons: false // No min/max/close buttons
            );

            // Add your own close button
            var btnClose = new Button
            {
                Text = "Close",
                Location = new Point(250, 150),
                Size = new Size(100, 40)
            };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }
    }

