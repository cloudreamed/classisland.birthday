using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BirthdayBoard.Models;

/// <summary>
/// 生日板块的组件设置。
/// </summary>
public class BirthdayComponentSettings : INotifyPropertyChanged
{
    private ObservableCollection<BirthdayEntry> _entries = new();
    private int _maxCountdownDays;

    /// <summary>生日名单。</summary>
    public ObservableCollection<BirthdayEntry> Entries
    {
        get => _entries;
        set
        {
            if (ReferenceEquals(value, _entries)) return;
            _entries = value ?? new ObservableCollection<BirthdayEntry>();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 倒计时上限（天）：最近一次生日超过这个天数时，整个板块隐藏。0 表示不限制。
    /// </summary>
    public int MaxCountdownDays
    {
        get => _maxCountdownDays;
        set
        {
            var normalized = value < 0 ? 0 : value;
            if (normalized == _maxCountdownDays) return;
            _maxCountdownDays = normalized;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
