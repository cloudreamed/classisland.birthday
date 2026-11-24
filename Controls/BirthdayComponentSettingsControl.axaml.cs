using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using BirthdayBoard.Models;
using BirthdayBoard.Services;
using ClassIsland.Core.Abstractions.Controls;

namespace BirthdayBoard.Controls;

/// <summary>
/// 「生日」组件的设置界面：编辑生日名单，支持从 txt 文件导入。
/// </summary>
public partial class BirthdayComponentSettingsControl : ComponentBase<BirthdayComponentSettings>
{
    public BirthdayComponentSettingsControl()
    {
        InitializeComponent();
        // 插件程序集在独立加载上下文中，XAML 里用 AncestorType=local:... 会触发
        // Assembly.Load("BirthdayBoard") 而失败；因此在挂载后直接把 DataContext 指向自身。
        AttachedToVisualTree += (_, _) =>
        {
            DataContext = this;
            MaxCountdownBox.Text = (Settings?.MaxCountdownDays ?? 0).ToString();
        };
    }

    private void AddEntry_Click(object? sender, RoutedEventArgs e)
    {
        Settings?.Entries.Add(new BirthdayEntry());
    }

    private void DeleteEntry_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: BirthdayEntry entry } && Settings != null)
        {
            Settings.Entries.Remove(entry);
        }
    }

    private void MaxCountdownBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (Settings == null)
        {
            return;
        }

        if (int.TryParse(MaxCountdownBox.Text, out var value) && value >= 0)
        {
            Settings.MaxCountdownDays = value;
        }
    }

    private async void ImportFromFile_Click(object? sender, RoutedEventArgs e)
    {
        var settings = Settings;
        var topLevel = TopLevel.GetTopLevel(this);
        if (settings == null || topLevel?.StorageProvider is not { } storageProvider)
        {
            return;
        }

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "导入生日名单",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("文本文件") { Patterns = new[] { "*.txt" } }
            }
        });

        if (files.Count == 0)
        {
            return;
        }

        var path = files[0].Path.LocalPath;
        string text;
        try
        {
            text = await File.ReadAllTextAsync(path);
        }
        catch (Exception)
        {
            ShowStatus("读取文件失败，请确认文件没有被占用。");
            return;
        }

        // .NET 默认按 UTF-8（含 BOM 识别）解码；出现替换符说明编码不是 UTF-8。
        if (text.Contains('\uFFFD'))
        {
            ShowStatus("文件编码无法识别，请把 txt 另存为 UTF-8 编码后重试。");
            return;
        }

        var entries = BirthdayImportParser.Parse(text);
        if (entries.Count == 0)
        {
            ShowStatus("没有解析到有效记录。每行格式应为：张三 2008年3月6日 0（0=公历，1=农历）；也兼容旧的：张三 3月6日");
            return;
        }

        var added = 0;
        foreach (var entry in entries)
        {
            var exists = settings.Entries.Any(x =>
                x != null &&
                string.Equals(x.Name?.Trim(), entry.Name.Trim(), StringComparison.Ordinal) &&
                x.Year == entry.Year &&
                x.Month == entry.Month &&
                x.Day == entry.Day &&
                x.IsLunar == entry.IsLunar);
            if (exists)
            {
                continue;
            }

            settings.Entries.Add(entry);
            added++;
        }

        ShowStatus(added > 0 ? $"已导入 {added} 条生日记录。" : "名单中已存在这些记录，未新增。");
    }

    private void ShowStatus(string message)
    {
        ImportStatusText.Text = message;
        ImportStatusText.IsVisible = true;
    }
}
