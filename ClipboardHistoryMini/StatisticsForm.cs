using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClipboardHistoryMini.Models;
using ClipboardHistoryMini.Services;

namespace ClipboardHistoryMini
{
    public partial class StatisticsForm : Form
    {
        private StorageService _storage;
        private Panel _statsPanel;

        public StatisticsForm(StorageService storage)
        {
            _storage = storage;
            InitializeComponent();
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            var history = _storage.GetHistory();
            int yPos = 10;

            // Title
            AddHeader("Clipboard History Statistics", ref yPos);

            // Basic Stats
            AddSectionHeader("Overview", ref yPos);
            AddStatItem("Total Items", history.Count.ToString(), ref yPos);
            AddStatItem("Pinned Items", history.Count(x => x.IsPinned).ToString(), ref yPos);
            AddStatItem("Text Items", history.Count(x => x.Type == ClipboardItemType.Text).ToString(), ref yPos);
            AddStatItem("Rich Text Items", history.Count(x => x.Type == ClipboardItemType.RichText).ToString(), ref yPos);
            AddStatItem("Image Items", history.Count(x => x.Type == ClipboardItemType.Image).ToString(), ref yPos);

            yPos += 20;

            // Date Stats
            AddSectionHeader("Timeline", ref yPos);
            var today = history.Count(x => x.CopiedAt.Date == DateTime.Today);
            var yesterday = history.Count(x => x.CopiedAt.Date == DateTime.Today.AddDays(-1));
            var thisWeek = history.Count(x => x.CopiedAt >= DateTime.Today.AddDays(-7));
            var thisMonth = history.Count(x => x.CopiedAt >= DateTime.Today.AddDays(-30));

            AddStatItem("Copied Today", today.ToString(), ref yPos);
            AddStatItem("Copied Yesterday", yesterday.ToString(), ref yPos);
            AddStatItem("Last 7 Days", thisWeek.ToString(), ref yPos);
            AddStatItem("Last 30 Days", thisMonth.ToString(), ref yPos);

            yPos += 20;

            // Size Stats
            AddSectionHeader("Storage", ref yPos);
            var textSize = history.Where(x => x.Content != null)
                                 .Sum(x => x.Content.Length);
            var imageSize = history.Where(x => x.ImageData != null)
                                  .Sum(x => x.ImageData.Length);
            var totalSize = textSize + imageSize;

            AddStatItem("Text Size", FormatBytes(textSize), ref yPos);
            AddStatItem("Image Size", FormatBytes(imageSize), ref yPos);
            AddStatItem("Total Size", FormatBytes(totalSize), ref yPos);

            yPos += 20;

            // Most Active
            AddSectionHeader("Most Active Times", ref yPos);
            if (history.Any())
            {
                var oldestItem = history.OrderBy(x => x.CopiedAt).First();
                var newestItem = history.OrderByDescending(x => x.CopiedAt).First();
                var avgPerDay = history.Count / Math.Max(1, (DateTime.Now - oldestItem.CopiedAt).Days);

                AddStatItem("Oldest Item", oldestItem.CopiedAt.ToString("yyyy-MM-dd HH:mm"), ref yPos);
                AddStatItem("Newest Item", newestItem.CopiedAt.ToString("yyyy-MM-dd HH:mm"), ref yPos);
                AddStatItem("Average Per Day", avgPerDay.ToString("F1"), ref yPos);
            }

            yPos += 20;

            // Most Common Words (for text items)
            AddSectionHeader("Top Content Preview", ref yPos);
            var topItems = history.Where(x => x.Type != ClipboardItemType.Image)
                                 .OrderByDescending(x => x.CopiedAt)
                                 .Take(5);

            foreach (var item in topItems)
            {
                AddPreviewItem(item.GetPreview(50), item.CopiedAt, ref yPos);
            }

            // Close button
            var btnClose = new Button
            {
                Text = "Close",
                Size = new Size(100, 35),
                Location = new Point(_statsPanel.Width / 2 - 50, yPos + 20)
            };
            btnClose.Click += (s, e) => this.Close();
            _statsPanel.Controls.Add(btnClose);
        }

        private void AddHeader(string text, ref int yPos)
        {
            var label = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, yPos),
                ForeColor = Color.FromArgb(0, 120, 215)
            };
            _statsPanel.Controls.Add(label);
            yPos += 40;
        }

        private void AddSectionHeader(string text, ref int yPos)
        {
            var label = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, yPos),
                ForeColor = Color.DarkSlateGray
            };
            _statsPanel.Controls.Add(label);
            yPos += 35;
        }

        private void AddStatItem(string label, string value, ref int yPos)
        {
            var lblLabel = new Label
            {
                Text = label + ":",
                Font = new Font("Segoe UI", 10F),
                AutoSize = true,
                Location = new Point(30, yPos),
                Width = 200
            };
            _statsPanel.Controls.Add(lblLabel);

            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(250, yPos),
                ForeColor = Color.FromArgb(0, 120, 215)
            };
            _statsPanel.Controls.Add(lblValue);

            yPos += 30;
        }

        private void AddPreviewItem(string preview, DateTime date, ref int yPos)
        {
            var panel = new Panel
            {
                Location = new Point(30, yPos),
                Size = new Size(500, 50),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            var lblPreview = new Label
            {
                Text = preview,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(5, 5),
                Size = new Size(490, 20),
                AutoEllipsis = true
            };
            panel.Controls.Add(lblPreview);

            var lblDate = new Label
            {
                Text = date.ToString("yyyy-MM-dd HH:mm:ss"),
                Font = new Font("Segoe UI", 8F),
                Location = new Point(5, 28),
                ForeColor = Color.Gray,
                AutoSize = true
            };
            panel.Controls.Add(lblDate);

            _statsPanel.Controls.Add(panel);
            yPos += 60;
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}