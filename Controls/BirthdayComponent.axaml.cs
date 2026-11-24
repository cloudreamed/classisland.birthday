using System;
using Avalonia;
using BirthdayBoard.Models;
using BirthdayBoard.Services;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;

namespace BirthdayBoard.Controls;

/// <summary>
/// 主界面「生日」组件：显示今天过生日的同学；今天没有人过生日时，显示最近一次生日的倒计时。
/// 当最近一次生日超过设置的上限天数时，整个组件隐藏。
/// </summary>
[ComponentInfo("7E3F1A2B-5C4D-4E6F-8A9B-0C1D2E3F4A5B", "生日", "\ue304",
    "显示今天过生日的同学姓名；支持公历/农历生日，并可设置倒计时最大天数。")]
public partial class BirthdayComponent : ComponentBase<BirthdayComponentSettings>
{
    private readonly ILessonsService _lessonsService;
    private readonly IExactTimeService _exactTimeService;

    public BirthdayComponent(ILessonsService lessonsService, IExactTimeService exactTimeService)
    {
        _lessonsService = lessonsService;
        _exactTimeService = exactTimeService;
        InitializeComponent();

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        UpdateDisplay();
        _lessonsService.PostMainTimerTicked += OnMainTimerTicked;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _lessonsService.PostMainTimerTicked -= OnMainTimerTicked;
    }

    private void OnMainTimerTicked(object? sender, EventArgs e) => UpdateDisplay();

    private void UpdateDisplay()
    {
        var settings = Settings;
        if (settings == null)
        {
            IsVisible = false;
            DisplayTextBlock.Text = string.Empty;
            return;
        }

        var today = _exactTimeService.GetCurrentLocalDateTime().Date;
        var text = BirthdayTextBuilder.Build(settings.Entries, today, settings.MaxCountdownDays);
        if (text is null)
        {
            // 超过倒计时上限：整个组件隐藏。
            IsVisible = false;
            DisplayTextBlock.Text = string.Empty;
        }
        else
        {
            IsVisible = true;
            DisplayTextBlock.Text = text;
        }
    }
}
