using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;

namespace Variant15.Services;

public sealed class ProjectService
{
    public async Task RunAsync()
    {
        // TODO: перенесите предметную логику в отдельные методы и классы.
        // TODO: не размещайте весь проект в Main.

        // TODO 1: Создать настройки Theme и LaunchCount.
        // TODO 2: Сохранять и читать настройки в HKCU\Software\StudentSystemProgramming.
        // TODO 3: Задавать значения по умолчанию при отсутствии ключа или значения.
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
}
