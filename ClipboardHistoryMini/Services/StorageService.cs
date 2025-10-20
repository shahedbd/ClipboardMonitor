using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ClipboardHistoryMini.Models;

namespace ClipboardHistoryMini.Services
{
    public class StorageService
    {
        private readonly string _dataFolder;
        private readonly string _historyFile;
        private readonly string _settingsFile;
        private List<ClipboardItem> _history;
        private AppSettings _settings;

        public StorageService()
        {
            _dataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClipboardHistoryMini"
            );

            if (!Directory.Exists(_dataFolder))
                Directory.CreateDirectory(_dataFolder);

            _historyFile = Path.Combine(_dataFolder, "history.json");
            _settingsFile = Path.Combine(_dataFolder, "settings.json");

            _history = new List<ClipboardItem>();
            _settings = new AppSettings();

            LoadHistory();
            LoadSettings();
        }

        public List<ClipboardItem> GetHistory()
        {
            return _history.OrderByDescending(x => x.IsPinned)
                          .ThenByDescending(x => x.CopiedAt)
                          .ToList();
        }

        public void AddItem(ClipboardItem item)
        {
            // Check if item already exists (avoid duplicates)
            var existing = _history.FirstOrDefault(x =>
                x.Type == item.Type &&
                x.Content == item.Content);

            if (existing != null)
            {
                // Update timestamp and move to top
                existing.CopiedAt = DateTime.Now;
            }
            else
            {
                _history.Insert(0, item);

                // Remove oldest non-pinned items if over limit
                var nonPinned = _history.Where(x => !x.IsPinned).ToList();
                if (nonPinned.Count > _settings.MaxHistorySize)
                {
                    var toRemove = nonPinned
                        .OrderBy(x => x.CopiedAt)
                        .Take(nonPinned.Count - _settings.MaxHistorySize)
                        .ToList();

                    foreach (var old in toRemove)
                    {
                        _history.Remove(old);
                    }
                }
            }

            SaveHistory();
        }

        public void RemoveItem(Guid id)
        {
            var item = _history.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                _history.Remove(item);
                SaveHistory();
            }
        }

        public void TogglePin(Guid id)
        {
            var item = _history.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                item.IsPinned = !item.IsPinned;
                SaveHistory();
            }
        }

        public void ClearHistory()
        {
            _history.RemoveAll(x => !x.IsPinned);
            SaveHistory();
        }

        public void ClearAll()
        {
            _history.Clear();
            SaveHistory();
        }

        public List<ClipboardItem> SearchHistory(string query, ClipboardItemType? filterType = null)
        {
            var items = _history.AsEnumerable();

            if (filterType.HasValue)
                items = items.Where(x => x.Type == filterType.Value);

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();
                items = items.Where(x =>
                    x.Content?.ToLower().Contains(query) == true ||
                    x.GetPreview().ToLower().Contains(query));
            }

            return items.OrderByDescending(x => x.IsPinned)
                       .ThenByDescending(x => x.CopiedAt)
                       .ToList();
        }

        public AppSettings GetSettings()
        {
            return _settings;
        }

        public void SaveSettings(AppSettings settings)
        {
            _settings = settings;
            try
            {
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_settingsFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        private void LoadHistory()
        {
            try
            {
                if (File.Exists(_historyFile))
                {
                    var json = File.ReadAllText(_historyFile);
                    _history = JsonSerializer.Deserialize<List<ClipboardItem>>(json) ?? new List<ClipboardItem>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load history: {ex.Message}");
                _history = new List<ClipboardItem>();
            }
        }

        private void SaveHistory()
        {
            try
            {
                var json = JsonSerializer.Serialize(_history, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_historyFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save history: {ex.Message}");
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFile))
                {
                    var json = File.ReadAllText(_settingsFile);
                    _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
                _settings = new AppSettings();
            }
        }
    }
}