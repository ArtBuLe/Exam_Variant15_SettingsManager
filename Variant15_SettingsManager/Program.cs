using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;

using Variant15Library.Models;
using Variant15Library.Services;

/*
 * (типо устный ответ)
 * 
 * 1. ОБЪЯСНЕНИЕ АРХИТЕКТУРЫ ПРОЕКТА 
 * 
 * Проект реализует паттерн разделения ответственности :
 * - Variant15Library (DLL): Содержит всю бизнес-логику. 
 *   Класс Settings описывает модель данных, а SettingsService инкапсулирует работу с реестром Windows, 
 *   миграцию схем и тестовый режим.
 * - Variant15_SettingsManager (Console App): 
 *   Точка входа. Отвечает только за взаимодействие с пользователем и вызовы методов из DLL.
 * 
 * Ключевые особенности:
 * - Тестовый режим: 
 *   При isTestMode = true операции перенаправляются в память, что позволяет тестировать код без прав администратора и засорения реестра.
 * - Миграция: 
 *   Метод MigrateSettings каскадно обновляет устаревшие настройки (v0 -> v1 -> v2) при первом запуске после обновления программы.
 * 
 * 
 * 2. ОПИСАНИЕ ИСПРАВЛЕННОГО ДЕФЕКТА
 * Дефект: 
 * Ошибка сохранения и чтения свойства LastRunUtc (DateTime).
 * Причина: 
 *   Изначально в проекте время сохранялось в формате double и записывалось в реестр как DWORD
 *   После чего при чтении программа ждала тип DateTime, из-за чего вылетали ошибки и исключения
 * Решение: 
 *   Время теперь сохраняется как long с использованием 
 *   типа QWord
 *   При чтении выполняется безопасное приведение: new DateTime(ticks, DateTimeKind.Utc). 
 */


namespace Variant15;

internal static class Program
{
    private static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Зачётный проект 15: Менеджер настроек приложения");
        Console.WriteLine(new string('=', 64));

        // Стартовый сценарий нужен для проверки структуры проекта.
        // Реализацию индивидуальных требований выполняйте в ProjectService.
        var service = new SettingsService();
        await service.RunAsync();

        var serveceTEST = new SettingsService(isTestMode:true);
        await serveceTEST.RunAsync();

        await RunAllTestsAsync();

        Console.WriteLine(new string('=', 75));
        Console.WriteLine("Тестирование завершено. Нажмите Enter для выхода.");
        Console.ReadLine();
    }

    private static void RunTest(string testName, Action testAction)
    {
        Console.Write($"Выполняется: {testName,-50} ... ");
        try
        {
            testAction();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[PASS]");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR]");
            Console.WriteLine($"       Детали: {ex.Message}");
            Console.ResetColor();
        }
    }
    private static async Task RunAllTestsAsync()
    {
        // ТЕСТ 1: Проверка настроек по умолчанию (первый запуск)
        RunTest("1. Загрузка настроек по умолчанию", () =>
        {
            var service = new SettingsService(isTestMode: true);
            var settings = service.Load();

            if (settings.Theme != "Dark") throw new Exception($"Ожидалось 'Dark', получено '{settings.Theme}'");
            if (settings.LaunchCount != 0) throw new Exception($"Ожидалось 0, получено {settings.LaunchCount}");
            if (settings.SchemaVersion != 2) throw new Exception($"Ожидалась схема v2, получена v{settings.SchemaVersion}");
        });

        // ТЕСТ 2: Цикл Сохранение -> Загрузка (проверка целостности данных)
        RunTest("2. Цикл Save() -> Load() (целостность данных)", () =>
        {
            var service = new SettingsService(isTestMode: true);
            var original = new Settings
            {
                Theme = "CustomTheme",
                LaunchCount = 42,
                SchemaVersion = 2,
                LastRunUtc = new DateTime(2023, 10, 25, 15, 30, 0, DateTimeKind.Utc)
            };

            service.Save(original);
            var loaded = service.Load();

            if (loaded.Theme != original.Theme) throw new Exception("Несовпадение Theme");
            if (loaded.LaunchCount != original.LaunchCount) throw new Exception("Несовпадение LaunchCount");
            if (loaded.LastRunUtc != original.LastRunUtc) throw new Exception("Несовпадение LastRunUtc (дефект времени!)");
        });

        // ТЕСТ 3: Проверка исправления дефекта времени (точность Ticks)
        RunTest("3. Корректность сохранения DateTime (QWord/Ticks)", () =>
        {
            var service = new SettingsService(isTestMode: true);
            // Используем дату с миллисекундами для проверки точности
            var preciseTime = new DateTime(2026, 11, 14, 12, 34, 56, 789, DateTimeKind.Utc);

            service.Save(new Settings { LastRunUtc = preciseTime });
            var loaded = service.Load();

            if (loaded.LastRunUtc.Ticks != preciseTime.Ticks)
            {
                throw new Exception($"Потеря точности времени! Ожидалось: {preciseTime.Ticks}, Получено: {loaded.LastRunUtc.Ticks}");
            }
        });

        // ТЕСТ 4: Проверка функции сброса (Reset)
        RunTest("4. Функция сброса настроек (Reset)", () =>
        {
            var service = new SettingsService(isTestMode: true);

            // 1. Сохраняем "мусорные" данные
            service.Save(new Settings { Theme = "Corrupted", LaunchCount = 999 });

            // 2. Сбрасываем
            service.Reset();

            // 3. Проверяем, что вернулись значения по умолчанию
            var afterReset = service.Load();
            if (afterReset.Theme != "Dark" || afterReset.LaunchCount != 0)
            {
                throw new Exception("После сброса настройки не вернулись к значениям по умолчанию");
            }
        });

        // ТЕСТ 5: Стресс-тест (множественные быстрые операции записи/чтения)
        RunTest("5. Стресс-тест (100 итераций Save/Load)", () =>
        {
            var service = new SettingsService(isTestMode: true);
            for (int i = 1; i <= 100; i++)
            {
                var temp = new Settings { LaunchCount = i, Theme = i % 2 == 0 ? "Light" : "Dark" };
                service.Save(temp);

                var loaded = service.Load();
                if (loaded.LaunchCount != i)
                {
                    throw new Exception($"Рассинхронизация на итерации {i}. Ожидалось: {i}, Получено: {loaded.LaunchCount}");
                }
            }
        });

        await Task.CompletedTask;
    }
}

