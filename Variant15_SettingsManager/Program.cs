using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;


using Variant15Library;

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

        Console.WriteLine();
        Console.WriteLine("Нажмите Enter для завершения.");
        Console.ReadLine();
    }
}

