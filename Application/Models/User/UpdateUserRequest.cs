using System.Text.Json;
using Infrastructure.Db.App.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Models.User;

public class UpdateUserRequest
{
    /// <summary>
    /// Временное поле для совместимости со Swagger/FastEndpoints.
    /// Будет заменено целевыми редактируемыми полями профиля.
    /// </summary>
    public string? Note { get; set; }
}
