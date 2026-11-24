using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BirthdayBoard.Models;

namespace BirthdayBoard.Services;

/// <summary>
/// 解析从 txt 文件导入的生日名单。
/// 新格式：`姓名 2008年3月6日 0`（0 = 公历过生日，1 = 农历过生日），也兼容 `2008-3-6 0`、`2008/3/6 0`。
/// 旧格式：`姓名 3月6日`（也兼容 `3-6`、`3/6`、`3.6`），按公历、年份未知导入。
/// </summary>
public static class BirthdayImportParser
{
    private static readonly Regex NewPattern = new(
        @"^(?<name>.+?)\s+(?<year>\d{4})\s*[年\-/.]\s*(?<month>\d{1,2})\s*[月\-/.]\s*(?<day>\d{1,2})\s*日?\s*(?<flag>[01])?\s*$",
        RegexOptions.Compiled);

    private static readonly Regex OldPattern = new(
        @"^(?<name>.+?)\s+(?<month>\d{1,2})\s*(?:月|[-/.])\s*(?<day>\d{1,2})\s*日?\s*$",
        RegexOptions.Compiled);

    /// <summary>逐行解析，返回所有合法的生日记录；无法识别的行会被跳过。</summary>
    public static IReadOnlyList<BirthdayEntry> Parse(string? text)
    {
        var result = new List<BirthdayEntry>();
        if (string.IsNullOrWhiteSpace(text))
        {
            return result;
        }

        foreach (var raw in text.Split('\n'))
        {
            if (TryParseLine(raw, out var entry))
            {
                result.Add(entry);
            }
        }

        return result;
    }

    /// <summary>解析一行。</summary>
    public static bool TryParseLine(string? line, out BirthdayEntry entry)
    {
        entry = null!;
        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        var text = line.Trim().TrimStart('\uFEFF');
        if (text.Length == 0)
        {
            return false;
        }

        var match = NewPattern.Match(text);
        var hasYear = match.Success;
        if (!hasYear)
        {
            match = OldPattern.Match(text);
            if (!match.Success)
            {
                return false;
            }
        }

        var name = match.Groups["name"].Value.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        if (!int.TryParse(match.Groups["month"].Value, out var month) || month is < 1 or > 12)
        {
            return false;
        }

        if (!int.TryParse(match.Groups["day"].Value, out var day) || day < 1)
        {
            return false;
        }

        int? year = null;
        if (hasYear)
        {
            if (!int.TryParse(match.Groups["year"].Value, out var parsedYear) || parsedYear is < 1900 or > 2100)
            {
                return false;
            }

            year = parsedYear;
        }

        // 用闰年校验天数，使 2/29 合法。
        var checkYear = year ?? 2000;
        if (day > DateTime.DaysInMonth(checkYear, month))
        {
            return false;
        }

        var isLunar = hasYear && match.Groups["flag"].Success && match.Groups["flag"].Value == "1";
        entry = new BirthdayEntry
        {
            Name = name,
            Year = year,
            Month = month,
            Day = day,
            IsLunar = isLunar
        };
        return true;
    }
}
