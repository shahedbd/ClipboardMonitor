using ClipboardHistoryMini.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClipboardHistoryMini.Services
{
    public class ClipboardMonitor : Form
    {
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        public event EventHandler<ClipboardItem> ClipboardChanged;

        private bool _trackImages = true;
        private string _lastTextContent = string.Empty;

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
                if (!Clipboard.ContainsData(DataFormats.Text) &&
                    !Clipboard.ContainsData(DataFormats.Rtf) &&
                    !Clipboard.ContainsImage())
                    return;

                ClipboardItem item = null;

                // Check for image
                if (_trackImages && Clipboard.ContainsImage())
                {
                    var img = Clipboard.GetImage();
                    if (img != null)
                    {
                        item = new ClipboardItem
                        {
                            Type = ClipboardItemType.Image,
                            ImageData = ImageToByteArray(img)
                        };
                    }
                }
                // Check for RTF
                else if (Clipboard.ContainsData(DataFormats.Rtf))
                {
                    var rtf = Clipboard.GetData(DataFormats.Rtf) as string;
                    var text = Clipboard.GetText();

                    if (!string.IsNullOrEmpty(text) && text != _lastTextContent)
                    {
                        item = new ClipboardItem
                        {
                            Type = ClipboardItemType.RichText,
                            Content = text
                        };
                        _lastTextContent = text;
                    }
                }
                // Check for plain text
                else if (Clipboard.ContainsText())
                {
                    var text = Clipboard.GetText();
                    if (!string.IsNullOrEmpty(text) && text != _lastTextContent)
                    {
                        item = new ClipboardItem
                        {
                            Type = ClipboardItemType.Text,
                            Content = text
                        };
                        _lastTextContent = text;
                    }
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

        private byte[] ImageToByteArray(Image img)
        {
            using (var ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
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