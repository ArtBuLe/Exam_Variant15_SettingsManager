namespace Variant15.Services;

public sealed class Settings
{
    // TODO 1: Создать настройки Theme и LaunchCount.
    // TODO 3: Задавать значения по умолчанию при отсутствии ключа или значения.
    public string Theme { get; set; } = "Dark";
    public int LaunchCount { get; set; } = 0;
    // TODO 5: Добавить версию схемы настроек.
    public int SchemaVersion { get; set; } = 1;
}