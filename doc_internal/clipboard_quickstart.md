# Clipboard History Mini - Quick Start Guide

## Step-by-Step Setup

### 1. Prerequisites Check

Before starting, ensure you have:
- Windows 10 or Windows 11 (64-bit)
- .NET 8.0 SDK installed ([Download here](https://dotnet.microsoft.com/download/dotnet/8.0))

To verify .NET installation, open Command Prompt and run:
```bash
dotnet --version
```
You should see version 8.0 or higher.

### 2. Create Project Structure

Open Command Prompt or PowerShell and run:

```bash
# Create main project folder
mkdir ClipboardHistoryMini
cd ClipboardHistoryMini

# Create subfolders
mkdir Models
mkdir Services
```

### 3. Add Source Files

Create the following files with the provided code:

**Root Directory:**
- `ClipboardHistoryMini.csproj` - Project configuration
- `Program.cs` - Application entry point
- `MainForm.cs` - Main window UI and logic
- `SettingsForm.cs` - Settings dialog

**Models Folder:**
- `Models/ClipboardModels.cs` - Data models (ClipboardItem, AppSettings)

**Services Folder:**
- `Services/ClipboardMonitor.cs` - Clipboard monitoring service
- `Services/StorageService.cs` - Data persistence
- `Services/HotkeyManager.cs` - Global hotkey handler

### 4. Build the Application

In the project root directory, run:

```bash
dotnet restore
dotnet build -c Release
```

If successful, you'll see: `Build succeeded.`

### 5. Run the Application

```bash
dotnet run
```

The application will start and appear in your system tray!

### 6. Test Basic Features

1. **Copy some text** - Select and copy any text (Ctrl+C)
2. **Press Ctrl+Shift+V** - The history window should open
3. **View your copied text** - You should see it in the list
4. **Double-click an item** - It will be copied back to your clipboard
5. **Right-click an item** - Try pinning it

## Common Setup Issues

### Issue: "SDK not found"
**Solution**: Install .NET 8.0 SDK from Microsoft's website

### Issue: "Build failed - missing references"
**Solution**: Run `dotnet restore` to restore NuGet packages

### Issue: "Cannot find Program.Main"
**Solution**: Ensure `Program.cs` has the correct namespace and Main method

### Issue: "WinForms types not found"
**Solution**: Verify `<UseWindowsForms>true</UseWindowsForms>` is in .csproj

## Creating a Standalone Executable

To create a single .exe file that doesn't require .NET installation:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false
```

Find your executable at:
```
bin\Release\net8.0-windows\win-x64\publish\ClipboardHistoryMini.exe
```

Copy this file anywhere and run it!

## File Locations

After first run, the app creates:

```
%AppData%\ClipboardHistoryMini\
├── history.json     (your clipboard history)
└── settings.json    (your preferences)
```

To view this folder, press `Win+R` and type: `%AppData%\ClipboardHistoryMini`

## First-Time Configuration

1. **Open Settings** (click system tray icon → Settings)
2. **Set max history size** (default: 50 items)
3. **Enable "Launch at startup"** if you want it to auto-start
4. **Configure image tracking** (enable/disable based on needs)
5. **Test notifications** (optional)

## Testing the App

### Test 1: Text Clipboard
1. Copy this text: "Hello World"
2. Press Ctrl+Shift+V
3. Verify "Hello World" appears in history

### Test 2: Multiple Items
1. Copy several different texts
2. Open history (Ctrl+Shift+V)
3. Verify all items are listed with timestamps

### Test 3: Restore Item
1. Copy "Test 1", then copy "Test 2"
2. Open history
3. Double-click "Test 1"
4. Paste (Ctrl+V) - should paste "Test 1"

### Test 4: Search
1. Copy several items
2. Open history
3. Type in search box
4. Verify filtered results

### Test 5: Pin/Unpin
1. Right-click any item
2. Select "Pin/Unpin"
3. Verify 📌 icon appears
4. Try clearing history - pinned items should remain

### Test 6: Image Clipboard
1. Take a screenshot (Win+Shift+S)
2. Check history - should show [Image] entry
3. Double-click to restore image
4. Paste in Paint to verify

## Development Tips

### Running in Debug Mode
```bash
dotnet run --configuration Debug
```

### Viewing Debug Output
Add this to see debug messages:
```csharp
System.Diagnostics.Debug.WriteLine("Your message");
```
View in Visual Studio's Output window or DebugView tool.

### Hot Reload (Quick Testing)
```bash
dotnet watch run
```
Changes to code automatically rebuild and restart the app.

### Clean Build
```bash
dotnet clean
dotnet build -c Release
```

## IDE Setup

### Visual Studio 2022
1. File → New → Project
2. Choose "Windows Forms App (.NET 8.0)"
3. Name: ClipboardHistoryMini
4. Replace generated files with provided code
5. Press F5 to run

### Visual Studio Code
1. Install C# Dev Kit extension
2. Open folder with project files
3. Press F5 to debug
4. Or use terminal: `dotnet run`

### JetBrains Rider
1. File → Open → Select folder
2. Wait for project indexing
3. Click Run button or Shift+F10

## Next Steps

Once the app is running:

1. **Customize Settings** - Adjust to your preferences
2. **Set Global Hotkey** - Currently Ctrl+Shift+V (hardcoded)
3. **Test with Different Apps** - Try Word, Chrome, Notepad, etc.
4. **Monitor Performance** - Check Task Manager for resource usage
5. **Backup Data** - Copy `%AppData%\ClipboardHistoryMini\` folder

## Getting Help

If you encounter issues:

1. Check the console output for error messages
2. Review the README.md troubleshooting section
3. Verify all source files are in correct folders
4. Ensure .NET 8.0 SDK is properly installed
5. Try a clean build: `dotnet clean && dotnet build`

## Distribution

To share with others:

**Option 1: Portable**
- Copy the published .exe
- Include a text file with instructions
- No installation needed!

**Option 2: Installer**
- Use Inno Setup (free)
- Creates professional installer
- Handles shortcuts and startup entries

**Option 3: ZIP Package**
- Publish self-contained
- Compress entire publish folder
- Include README in ZIP

---

**Congratulations!** 🎉 You now have a fully functional clipboard history manager!

Enjoy tracking your clipboard history with privacy and speed!
