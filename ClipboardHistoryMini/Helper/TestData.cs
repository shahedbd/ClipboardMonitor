using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ClipboardHistoryMini.Helper
{
    public static class TestData
    {
        public static void LoadSampleData(ListView _historyList)
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
                new { Type = "Text", Content = "C:\\Users\\User\\Downloads\\clipboard_readme.md", Time = DateTime.Now.AddMinutes(-25), Pinned = true },
                new { Type = "Image", Content = "diagram.jpg (800x600)", Time = DateTime.Now.AddMinutes(-30), Pinned = false }
            };

            foreach (var item in items)
            {
                var listItem = new ListViewItem(item.Type);
                listItem.SubItems.Add(item.Content.Length > 50 ? item.Content.Substring(0, 47) + "..." : item.Content);

                // Extract filename for file paths
                string fileName = "";
                if (item.Content.Contains("\\") || item.Content.Contains("/"))
                {
                    try
                    {
                        fileName = Path.GetFileName(item.Content);
                    }
                    catch
                    {
                        fileName = "";
                    }
                }
                listItem.SubItems.Add(fileName); // NEW: File name column

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
        }
    }
}
