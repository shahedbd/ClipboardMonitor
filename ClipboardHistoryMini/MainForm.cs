using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClipboardHistoryMini.Models;
using ClipboardHistoryMini.Services;

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
        }

        //private void InitializeComponent()
        //{
        //    this.Text = "Clipboard History Mini";
        //    this.Size = new Size(600, 500);
        //    this.MinimumSize = new Size(400, 300);
        //    this.StartPosition = FormStartPosition.CenterScreen;
        //    this.Icon = SystemIcons.Application;
        //}

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
            // Main panel
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            this.Controls.Add(mainPanel);

            // Top toolbar
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(0, 0, 0, 10)
            };
            mainPanel.Controls.Add(toolbar);

            // Search box
            _searchBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "Search clipboard history..."
            };
            _searchBox.TextChanged += SearchBox_TextChanged;
            toolbar.Controls.Add(_searchBox);

            // Filter buttons panel
            var filterPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 200,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(5, 0, 0, 0)
            };
            toolbar.Controls.Add(filterPanel);

            var btnAll = new Button { Text = "All", Width = 60, Height = 30 };
            btnAll.Click += (s, e) => FilterHistory(null);
            filterPanel.Controls.Add(btnAll);

            var btnText = new Button { Text = "Text", Width = 60, Height = 30 };
            btnText.Click += (s, e) => FilterHistory(ClipboardItemType.Text);
            filterPanel.Controls.Add(btnText);

            var btnImages = new Button { Text = "Images", Width = 70, Height = 30 };
            btnImages.Click += (s, e) => FilterHistory(ClipboardItemType.Image);
            filterPanel.Controls.Add(btnImages);

            // History ListView
            _historyList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9F),
                MultiSelect = false
            };

            _historyList.Columns.Add("Type", 60);
            _historyList.Columns.Add("Preview", 400);
            _historyList.Columns.Add("Time", 120);
            _historyList.Columns.Add("", 30); // Pin indicator

            _historyList.DoubleClick += HistoryList_DoubleClick;
            _historyList.KeyDown += HistoryList_KeyDown;
            mainPanel.Controls.Add(_historyList);

            // Context menu for list items
            _itemContextMenu = new ContextMenuStrip();
            _itemContextMenu.Items.Add("Copy", null, ContextMenu_Copy);
            _itemContextMenu.Items.Add("Pin/Unpin", null, ContextMenu_TogglePin);
            _itemContextMenu.Items.Add("Delete", null, ContextMenu_Delete);
            _historyList.ContextMenuStrip = _itemContextMenu;

            // Bottom toolbar
            var bottomToolbar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(0, 10, 0, 0)
            };
            mainPanel.Controls.Add(bottomToolbar);

            var btnSettings = new Button
            {
                Text = "Settings",
                Dock = DockStyle.Left,
                Width = 80
            };
            btnSettings.Click += BtnSettings_Click;
            bottomToolbar.Controls.Add(btnSettings);

            var btnStats = new Button
            {
                Text = "Stats",
                Dock = DockStyle.Left,
                Width = 70
            };
            //btnStats.Click += BtnStats_Click;
            bottomToolbar.Controls.Add(btnStats);

            var btnExport = new Button
            {
                Text = "Export",
                Dock = DockStyle.Left,
                Width = 70
            };
            //btnExport.Click += BtnExport_Click;
            bottomToolbar.Controls.Add(btnExport);

            var btnImport = new Button
            {
                Text = "Import",
                Dock = DockStyle.Left,
                Width = 70
            };
            //btnImport.Click += BtnImport_Click;
            bottomToolbar.Controls.Add(btnImport);

            var btnClear = new Button
            {
                Text = "Clear History",
                Dock = DockStyle.Right,
                Width = 100
            };
            btnClear.Click += BtnClear_Click;
            bottomToolbar.Controls.Add(btnClear);

            var lblCount = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "0 items"
            };
            lblCount.Name = "lblCount";
            bottomToolbar.Controls.Add(lblCount);
        }

        private void SetupTrayIcon()
        {
            _trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
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

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        _monitor?.Dispose();
        //        _trayIcon?.Dispose();
        //        _searchDebounceTimer?.Dispose();
        //        _hotkeyManager?.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}