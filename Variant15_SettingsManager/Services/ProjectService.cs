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
        {
            // TODO 4: Обрабатывать ошибки доступа и повреждённые данные.
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(RegistryPath);

                if (key == null)
                {
                    throw new InvalidOperationException("Не удалось создать или открыть ключ реестра.");
                }

                key.SetValue("Theme", settings.Theme, RegistryValueKind.String);
                key.SetValue("LaunchCount", settings.LaunchCount, RegistryValueKind.DWord);
                key.SetValue("Schema_Version",settings.SchemaVersion, RegistryValueKind.DWord);

                Console.WriteLine("[INFO] Настройки успешно сохранены в реестр.");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"[ERROR] Нет прав на запись в реестр: {ex.Message}");
                Console.WriteLine("[INFO] Настройки не были сохранены.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка при сохранении настроек: {ex.Message}");
            }
        }
    }

    public Settings Load()
    {
        var settings = new Settings();

        // TODO 4: Обрабатывать ошибки доступа и повреждённые данные.
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath);

            if (key == null)
            {
                Console.WriteLine("[INFO] Ключ реестра не найден. Используются настройки по умолчанию.");
                return settings;
            }
            var themeValue = key.GetValue("Theme");
            if (themeValue is string themeStr)
            {
                settings.Theme = themeStr;
            }
            else if (themeValue != null)
            {
                Console.WriteLine($"[WARNING] Theme имеет неправильный тип ({themeValue.GetType().Name}). Используется значение по умолчанию.");
            }

            var launchCountValue = key.GetValue("LaunchCount");
            if (launchCountValue is int countInt)
            {
                settings.LaunchCount = countInt;
            }
            else if (launchCountValue != null)
            {
                Console.WriteLine($"[WARNING] LaunchCount имеет неправильный тип ({launchCountValue.GetType().Name}). Используется значение по умолчанию.");
            }

            var schemaVersionValue = key.GetValue("Schema_Version");
            if (schemaVersionValue is int schemaVersion)
            {
                settings.SchemaVersion = schemaVersion;
            }
            else if (schemaVersionValue != null)
            {
                Console.WriteLine($"[WARNING] Schema_Version имеет неправильный тип ({schemaVersionValue.GetType().Name}). Используется значение по умолчанию.");
            }

            Console.WriteLine("[INFO] Настройки успешно загружены из реестра.");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"[ERROR] Нет прав доступа к реестру: {ex.Message}");
            Console.WriteLine("[INFO] Используются настройки по умолчанию.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Неожиданная ошибка при чтении реестра: {ex.Message}");
            Console.WriteLine("[INFO] Используются настройки по умолчанию.");
        }

        return settings;
    }
}