using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using ClipboardHistoryMini.Models;
using ClipboardHistoryMini.Services;

namespace ClipboardHistoryMini
{
    public partial class SettingsForm : Form
    {
        private StorageService _storage;
        private AppSettings _settings;
        private NumericUpDown _numMaxHistory;
        private CheckBox _chkLaunchStartup;
        private CheckBox _chkTrackImages;
        private CheckBox _chkShowNotifications;

        public SettingsForm(StorageService storage)
        {
            _storage = storage;
            _settings = _storage.GetSettings();
            InitializeComponent();
            SetupUI();
            LoadSettings();
        }

        private void SetupUI()
        {
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 2,
                RowCount = 7,
                AutoSize = true
            };
            this.Controls.Add(mainPanel);

            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

            // Max History Size
            var lblMaxHistory = new Label
            {
                Text = "Max History Size:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };
            mainPanel.Controls.Add(lblMaxHistory, 0, 0);

            _numMaxHistory = new NumericUpDown
            {
                Minimum = 10,
                Maximum = 100,
                Dock = DockStyle.Left,
                Width = 100
            };
            mainPanel.Controls.Add(_numMaxHistory, 1, 0);

            // Launch at Startup
            _chkLaunchStartup = new CheckBox
            {
                Text = "Launch at Windows startup",
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            mainPanel.Controls.Add(_chkLaunchStartup, 0, 1);
            mainPanel.SetColumnSpan(_chkLaunchStartup, 2);

            // Track Images
            _chkTrackImages = new CheckBox
            {
                Text = "Track images (may increase memory usage)",
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            mainPanel.Controls.Add(_chkTrackImages, 0, 2);
            mainPanel.SetColumnSpan(_chkTrackImages, 2);

            // Show Notifications
            _chkShowNotifications = new CheckBox
            {
                Text = "Show notifications when clipboard changes",
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            mainPanel.Controls.Add(_chkShowNotifications, 0, 3);
            mainPanel.SetColumnSpan(_chkShowNotifications, 2);

            // Spacer
            mainPanel.Controls.Add(new Label { Height = 20 }, 0, 4);

            // Info Label
            var lblInfo = new Label
            {
                Text = "Note: All clipboard data is stored locally on your device.\n" +
                       "No data is sent to the cloud or shared with third parties.",
                Dock = DockStyle.Fill,
                AutoSize = true,
                ForeColor = Color.Gray
            };
            mainPanel.Controls.Add(lblInfo, 0, 5);
            mainPanel.SetColumnSpan(lblInfo, 2);

            // Buttons Panel
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };
            mainPanel.Controls.Add(buttonPanel, 0, 6);
            mainPanel.SetColumnSpan(buttonPanel, 2);

            var btnCancel = new Button
            {
                Text = "Cancel",
                Width = 80,
                Height = 30,
                DialogResult = DialogResult.Cancel
            };
            buttonPanel.Controls.Add(btnCancel);

            var btnSave = new Button
            {
                Text = "Save",
                Width = 80,
                Height = 30
            };
            btnSave.Click += BtnSave_Click;
            buttonPanel.Controls.Add(btnSave);

            var btnClearAll = new Button
            {
                Text = "Clear All Data",
                Width = 120,
                Height = 30,
                Margin = new Padding(20, 0, 0, 0)
            };
            btnClearAll.Click += BtnClearAll_Click;
            buttonPanel.Controls.Add(btnClearAll);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadSettings()
        {
            _numMaxHistory.Value = _settings.MaxHistorySize;
            _chkLaunchStartup.Checked = _settings.LaunchAtStartup;
            _chkTrackImages.Checked = _settings.TrackImages;
            _chkShowNotifications.Checked = _settings.ShowNotifications;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            _settings.MaxHistorySize = (int)_numMaxHistory.Value;
            _settings.TrackImages = _chkTrackImages.Checked;
            _settings.ShowNotifications = _chkShowNotifications.Checked;

            // Handle startup setting
            if (_chkLaunchStartup.Checked != _settings.LaunchAtStartup)
            {
                if (SetStartup(_chkLaunchStartup.Checked))
                {
                    _settings.LaunchAtStartup = _chkLaunchStartup.Checked;
                }
                else
                {
                    MessageBox.Show(
                        "Failed to update startup settings. Please check your permissions.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }

            _storage.SaveSettings(_settings);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "This will delete ALL clipboard history including pinned items.\n\nAre you sure?",
                "Clear All Data",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                _storage.ClearAll();
                MessageBox.Show(
                    "All clipboard history has been cleared.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private bool SetStartup(bool enable)
        {
            try
            {
                string appName = "ClipboardHistoryMini";
                string appPath = Application.ExecutablePath;

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (enable)
                    {
                        key?.SetValue(appName, $"\"{appPath}\"");
                    }
                    else
                    {
                        key?.DeleteValue(appName, false);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}