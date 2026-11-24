using System;
using System.Globalization;

namespace BirthdayBoard.Services;

/// <summary>
/// 公历与农历的换算。
/// 注意 .NET 的 <see cref="ChineseLunisolarCalendar"/> 里月份用 1-13 的“序号”，闰月会占用其中一个序号；
/// <c>GetLeapMonth(year)</c> 返回的正是这个序号（例如 2023 年返回 3，表示序号 3 是闰二月）。
/// </summary>
public static class LunarDateConverter
{
    private static readonly ChineseLunisolarCalendar Calendar = new();

    /// <summary>
    /// 把公历日期换算成「普通月 + 日」。闰月一律归到对应的普通月（如闰二月 → 二月）。
    /// </summary>
    public static (int Month, int Day) ToLunarRegular(DateTime solar)
    {
        var lunarYear = Calendar.GetYear(solar);
        var index = Calendar.GetMonth(solar);
        var day = Calendar.GetDayOfMonth(solar);
        var leap = Calendar.GetLeapMonth(lunarYear);
        var regularMonth = index == leap ? index - 1 : index;
        return (regularMonth, day);
    }

    /// <summary>
    /// 把「普通月 + 日」换算成指定农历年的公历日期；该月没有这一天时取当月最后一天。
    /// </summary>
    public static DateTime ToSolar(int lunarYear, int regularMonth, int day)
    {
        var leap = Calendar.GetLeapMonth(lunarYear);
        var index = leap == 0 || regularMonth < leap ? regularMonth : regularMonth + 1;
        var maxDay = Calendar.GetDaysInMonth(lunarYear, index);
        if (day > maxDay)
        {
            day = maxDay;
        }

        return Calendar.ToDateTime(lunarYear, index, day, 0, 0, 0, 0);
    }

    /// <summary>
    /// 返回下一个「农历（普通月 + 日）」对应的公历日期；今天就是该农历日时返回今天。
    /// </summary>
    public static DateTime NextOccurrence(int regularMonth, int day, DateTime today)
    {
        var lunarYear = Calendar.GetYear(today);
        var next = ToSolar(lunarYear, regularMonth, day);
        if (next.Date < today.Date)
        {
            next = ToSolar(lunarYear + 1, regularMonth, day);
        }

        return next.Date;
    }
}
