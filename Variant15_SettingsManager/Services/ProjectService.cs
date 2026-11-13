using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.Win32;

namespace Variant15.Services;

public sealed class ProjectService
{
    public async Task RunAsync()
    {
        // TODO: перенесите предметную логику в отдельные методы и классы.
        // TODO: не размещайте весь проект в Main.
        
        
        // TODO 4: Обрабатывать ошибки доступа и повреждённые данные.
        // TODO 5: Добавить версию схемы настроек.
        // TODO 6: Реализовать сброс только собственной ветки реестра.
        // TODO 7: Вынести работу с настройками в DLL.

        // TODO 8: Добавить LastRunUtc.
        // TODO 9: Реализовать миграцию настроек между версиями схемы.
        // TODO 10: Добавить тестовый режим без записи в реестр.

        Console.WriteLine("Стартовый проект собран. Реализуйте задания варианта.");
        await Task.CompletedTask;
    }
    private const string RegistryPath = @"Software\StudentSystemProgramming";

    // TODO 2: Сохранять и читать настройки в HKCU\Software\StudentSystemProgramming.
    public void Save(Settings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        using RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath, writable: true);

        if (settings.Theme != "Light" && settings.Theme != "Dark")
        {
            throw new ArgumentException("Недопустимая тема оформления.");
        }

        if (settings.LaunchCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings.LaunchCount));
        }

        key.SetValue("Theme", settings.Theme, RegistryValueKind.String);
        key.SetValue("LaunchCount", settings.LaunchCount, RegistryValueKind.DWord);
    }

    public Settings Load()
    {
        var settings = new Settings();

        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryPath);

        if (key is null)
        {
            return settings;
        }

        object? themeValue = key.GetValue("Theme");
        object? launchCountValue = key.GetValue("LaunchCount");

        if (themeValue is string theme && (theme == "Light" || theme == "Dark"))
        {
            settings.Theme = theme;
        }

        if (launchCountValue is int launchCount && launchCount >= 0)
        {
            settings.LaunchCount = launchCount;
        }

        return settings;
    }
}