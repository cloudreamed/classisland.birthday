using BirthdayBoard.Controls;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BirthdayBoard;

[PluginEntrance]
public class Plugin : PluginBase
{
    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        // 注册主界面「生日」组件，以及它的设置界面。
        services.AddComponent<BirthdayComponent, BirthdayComponentSettingsControl>();
    }
}
