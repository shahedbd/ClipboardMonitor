using System;
using System.Drawing;

namespace ClipboardHistoryMini.Models
{
    public enum ClipboardItemType
    {
        Text,
        RichText,
        Image
    }

    public class ClipboardItem
    {
        public Guid Id { get; set; }
        public ClipboardItemType Type { get; set; }
        public string Content { get; set; }
        public byte[] ImageData { get; set; }
        public DateTime CopiedAt { get; set; }
        public bool IsPinned { get; set; }
        public string Preview { get; set; }

        public ClipboardItem()
        {
            Id = Guid.NewGuid();
            CopiedAt = DateTime.Now;
            IsPinned = false;
        }

        public string GetPreview(int maxLength = 100)
        {
            if (!string.IsNullOrEmpty(Preview))
                return Preview;

            //if (Type == ClipboardItemType.Image)
            //    return "[Image]";

            if (string.IsNullOrEmpty(Content))
                return "[Empty]";

            var preview = Content.Replace("\r\n", " ").Replace("\n", " ").Trim();
            if (preview.Length > maxLength)
                preview = preview.Substring(0, maxLength) + "...";

            Preview = preview;
            return Preview;
        }

        public Image GetThumbnail(int width = 64, int height = 64)
        {
            if (ImageData == null || ImageData.Length == 0)
                return null;

            try
            {
                using (var ms = new System.IO.MemoryStream(ImageData))
                {
                    var img = Image.FromStream(ms);
                    return img.GetThumbnailImage(width, height, null, IntPtr.Zero);
                }
            }
            catch
            {
                return null;
            }
        }
    }

    public class AppSettings
    {
        public int MaxHistorySize { get; set; } = 50;
        public bool LaunchAtStartup { get; set; } = false;
        public bool TrackImages { get; set; } = true;
        public bool ShowNotifications { get; set; } = false;
        public string GlobalHotkey { get; set; } = "Ctrl+Shift+V";
        public string[] ExcludedApps { get; set; } = new string[] { };
    }
}