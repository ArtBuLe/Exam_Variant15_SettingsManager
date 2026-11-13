using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.Win32;

namespace Variant15.Services;

public sealed class ProjectService
{
    // TODO 9: Реализовать миграцию настроек между версиями схемы.
    // TODO 10: Добавить тестовый режим без записи в реестр.
    // TODO 7: Вынести работу с настройками в DLL.
    public async Task RunAsync()
    {
        var settings = Load();
        Console.WriteLine($"[ЗАГРУЗКА] Тема: {settings.Theme}, Запусков: {settings.LaunchCount}, Схема: v{settings.SchemaVersion}, Последний запуск: {settings.LastRunUtc}");

        settings.Theme = settings.Theme == "Light" ? "Dark" : "Light";
        settings.LaunchCount++;
        settings.LastRunUtc = DateTime.UtcNow;

        Save(settings);

        var reloaded = Load();
        Console.WriteLine($"[ПОВТОР]  Тема: {reloaded.Theme}, Запусков: {reloaded.LaunchCount}, Схема: v{reloaded.SchemaVersion}, Последний запуск: {settings.LastRunUtc}");
        Console.WriteLine("\n[СБРОС] Удаляем ветку реестра...");
        Reset();

        var afterReset = Load();
        Console.WriteLine($"[ПОСЛЕ СБРОСА] Тема: {afterReset.Theme}, Запусков: {afterReset.LaunchCount}, Схема: v{afterReset.SchemaVersion}, Последний запуск: {settings.LastRunUtc}");
        
        Console.WriteLine("\nНажмите Enter для завершения.");
        Console.ReadLine();

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

                key.SetValue("LastRunUTC", settings.LastRunUtc.ToUniversalTime().Ticks, RegistryValueKind.QWord);

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

            var lastRunUTCValue = key.GetValue("LastRunUTC");
            if (lastRunUTCValue is long ticks)
            {
                settings.LastRunUtc = new DateTime(ticks, DateTimeKind.Utc);
            }
            else if (lastRunUTCValue != null)
            {
                Console.WriteLine($"[WARNING] LastRunUTC имеет неправильный тип ({lastRunUTCValue.GetType().Name}). Используется значение по умолчанию.");
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

        if (MigrateSettings(settings))
        {
            Console.WriteLine("[MIGRATION] Схема обновлена. Сохраняем новые настройки...");
            Save(settings); // Фиксируем мигрированные данные в реестре
        }

        return settings;
    }
    // TODO 6: Реализовать сброс только собственной ветки реестра.
    public void Reset()
    {
        Registry.CurrentUser.DeleteSubKeyTree(RegistryPath, throwOnMissingSubKey: false);
    }

    public bool MigrateSettings(Settings settings)
    {
        bool migrated = false;

        // ПРИМЕР
        // Миграция с v0 на v1:
        // В самой первой версии программы не было поля Schema_Version,
        // а тёмная тема называлась "Black".
        if (settings.SchemaVersion < 1)
        {
            Console.WriteLine("[MIGRATION] v0 -> v1: обновляем устаревшие значения...");

            if (settings.Theme == "Black")
            {
                settings.Theme = "Dark";
                Console.WriteLine($"[MIGRATION]   Theme: \"Black\" -> \"{settings.Theme}\"");
            }

            if (settings.LaunchCount < 0)
            {
                Console.WriteLine($"[MIGRATION]   LaunchCount: {settings.LaunchCount} -> 0 (исправление бага)");
                settings.LaunchCount = 0;
            }

            settings.SchemaVersion = 1;
            migrated = true;
        }

        // ПРИМЕР
        // Миграция с v1 на v2:
        // Допустим, в v2 мы решили, что тема "White" теперь называется "Light".
        if (settings.SchemaVersion < 2)
        {
            Console.WriteLine("[MIGRATION] v1 → v2: переименование тем...");

            if (settings.Theme == "White")
            {
                settings.Theme = "Light";
                Console.WriteLine($"[MIGRATION]   Theme: \"White\" ->\"{settings.Theme}\"");
            }

            settings.SchemaVersion = 2;
            migrated = true;
        }

        return migrated;
    }
}