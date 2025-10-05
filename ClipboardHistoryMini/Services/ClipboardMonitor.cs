using ClipboardHistoryMini.Models;
using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClipboardHistoryMini.Services
{
    public class ClipboardMonitor : Form
    {
        private const int WM_CLIPBOARDUPDATE = 0x031D;
        private const int MAX_IMAGE_SIZE_BYTES = 5 * 1024 * 1024; // 5MB limit for images

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        public event EventHandler<ClipboardItem> ClipboardChanged;

        private bool _trackImages = true;
        private string _lastTextContent = string.Empty;
        private byte[] _lastImageHash = null;

        public ClipboardMonitor()
        {
            // Create invisible form for clipboard monitoring
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            this.Opacity = 0;
        }

        public void StartMonitoring()
        {
            AddClipboardFormatListener(this.Handle);
        }

        public void StopMonitoring()
        {
            RemoveClipboardFormatListener(this.Handle);
        }

        public void SetTrackImages(bool track)
        {
            _trackImages = track;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                ProcessClipboardChange();
            }
            base.WndProc(ref m);
        }

        private void ProcessClipboardChange()
        {
            try
            {
                ClipboardItem item = null;

                // Priority 1: Check for file drops (images or other files)
                if (Clipboard.ContainsFileDropList())
                {
                    item = ProcessFileDropList();
                }
                // Priority 2: Check for image
                //else if (_trackImages && Clipboard.ContainsImage())
                //{
                //    item = ProcessImage();
                //}
                // Priority 3: Check for RTF
                else if (Clipboard.ContainsData(DataFormats.Rtf))
                {
                    item = ProcessRichText();
                }
                // Priority 4: Check for plain text
                else if (Clipboard.ContainsText())
                {
                    item = ProcessPlainText();
                }

                if (item != null)
                {
                    ClipboardChanged?.Invoke(this, item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Clipboard monitoring error: {ex.Message}");
            }
        }
        private ClipboardItem ProcessFileDropList()
        {
            try
            {
                var files = Clipboard.GetFileDropList();
                if (files == null || files.Count == 0)
                    return null;

                // Get the first file
                string filePath = files[0];

                if (!File.Exists(filePath))
                    return null;

                var fileInfo = new FileInfo(filePath);

                // Determine type based on file extension
                ClipboardItemType type;
                string ext = fileInfo.Extension.ToLower();
                if (ext == ".txt" || ext == ".log" || ext == ".csv")
                {
                    type = ClipboardItemType.Text;
                }
                else if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
                {
                    type = ClipboardItemType.Image;
                }
                else
                {
                    type = ClipboardItemType.Text; // Default to Text type for file paths
                }

                _lastTextContent = filePath;

                return new ClipboardItem
                {
                    Type = type,
                    Content = filePath // Always store the full file path
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing file drop: {ex.Message}");
            }

            return null;
        }

        private ClipboardItem ProcessRichText()
        {
            try
            {
                var text = Clipboard.GetText();

                if (!string.IsNullOrEmpty(text) && text != _lastTextContent)
                {
                    _lastTextContent = text;
                    return new ClipboardItem
                    {
                        Type = ClipboardItemType.RichText,
                        Content = text
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing rich text: {ex.Message}");
            }

            return null;
        }

        private ClipboardItem ProcessPlainText()
        {
            try
            {
                var text = Clipboard.GetText();

                if (!string.IsNullOrEmpty(text) && text != _lastTextContent)
                {
                    _lastTextContent = text;
                    return new ClipboardItem
                    {
                        Type = ClipboardItemType.Text,
                        Content = text
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing plain text: {ex.Message}");
            }

            return null;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopMonitoring();
            }
            base.Dispose(disposing);
        }
    }
}