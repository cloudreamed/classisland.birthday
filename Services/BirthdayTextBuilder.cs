using System;
using System.Collections.Generic;
using System.Linq;
using BirthdayBoard.Models;

namespace BirthdayBoard.Services;

/// <summary>
/// 根据生日名单与当前日期生成主界面要显示的文本。
/// </summary>
public static class BirthdayTextBuilder
{
    /// <summary>名单为空时显示的占位文本。</summary>
    public const string EmptyPlaceholder = "尚未录入生日名单";

    /// <summary>今天有人过生日时的前缀。</summary>
    public const string TodayPrefix = "今日生日：";

    /// <summary>
    /// 生成显示文本。返回 <c>null</c> 表示「按倒计时上限应当隐藏整个板块」。
    /// </summary>
    public static string? Build(IEnumerable<BirthdayEntry>? entries, DateTime today, int maxCountdownDays = 0)
    {
        var date = today.Date;
        var list = (entries ?? Enumerable.Empty<BirthdayEntry>())
            .Where(e => e != null && !string.IsNullOrWhiteSpace(e.Name) && IsUsable(e))
            .ToList();

        if (list.Count == 0)
        {
            return EmptyPlaceholder;
        }

        // 今天过生日的人：公历、农历两种规则都算进来，同时显示。
        var todays = list
            .Where(e => DaysUntil(e, date) == 0)
            .Select(e => e.Name.Trim())
            .Distinct()
            .ToList();
        if (todays.Count > 0)
        {
            return $"{TodayPrefix}{string.Join("、", todays)}";
        }

        // 没有人今天过生日：找最近的一次生日。
        var bestDays = int.MaxValue;
        var bestNames = new List<string>();
        foreach (var entry in list)
        {
            var days = DaysUntil(entry, date);
            if (days < bestDays)
            {
                bestDays = days;
                bestNames = new List<string> { entry.Name.Trim() };
            }
            else if (days == bestDays)
            {
                bestNames.Add(entry.Name.Trim());
            }
        }

        if (maxCountdownDays > 0 && bestDays > maxCountdownDays)
        {
            return null;
        }

        return $"{string.Join("、", bestNames)} 还有 {bestDays} 天过生日";
    }

    /// <summary>距离下一次生日的天数；今天就是生日时返回 0。</summary>
    public static int DaysUntil(BirthdayEntry entry, DateTime today)
        => (NextOccurrence(entry, today.Date) - today.Date).Days;

    /// <summary>下一次生日的公历日期（按公历或农历规则计算）。</summary>
    public static DateTime NextOccurrence(BirthdayEntry entry, DateTime today)
    {
        if (entry.IsLunar && entry.Year is > 0)
        {
            try
            {
                var (month, day) = LunarDateConverter.ToLunarRegular(new DateTime(entry.Year.Value, entry.Month, entry.Day));
                return LunarDateConverter.NextOccurrence(month, day, today.Date);
            }
            catch (Exception)
            {
                // 出生年份超出农历日历支持范围时退回公历规则。
            }
        }

        var next = CreateDate(today.Year, entry.Month, entry.Day);
        if (next < today.Date)
        {
            next = CreateDate(today.Year + 1, entry.Month, entry.Day);
        }

        return next;
    }

    /// <summary>判断一条记录是否能参与计算（月、日合法）。</summary>
    public static bool IsUsable(BirthdayEntry entry)
    {
        if (entry.Month is < 1 or > 12)
        {
            return false;
        }

        var year = entry.Year is > 0 ? entry.Year.Value : 2000;
        return entry.Day >= 1 && entry.Day <= DateTime.DaysInMonth(year, entry.Month);
    }

    /// <summary>构造某年某月某日；平年遇到 2/29 时按 2/28 处理。</summary>
    private static DateTime CreateDate(int year, int month, int day)
    {
        if (month == 2 && day == 29 && !DateTime.IsLeapYear(year))
        {
            day = 28;
        }

        return new DateTime(year, month, day);
    }
}
