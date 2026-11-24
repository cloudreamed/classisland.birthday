using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BirthdayBoard.Models;

/// <summary>
/// 一条生日记录：姓名 + 公历出生日期 + 过生日采用的历法。
/// </summary>
public class BirthdayEntry : INotifyPropertyChanged
{
    private static readonly IReadOnlyList<int> SharedYears = BuildYearOptions();
    private static readonly IReadOnlyList<int> SharedMonths = Enumerable.Range(1, 12).ToArray();
    private static readonly IReadOnlyList<int> Days28 = Enumerable.Range(1, 28).ToArray();
    private static readonly IReadOnlyList<int> Days29 = Enumerable.Range(1, 29).ToArray();
    private static readonly IReadOnlyList<int> Days30 = Enumerable.Range(1, 30).ToArray();
    private static readonly IReadOnlyList<int> Days31 = Enumerable.Range(1, 31).ToArray();

    private string _name = "";
    private int? _year;
    private int _month = 1;
    private int _day = 1;
    private bool _isLunar;

    /// <summary>同学姓名。</summary>
    public string Name
    {
        get => _name;
        set
        {
            if (value == _name) return;
            _name = value;
            OnPropertyChanged();
        }
    }

    /// <summary>公历出生年份；null 表示未知（旧格式导入）。</summary>
    public int? Year
    {
        get => _year;
        set
        {
            if (value == _year) return;
            _year = value;
            OnPropertyChanged();
            ClampDay();
            OnPropertyChanged(nameof(DayOptions));
        }
    }

    /// <summary>公历出生月份（1-12）。</summary>
    public int Month
    {
        get => _month;
        set
        {
            if (value < 1 || value > 12 || value == _month) return;
            _month = value;
            OnPropertyChanged();
            ClampDay();
            OnPropertyChanged(nameof(DayOptions));
        }
    }

    /// <summary>公历出生日（1-31）。</summary>
    public int Day
    {
        get => _day;
        set
        {
            if (value < 1 || value == _day) return;
            _day = value;
            OnPropertyChanged();
        }
    }

    /// <summary>true = 按农历过生日（用公历出生日期换算成农历月日）。</summary>
    public bool IsLunar
    {
        get => _isLunar;
        set
        {
            if (value == _isLunar) return;
            _isLunar = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CelebrateLunarIndex));
        }
    }

    /// <summary>设置界面「公历 / 农历」下拉框的选中项（0 = 公历，1 = 农历）。</summary>
    public int CelebrateLunarIndex
    {
        get => _isLunar ? 1 : 0;
        set
        {
            var lunar = value == 1;
            if (lunar == _isLunar) return;
            _isLunar = lunar;
            OnPropertyChanged(nameof(IsLunar));
            OnPropertyChanged();
        }
    }

    /// <summary>年份下拉框的可选项（1900 到今年）。</summary>
    public IReadOnlyList<int> YearOptions => SharedYears;

    /// <summary>月份下拉框的可选项。</summary>
    public IReadOnlyList<int> MonthOptions => SharedMonths;

    /// <summary>日期下拉框的可选项（随年份/月份变化；返回共享实例，避免下拉框反复重置选中项）。</summary>
    public IReadOnlyList<int> DayOptions => MaxDayInMonth() switch
    {
        28 => Days28,
        29 => Days29,
        30 => Days30,
        _ => Days31
    };

    private int MaxDayInMonth()
    {
        if (_month is < 1 or > 12)
        {
            return 31;
        }

        var year = _year is > 0 ? _year.Value : 2000;
        return DateTime.DaysInMonth(year, _month);
    }

    private void ClampDay()
    {
        var max = MaxDayInMonth();
        if (_day > max)
        {
            _day = max;
            OnPropertyChanged(nameof(Day));
        }
    }

    private static IReadOnlyList<int> BuildYearOptions()
    {
        var current = DateTime.Now.Year;
        var list = new int[current - 1900 + 1];
        for (var i = 0; i < list.Length; i++)
        {
            list[i] = 1900 + i;
        }

        return list;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
