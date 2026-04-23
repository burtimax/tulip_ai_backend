namespace Shared.Configs;

/// <summary>Параметры фоновой обработки.</summary>
public sealed class ProcessorJobOptions
{
    public const string SectionName = "ProcessorJob";

    /// <summary>Сколько сканов могут одновременно проходить</summary>
    public int MaxParallelGenerations { get; set; } = 2;
}
