using ClipboardHistoryMini.Models;
using ClipboardHistoryMini.Services;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ClipboardHistoryMini
{
    public partial class MainForm : Form
    {
        private ClipboardMonitor _monitor;
        private StorageService _storage;
        private NotifyIcon _trayIcon;
        private TextBox _searchBox;
        private ListView _historyList;
        private ContextMenuStrip _itemContextMenu;
        private Timer _searchDebounceTimer;
        private bool _isSearching = false;
        private HotkeyManager _hotkeyManager;

        public MainForm()
        {
            InitializeComponent();
            InitializeServices();
            SetupUI();
            SetupTrayIcon();
            LoadHistory();

            // Add sample data
            //LoadSampleData();
        }


        private void InitializeServices()
        {
            _storage = new StorageService();
            _monitor = new ClipboardMonitor();
            _monitor.ClipboardChanged += OnClipboardChanged;
            _monitor.SetTrackImages(_storage.GetSettings().TrackImages);
            _monitor.StartMonitoring();

            _searchDebounceTimer = new Timer { Interval = 300 };
            _searchDebounceTimer.Tick += SearchDebounceTimer_Tick;

            // Setup global hotkey (Ctrl+Shift+V by default)
            _hotkeyManager = new HotkeyManager(this.Handle);
            _hotkeyManager.HotkeyPressed += (s, e) => ShowMainWindow();
            _hotkeyManager.RegisterHotkey(Keys.V, ctrl: true, shift: true);
        }



        private void SetupUI()
        {
            this.SuspendLayout();
            this.Controls.Clear();

            // Create all panels first
            var topToolbar = new Panel
            {
                Name = "topToolbar",
                Height = 40,
                Dock = DockStyle.Top,
                Padding = new Padding(10, 5, 10, 5),
                BackColor = Color.FromArgb(240, 240, 240)
            };

            var bottomToolbar = new Panel
            {
                Name = "bottomToolbar",
                Height = 40,
                Dock = DockStyle.Bottom,
                Padding = new Padding(10, 5, 10, 5),
                BackColor = Color.FromArgb(240, 240, 240)
            };

            _historyList = new ListView
            {
                Name = "historyList",
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9F),
                MultiSelect = false,
                HeaderStyle = ColumnHeaderStyle.Clickable,
                BorderStyle = BorderStyle.FixedSingle,
                ShowGroups = false,
                BackColor = Color.White
            };

            // CRITICAL: Add controls in this specific order
            //this.Controls.Add(bottomToolbar);  // Bottom first
            //this.Controls.Add(topToolbar);     // Top second  
            //this.Controls.Add(_historyList);   // Fill last

            this.Controls.Add(_historyList);   // Fill first
            this.Controls.Add(topToolbar);     // Top second  
            this.Controls.Add(bottomToolbar);  // Bottom last

            // Setup ListView columns
            _historyList.Columns.Add("Type", 60);
            _historyList.Columns.Add("Content", 300);
            _historyList.Columns.Add("Time", 120);
            _historyList.Columns.Add("Pin", 40);

            // Setup toolbars
            SetupTopToolbar(topToolbar);
            SetupBottomToolbar(bottomToolbar);

            // Context menu
            _itemContextMenu = new ContextMenuStrip();
            _itemContextMenu.Items.Add("Copy", null, ContextMenu_Copy);
            _itemContextMenu.Items.Add("Pin/Unpin", null, ContextMenu_TogglePin);
            _itemContextMenu.Items.Add("Delete", null, ContextMenu_Delete);
            _historyList.ContextMenuStrip = _itemContextMenu;

            // Event handlers
            _historyList.DoubleClick += HistoryList_DoubleClick;
            _historyList.KeyDown += HistoryList_KeyDown;

            this.ResumeLayout(true);
        }


        private void SetupTopToolbar(Panel topToolbar)
        {
            var btnAll = new Button
            {
                Text = "All",
                Width = 60,
                Height = 28,
                Location = new Point(5, 6),
                BackColor = SystemColors.ButtonHighlight,
                FlatStyle = FlatStyle.System
            };
            btnAll.Click += (s, e) => FilterHistory(null);
            topToolbar.Controls.Add(btnAll);

            var btnText = new Button
            {
                Text = "Text",
                Width = 60,
                Height = 28,
                Location = new Point(75, 6),
                BackColor = SystemColors.Control,
                FlatStyle = FlatStyle.System
            };
            btnText.Click += (s, e) => FilterHistory(ClipboardItemType.Text);
            topToolbar.Controls.Add(btnText);

            var btnImages = new Button
            {
                Text = "Images",
                Width = 70,
                Height = 28,
                Location = new Point(145, 6),
                BackColor = SystemColors.Control,
                FlatStyle = FlatStyle.System
            };
            btnImages.Click += (s, e) => FilterHistory(ClipboardItemType.Image);
            topToolbar.Controls.Add(btnImages);

            _searchBox = new TextBox
            {
                Height = 23,
                Location = new Point(225, 8),
                Width = 200,
                Font = new Font("Segoe UI", 9F),
                PlaceholderText = "Search clipboard history..."
            };
            _searchBox.TextChanged += SearchBox_TextChanged;
            topToolbar.Controls.Add(_searchBox);

            topToolbar.Resize += (s, e) => {
                if (_searchBox != null)
                {
                    _searchBox.Width = Math.Max(150, topToolbar.Width - 240);
                }
            };
        }

        private void SetupBottomToolbar(Panel bottomToolbar)
        {
            //var btnImport = new Button
            //{
            //    Text = "Import",
            //    Width = 70,
            //    Height = 28,
            //    Location = new Point(5, 6),
            //    FlatStyle = FlatStyle.System
            //};
            //bottomToolbar.Controls.Add(btnImport);

            //var btnExport = new Button
            //{
            //    Text = "Export",
            //    Width = 70,
            //    Height = 28,
            //    Location = new Point(85, 6),
            //    FlatStyle = FlatStyle.System
            //};
            //bottomToolbar.Controls.Add(btnExport);

            var btnStats = new Button
            {
                Text = "Stats",
                Width = 70,
                Height = 28,
                Location = new Point(5, 6),
                FlatStyle = FlatStyle.System,
            };
            bottomToolbar.Controls.Add(btnStats);
            btnStats.Click += BtnStats_Click;

            var btnSettings = new Button
            {
                Text = "Settings",
                Width = 80,
                Height = 28,
                Location = new Point(85, 6),
                FlatStyle = FlatStyle.System
            };
            btnSettings.Click += BtnSettings_Click;
            bottomToolbar.Controls.Add(btnSettings);

            var lblCount = new Label
            {
                Name = "lblCount",
                Text = "4 items",
                TextAlign = ContentAlignment.MiddleCenter,
                Width = 80,
                Height = 28,
                Location = new Point(350, 6),
                Font = new Font("Segoe UI", 9F)
            };
            bottomToolbar.Controls.Add(lblCount);

            var btnClear = new Button
            {
                Text = "Clear History",
                Width = 100,
                Height = 28,
                Location = new Point(450, 6),
                FlatStyle = FlatStyle.System
            };
            btnClear.Click += BtnClear_Click;
            bottomToolbar.Controls.Add(btnClear);

            bottomToolbar.Resize += (s, e) => {
                if (lblCount != null && btnClear != null)
                {
                    btnClear.Location = new Point(bottomToolbar.Width - 110, 6);
                    lblCount.Location = new Point(bottomToolbar.Width - 200, 6);
                }
            };
        }





        private void SetupTrayIcon()
        {
            var iconPath = Path.Combine(Application.StartupPath, "Resources", "favicon.ico");
            _trayIcon = new NotifyIcon
            {
                //Icon = SystemIcons.Application,
                Icon = new Icon(iconPath),
                Text = "Clipboard History Mini",
                Visible = true
            };

            var trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show", null, (s, e) => ShowMainWindow());
            trayMenu.Items.Add("Settings", null, BtnSettings_Click);
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("Exit", null, (s, e) => ExitApplication());

            _trayIcon.ContextMenuStrip = trayMenu;
            _trayIcon.DoubleClick += (s, e) => ShowMainWindow();
        }

        private void OnClipboardChanged(object sender, ClipboardItem item)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, ClipboardItem>(OnClipboardChanged), sender, item);
                return;
            }

            _storage.AddItem(item);

            if (!_isSearching)
                LoadHistory();

            if (_storage.GetSettings().ShowNotifications)
            {
                _trayIcon.ShowBalloonTip(1000, "Clipboard Updated",
                    item.GetPreview(50), ToolTipIcon.Info);
            }
        }

        private void LoadHistory()
        {
            _historyList.Items.Clear();
            var items = _storage.GetHistory();

            foreach (var item in items)
            {
                var lvi = new ListViewItem(new[]
                {
                    GetTypeIcon(item.Type),
                    item.GetPreview(80),
                    item.CopiedAt.ToString("MM/dd HH:mm"),
                    item.IsPinned ? "📌" : ""
                })
                {
                    Tag = item
                };

                _historyList.Items.Add(lvi);
            }

            UpdateItemCount();
        }

        private string GetTypeIcon(ClipboardItemType type)
        {
            return type switch
            {
                ClipboardItemType.Image => "🖼️",
                ClipboardItemType.RichText => "📝",
                _ => "📄"
            };
        }

        private void FilterHistory(ClipboardItemType? type)
        {
            _isSearching = true;
            _historyList.Items.Clear();

            var items = _storage.SearchHistory(_searchBox.Text, type);

            foreach (var item in items)
            {
                var lvi = new ListViewItem(new[]
                {
                    GetTypeIcon(item.Type),
                    item.GetPreview(80),
                    item.CopiedAt.ToString("MM/dd HH:mm"),
                    item.IsPinned ? "📌" : ""
                })
                {
                    Tag = item
                };

                _historyList.Items.Add(lvi);
            }

            UpdateItemCount();
            _isSearching = false;
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        }

        private void SearchDebounceTimer_Tick(object sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            FilterHistory(null);
        }

        private void HistoryList_DoubleClick(object sender, EventArgs e)
        {
            CopySelectedItem();
        }

        private void HistoryList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CopySelectedItem();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedItem();
                e.Handled = true;
            }
        }

        private void CopySelectedItem()
        {
            if (_historyList.SelectedItems.Count > 0)
            {
                var item = _historyList.SelectedItems[0].Tag as ClipboardItem;
                if (item != null)
                {
                    if (item.Type == ClipboardItemType.Image && item.ImageData != null)
                    {
                        using (var ms = new System.IO.MemoryStream(item.ImageData))
                        {
                            var img = Image.FromStream(ms);
                            Clipboard.SetImage(img);
                        }
                    }
                    else
                    {
                        Clipboard.SetText(item.Content);
                    }

                    _trayIcon.ShowBalloonTip(1000, "Copied", "Item copied to clipboard", ToolTipIcon.Info);
                }
            }
        }

        private void DeleteSelectedItem()
        {
            if (_historyList.SelectedItems.Count > 0)
            {
                var item = _historyList.SelectedItems[0].Tag as ClipboardItem;
                if (item != null)
                {
                    _storage.RemoveItem(item.Id);
                    LoadHistory();
                }
            }
        }

        private void ContextMenu_Copy(object sender, EventArgs e)
        {
            CopySelectedItem();
        }

        private void ContextMenu_TogglePin(object sender, EventArgs e)
        {
            if (_historyList.SelectedItems.Count > 0)
            {
                var item = _historyList.SelectedItems[0].Tag as ClipboardItem;
                if (item != null)
                {
                    _storage.TogglePin(item.Id);
                    LoadHistory();
                }
            }
        }

        private void ContextMenu_Delete(object sender, EventArgs e)
        {
            DeleteSelectedItem();
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            var settingsForm = new SettingsForm(_storage);
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                _monitor.SetTrackImages(_storage.GetSettings().TrackImages);
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Clear all non-pinned items from history?",
                "Clear History",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _storage.ClearHistory();
                LoadHistory();
            }
        }

        private void UpdateItemCount()
        {
            var lbl = this.Controls.Find("lblCount", true).FirstOrDefault() as Label;
            if (lbl != null)
            {
                lbl.Text = $"{_historyList.Items.Count} items";
            }
        }

        private void ShowMainWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void ExitApplication()
        {
            _monitor?.StopMonitoring();
            _trayIcon.Visible = false;
            Application.Exit();
        }

        protected override void WndProc(ref Message m)
        {
            if (_hotkeyManager?.ProcessHotkey(ref m) == true)
            {
                return;
            }
            base.WndProc(ref m);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            base.OnFormClosing(e);
        }

        private void BtnStats_Click(object sender, EventArgs e)
        {
            var items = _storage.GetHistory();
            int total = items.Count();
            int pinned = items.Count(i => i.IsPinned);
            int textItems = items.Count(i => i.Type == ClipboardItemType.Text);
            int imageItems = items.Count(i => i.Type == ClipboardItemType.Image);
            int richTextItems = items.Count(i => i.Type == ClipboardItemType.RichText);

            var mostRecent = items.OrderByDescending(i => i.CopiedAt).FirstOrDefault();

            string stats =
                $"Clipboard Stats:\n\n" +
                $"Total items: {total}\n" +
                $"Pinned items: {pinned}\n" +
                $"Text items: {textItems}\n" +
                $"Image items: {imageItems}\n" +
                $"RichText items: {richTextItems}\n" +
                (mostRecent != null ? $"Most recent: {mostRecent.CopiedAt:MM/dd HH:mm}" : "");

            MessageBox.Show(stats, "Clipboard Statistics", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadSampleData()
        {
            // Clear existing items
            _historyList.Items.Clear();

            // Add some sample clipboard items
            var items = new[]
            {
                new { Type = "Text", Content = "Hello, this is a sample text from clipboard", Time = DateTime.Now.AddMinutes(-5), Pinned = false },
                new { Type = "Text", Content = "https://github.com/user/repository", Time = DateTime.Now.AddMinutes(-10), Pinned = true },
                new { Type = "Image", Content = "Screenshot_2024.png (1920x1080)", Time = DateTime.Now.AddMinutes(-15), Pinned = false },
                new { Type = "Text", Content = "public class ClipboardManager { }", Time = DateTime.Now.AddMinutes(-20), Pinned = false },
                new { Type = "Text", Content = "Meeting notes: Discuss project timeline...", Time = DateTime.Now.AddMinutes(-25), Pinned = true },
                new { Type = "Image", Content = "diagram.jpg (800x600)", Time = DateTime.Now.AddMinutes(-30), Pinned = false }
            };

            foreach (var item in items)
            {
                var listItem = new ListViewItem(item.Type);
                listItem.SubItems.Add(item.Content.Length > 50 ? item.Content.Substring(0, 47) + "..." : item.Content);
                listItem.SubItems.Add(item.Time.ToString("HH:mm:ss"));
                listItem.SubItems.Add(item.Pinned ? "📌" : "");

                // Set different icons/colors for different types
                if (item.Type == "Image")
                {
                    listItem.BackColor = Color.FromArgb(240, 248, 255); // Light blue for images
                }
                else if (item.Pinned)
                {
                    listItem.BackColor = Color.FromArgb(255, 255, 240); // Light yellow for pinned items
                }

                listItem.Tag = item; // Store the original data
                _historyList.Items.Add(listItem);
            }

            // Update the count label
            UpdateItemCount();
        }

        private void UpdateItemCountOLD()
        {
            var countLabel = this.Controls.Find("lblCount", true).FirstOrDefault() as Label;
            if (countLabel != null)
            {
                countLabel.Text = $"{_historyList.Items.Count} items";
            }
        }

    }
}