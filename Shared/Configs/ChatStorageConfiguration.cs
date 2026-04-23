using System.ComponentModel.DataAnnotations;

namespace Shared.Configs;

/// <summary>
/// Конфигурация хранения изображений чата и входных лимитов.
/// </summary>
public sealed class ChatStorageConfiguration
{
    public const string Section = "ChatStorage";

    [Required]
    public string Provider { get; set; } = "Local";

    [Range(1, 20)]
    public int MaxImagesPerMessage { get; set; } = 5;

    [Range(1, 50)]
    public int MaxImageSizeMb { get; set; } = 10;

    [MinLength(1)]
    public List<string> AllowedMimeTypes { get; set; } = new()
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };
}
