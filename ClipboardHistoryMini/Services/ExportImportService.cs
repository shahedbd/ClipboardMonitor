using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using ClipboardHistoryMini.Models;

namespace ClipboardHistoryMini.Services
{
    public class ExportImportService
    {
        private readonly StorageService _storage;

        public ExportImportService(StorageService storage)
        {
            _storage = storage;
        }

        public bool ExportToFile(string filePath)
        {
            try
            {
                var history = _storage.GetHistory();
                var exportData = new ExportData
                {
                    ExportDate = DateTime.Now,
                    Version = "1.0",
                    ItemCount = history.Count,
                    Items = history
                };

                var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                // Compress if file is large
                if (json.Length > 1024 * 1024) // > 1MB
                {
                    using var fileStream = File.Create(filePath + ".gz");
                    using var gzipStream = new GZipStream(fileStream, CompressionMode.Compress);
                    using var writer = new StreamWriter(gzipStream);
                    writer.Write(json);
                }
                else
                {
                    File.WriteAllText(filePath, json);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool ImportFromFile(string filePath)
        {
            try
            {
                string json;

                // Check if compressed
                if (filePath.EndsWith(".gz"))
                {
                    using var fileStream = File.OpenRead(filePath);
                    using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
                    using var reader = new StreamReader(gzipStream);
                    json = reader.ReadToEnd();
                }
                else
                {
                    json = File.ReadAllText(filePath);
                }

                var exportData = JsonSerializer.Deserialize<ExportData>(json);

                if (exportData == null || exportData.Items == null)
                {
                    MessageBox.Show("Invalid export file format.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Ask user how to handle existing data
                var result = MessageBox.Show(
                    $"Import {exportData.ItemCount} items?\n\n" +
                    "Yes - Merge with existing history\n" +
                    "No - Replace existing history\n" +
                    "Cancel - Cancel import",
                    "Import Clipboard History",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Cancel)
                    return false;

                if (result == DialogResult.No)
                {
                    // Clear existing (keep pinned)
                    _storage.ClearHistory();
                }

                // Import items
                foreach (var item in exportData.Items)
                {
                    _storage.AddItem(item);
                }

                MessageBox.Show($"Successfully imported {exportData.ItemCount} items!",
                    "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Import failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool ExportAsText(string filePath, bool includeImages = false)
        {
            try
            {
                var history = _storage.GetHistory();
                using var writer = new StreamWriter(filePath);

                writer.WriteLine("Clipboard History Export");
                writer.WriteLine($"Exported: {DateTime.Now}");
                writer.WriteLine($"Total Items: {history.Count}");
                writer.WriteLine(new string('=', 60));
                writer.WriteLine();

                int index = 1;
                foreach (var item in history)
                {
                    writer.WriteLine($"[{index}] {item.Type} - {item.CopiedAt:yyyy-MM-dd HH:mm:ss}");

                    if (item.IsPinned)
                        writer.WriteLine("    📌 PINNED");

                    if (item.Type == ClipboardItemType.Image)
                    {
                        writer.WriteLine("    [Image Content]");
                        if (includeImages && item.ImageData != null)
                        {
                            writer.WriteLine($"    Size: {item.ImageData.Length} bytes");
                        }
                    }
                    else
                    {
                        writer.WriteLine($"    {item.Content}");
                    }

                    writer.WriteLine();
                    index++;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void ShowExportDialog()
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|Compressed JSON (*.json.gz)|*.json.gz|Text Files (*.txt)|*.txt",
                DefaultExt = "json",
                FileName = $"clipboard_history_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.FileName.EndsWith(".txt"))
                {
                    ExportAsText(dialog.FileName);
                }
                else
                {
                    ExportToFile(dialog.FileName);
                }
            }
        }

        public void ShowImportDialog()
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|Compressed JSON (*.json.gz)|*.json.gz|All Files (*.*)|*.*",
                DefaultExt = "json"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ImportFromFile(dialog.FileName);
            }
        }
    }

    public class ExportData
    {
        public DateTime ExportDate { get; set; }
        public string Version { get; set; }
        public int ItemCount { get; set; }
        public List<ClipboardItem> Items { get; set; }
    }
}