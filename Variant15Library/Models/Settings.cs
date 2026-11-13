using System;

namespace Variant15Library.Models;

public sealed class Settings
{
    public string Theme { get; set; } = "Dark";
    public int LaunchCount { get; set; } = 0;
    public int SchemaVersion { get; set; } = 2;
    public DateTime LastRunUtc { get; set; } = DateTime.UtcNow;
}