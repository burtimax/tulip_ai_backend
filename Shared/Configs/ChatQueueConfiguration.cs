using System.ComponentModel.DataAnnotations;

namespace Shared.Configs;

/// <summary>
/// Конфигурация очереди фоновой обработки сообщений чата.
/// </summary>
public sealed class ChatQueueConfiguration
{
    public const string Section = "ChatQueue";

    [Range(1, 300)]
    public int PollIntervalSeconds { get; set; } = 3;

    [Range(5, 3600)]
    public int LockTimeoutSeconds { get; set; } = 120;

    [Range(1, 20)]
    public int MaxAttempts { get; set; } = 3;

    [Range(1, 100)]
    public int BatchSize { get; set; } = 10;
}
